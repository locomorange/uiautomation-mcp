using System.Diagnostics;
using System.Text.Json;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// Worker service for executing UI Automation operations in a separate process
    /// to prevent main process from hanging due to COM/native API blocking
    /// </summary>
    public interface IUIAutomationWorker
    {
        Task<OperationResult<AutomationElement?>> FindFirstInProcessAsync(
            string searchRootInfo,
            TreeScope scope,
            string conditionJson,
            int timeoutSeconds = 10);
            
        Task<OperationResult<string>> ExecuteInProcessAsync(
            string operationJson,
            int timeoutSeconds = 10);
    }

    public class UIAutomationWorker : IUIAutomationWorker
    {
        private readonly ILogger<UIAutomationWorker> _logger;
        private readonly string _workerExecutablePath;

        public UIAutomationWorker(ILogger<UIAutomationWorker> logger)
        {
            _logger = logger;
            // Worker executable will be in the same directory as the main application
            _workerExecutablePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                "UiAutomationWorker.exe");
        }

        public async Task<OperationResult<AutomationElement?>> FindFirstInProcessAsync(
            string searchRootInfo,
            TreeScope scope,
            string conditionJson,
            int timeoutSeconds = 10)
        {
            _logger.LogInformation("[UIAutomationWorker] Starting FindFirst operation in subprocess, timeout: {Timeout}s", timeoutSeconds);

            try
            {
                var operationData = new
                {
                    Operation = "FindFirst",
                    SearchRoot = searchRootInfo,
                    Scope = scope.ToString(),
                    Condition = conditionJson,
                    Timeout = timeoutSeconds
                };

                var result = await ExecuteWorkerProcessAsync(
                    JsonSerializer.Serialize(operationData),
                    timeoutSeconds + 2); // Add buffer for process overhead

                if (!result.Success)
                {
                    return new OperationResult<AutomationElement?>
                    {
                        Success = false,
                        Error = result.Error
                    };
                }

                // Parse result and reconstruct AutomationElement if needed
                // Note: AutomationElement cannot be directly serialized across processes
                // We'll need to return element identifiers instead
                return new OperationResult<AutomationElement?>
                {
                    Success = true,
                    Data = null, // Will be populated by element reconstruction logic
                    Error = "Subprocess execution completed - element reconstruction needed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationWorker] FindFirstInProcess failed");
                return new OperationResult<AutomationElement?>
                {
                    Success = false,
                    Error = $"Subprocess execution failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<string>> ExecuteInProcessAsync(
            string operationJson,
            int timeoutSeconds = 10)
        {
            _logger.LogInformation("[UIAutomationWorker] Executing operation in subprocess, timeout: {Timeout}s", timeoutSeconds);

            try
            {
                return await ExecuteWorkerProcessAsync(operationJson, timeoutSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationWorker] ExecuteInProcess failed");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Subprocess execution failed: {ex.Message}"
                };
            }
        }

        private async Task<OperationResult<string>> ExecuteWorkerProcessAsync(
            string inputJson,
            int timeoutSeconds)
        {
            Process? workerProcess = null;
            
            try
            {
                // Check if worker executable exists
                if (!System.IO.File.Exists(_workerExecutablePath))
                {
                    _logger.LogError("[UIAutomationWorker] Worker executable not found: {Path}", _workerExecutablePath);
                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = $"Worker executable not found: {_workerExecutablePath}"
                    };
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _workerExecutablePath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                workerProcess = new Process { StartInfo = processStartInfo };
                
                _logger.LogInformation("[UIAutomationWorker] Starting worker process: {Path}", _workerExecutablePath);
                workerProcess.Start();

                // Send input data
                await workerProcess.StandardInput.WriteLineAsync(inputJson);
                await workerProcess.StandardInput.FlushAsync();
                workerProcess.StandardInput.Close();

                // Wait for completion with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var processTask = workerProcess.WaitForExitAsync(cts.Token);
                
                bool completedInTime;
                try
                {
                    await processTask;
                    completedInTime = true;
                }
                catch (OperationCanceledException)
                {
                    completedInTime = false;
                }
                
                if (!completedInTime)
                {
                    _logger.LogWarning("[UIAutomationWorker] Worker process timeout after {Timeout}s, killing process", timeoutSeconds);
                    
                    try
                    {
                        workerProcess.Kill(entireProcessTree: true);
                    }
                    catch (Exception killEx)
                    {
                        _logger.LogError(killEx, "[UIAutomationWorker] Failed to kill worker process");
                    }

                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = $"Worker process timeout after {timeoutSeconds} seconds. UI Automation operation was forcibly terminated."
                    };
                }

                // Read results
                var output = await workerProcess.StandardOutput.ReadToEndAsync();
                var error = await workerProcess.StandardError.ReadToEndAsync();

                if (workerProcess.ExitCode != 0)
                {
                    _logger.LogError("[UIAutomationWorker] Worker process failed with exit code {ExitCode}. Error: {Error}", 
                        workerProcess.ExitCode, error);
                    
                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = $"Worker process failed (exit code {workerProcess.ExitCode}): {error}"
                    };
                }

                _logger.LogInformation("[UIAutomationWorker] Worker process completed successfully");
                return new OperationResult<string> { Success = true, Data = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationWorker] Worker process execution failed");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Worker process execution failed: {ex.Message}"
                };
            }
            finally
            {
                try
                {
                    workerProcess?.Kill(entireProcessTree: true);
                    workerProcess?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[UIAutomationWorker] Error disposing worker process");
                }
            }
        }
    }

    /// <summary>
    /// Data models for worker process communication
    /// </summary>
    public class WorkerOperation
    {
        public string Operation { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public int Timeout { get; set; } = 10;
    }

    public class WorkerResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
