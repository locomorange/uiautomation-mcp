using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// Common timeout management service for all worker subprocesses
    /// Provides centralized process lifecycle and timeout handling
    /// 
    /// Usage examples:
    /// - UIAutomationWorker: UI automation operations
    /// - Future workers: File processing, data analysis, etc.
    /// </summary>
    public interface IProcessTimeoutManager
    {
        /// <summary>
        /// Executes a worker process with timeout management
        /// </summary>
        /// <param name="processStartInfo">Process configuration</param>
        /// <param name="inputData">Data to send to process stdin</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <param name="operationName">Name for logging purposes</param>
        /// <returns>Process execution result</returns>
        Task<ProcessExecutionResult> ExecuteWithTimeoutAsync(
            ProcessStartInfo processStartInfo,
            string inputData,
            int timeoutSeconds,
            string operationName);

        /// <summary>
        /// Kills a process and all its child processes
        /// </summary>
        /// <param name="process">Process to kill</param>
        /// <param name="operationName">Operation name for logging</param>
        void KillProcessTree(Process process, string operationName);
    }

    /// <summary>
    /// Result of process execution with timeout management
    /// </summary>
    public class ProcessExecutionResult
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
        public int ExitCode { get; set; }
        public bool TimedOut { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    public class ProcessTimeoutManager : IProcessTimeoutManager
    {
        private readonly ILogger<ProcessTimeoutManager> _logger;
        private static long _processCounter = 0;

        public ProcessTimeoutManager(ILogger<ProcessTimeoutManager> logger)
        {
            _logger = logger;
        }

        public async Task<ProcessExecutionResult> ExecuteWithTimeoutAsync(
            ProcessStartInfo processStartInfo,
            string inputData,
            int timeoutSeconds,
            string operationName)
        {
            var startTime = DateTime.UtcNow;
            var processId = Interlocked.Increment(ref _processCounter);
            Process? process = null;

            _logger.LogInformation("[ProcessTimeoutManager] Starting {Operation}#{ProcessId} with {Timeout}s timeout", 
                operationName, processId, timeoutSeconds);

            try
            {
                // Validate process executable exists
                if (!File.Exists(processStartInfo.FileName))
                {
                    _logger.LogError("[ProcessTimeoutManager] Executable not found: {FileName}", processStartInfo.FileName);
                    return new ProcessExecutionResult
                    {
                        Success = false,
                        Error = $"Executable not found: {processStartInfo.FileName}",
                        ExecutionTime = DateTime.UtcNow - startTime
                    };
                }

                // Configure process for proper subprocess execution
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;

                process = new Process { StartInfo = processStartInfo };

                _logger.LogInformation("[ProcessTimeoutManager] Launching {Operation}#{ProcessId}: {FileName} (PID: will be assigned)", 
                    operationName, processId, processStartInfo.FileName);
                process.Start();
                
                _logger.LogDebug("[ProcessTimeoutManager] {Operation}#{ProcessId} started with PID: {PID}", 
                    operationName, processId, process.Id);

                // Send input data to process
                await process.StandardInput.WriteLineAsync(inputData);
                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();

                // Wait for completion with centralized timeout management
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var processTask = process.WaitForExitAsync(cts.Token);

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

                var executionTime = DateTime.UtcNow - startTime;

                if (!completedInTime)
                {
                    _logger.LogWarning("[ProcessTimeoutManager] {Operation}#{ProcessId} timeout after {Timeout}s - Terminating process (PID: {PID})", 
                        operationName, processId, timeoutSeconds, process.Id);

                    KillProcessTree(process, $"{operationName}#{processId}");

                    return new ProcessExecutionResult
                    {
                        Success = false,
                        Error = $"{operationName} timeout after {timeoutSeconds} seconds. Process was terminated.",
                        TimedOut = true,
                        ExecutionTime = executionTime
                    };
                }

                // Read process output
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                var success = process.ExitCode == 0;
                if (!success)
                {
                    _logger.LogError("[ProcessTimeoutManager] {Operation}#{ProcessId} failed with exit code {ExitCode} in {ElapsedMs}ms. Error: {Error}", 
                        operationName, processId, process.ExitCode, executionTime.TotalMilliseconds, error);
                }
                else
                {
                    _logger.LogInformation("[ProcessTimeoutManager] {Operation}#{ProcessId} completed successfully in {ElapsedMs}ms", 
                        operationName, processId, executionTime.TotalMilliseconds);
                }

                return new ProcessExecutionResult
                {
                    Success = success,
                    Output = output,
                    Error = error,
                    ExitCode = process.ExitCode,
                    TimedOut = false,
                    ExecutionTime = executionTime
                };
            }
            catch (Exception ex)
            {
                var executionTime = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "[ProcessTimeoutManager] {Operation}#{ProcessId} execution failed after {ElapsedMs}ms", 
                    operationName, processId, executionTime.TotalMilliseconds);
                
                return new ProcessExecutionResult
                {
                    Success = false,
                    Error = $"{operationName} execution failed: {ex.Message}",
                    ExecutionTime = executionTime
                };
            }
            finally
            {
                await CleanupProcessAsync(process, operationName);
            }
        }

        public void KillProcessTree(Process process, string operationName)
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    _logger.LogWarning("[ProcessTimeoutManager] Killing process tree for {Operation} (PID: {PID})", 
                        operationName, process.Id);
                    process.Kill(entireProcessTree: true);
                    _logger.LogDebug("[ProcessTimeoutManager] Process tree killed for {Operation}", operationName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProcessTimeoutManager] Failed to kill process tree for {Operation}", operationName);
            }
        }

        private async Task CleanupProcessAsync(Process? process, string operationName)
        {
            if (process == null) return;

            try
            {
                // Give the process a moment to exit gracefully
                if (!process.HasExited)
                {
                    await Task.Delay(100);
                }

                // Force kill if still running
                if (!process.HasExited)
                {
                    _logger.LogWarning("[ProcessTimeoutManager] Process still running during cleanup - Terminating {Operation} (PID: {PID})", 
                        operationName, process.Id);
                    KillProcessTree(process, operationName);
                }

                process.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ProcessTimeoutManager] Error during process cleanup for {Operation}", operationName);
            }
        }
    }
}