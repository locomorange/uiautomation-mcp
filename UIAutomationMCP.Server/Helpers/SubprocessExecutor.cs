using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace UIAutomationMCP.Server.Helpers
{
    public class SubprocessExecutor : IDisposable
    {
        private readonly ILogger<SubprocessExecutor> _logger;
        private readonly string _workerPath;
        private Process? _workerProcess;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;

        public SubprocessExecutor(ILogger<SubprocessExecutor> logger, string workerPath)
        {
            _logger = logger;
            _workerPath = workerPath;
        }

        public async Task<TResult> ExecuteAsync<TResult>(string operation, object? parameters = null, int timeoutSeconds = 30)
        {
            await _semaphore.WaitAsync();
            try
            {
                await EnsureWorkerProcessAsync();
                
                var request = new WorkerRequest
                {
                    Operation = operation,
                    Parameters = parameters as Dictionary<string, object>
                };

                var requestJson = JsonSerializer.Serialize(request);
                _logger.LogDebug("Sending request to worker: {Request}", requestJson);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                await _workerProcess!.StandardInput.WriteLineAsync(requestJson);
                await _workerProcess.StandardInput.FlushAsync();

                var responseTask = _workerProcess.StandardOutput.ReadLineAsync();
                var completedTask = await Task.WhenAny(responseTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (completedTask != responseTask)
                {
                    _logger.LogWarning("Worker operation timed out: {Operation}", operation);
                    await RestartWorkerProcessAsync();
                    throw new TimeoutException($"Worker operation '{operation}' timed out after {timeoutSeconds} seconds");
                }

                var responseJson = await responseTask;
                if (string.IsNullOrEmpty(responseJson))
                {
                    throw new InvalidOperationException("Worker process returned empty response");
                }

                _logger.LogDebug("Received response from worker: {Response}", responseJson);

                var response = JsonSerializer.Deserialize<WorkerResponse>(responseJson);
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to deserialize worker response");
                }

                if (!response.Success)
                {
                    throw new InvalidOperationException($"Worker operation failed: {response.Error}");
                }

                return JsonSerializer.Deserialize<TResult>(JsonSerializer.Serialize(response.Data))!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task EnsureWorkerProcessAsync()
        {
            if (_workerProcess == null || _workerProcess.HasExited)
            {
                await StartWorkerProcessAsync();
            }
        }

        private async Task StartWorkerProcessAsync()
        {
            _logger.LogInformation("Starting worker process: {WorkerPath}", _workerPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = _workerPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _workerProcess = new Process { StartInfo = startInfo };
            _workerProcess.Start();

            // Wait a bit for the process to start
            await Task.Delay(100);

            if (_workerProcess.HasExited)
            {
                var exitCode = _workerProcess.ExitCode;
                _logger.LogError("Worker process exited immediately with code: {ExitCode}", exitCode);
                throw new InvalidOperationException($"Worker process failed to start (exit code: {exitCode})");
            }

            _logger.LogInformation("Worker process started with PID: {ProcessId}", _workerProcess.Id);
        }

        private async Task RestartWorkerProcessAsync()
        {
            _logger.LogInformation("Restarting worker process");
            
            if (_workerProcess != null && !_workerProcess.HasExited)
            {
                try
                {
                    _workerProcess.Kill();
                    await _workerProcess.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to kill worker process");
                }
                finally
                {
                    _workerProcess.Dispose();
                    _workerProcess = null;
                }
            }

            await StartWorkerProcessAsync();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("Disposing SubprocessExecutor");

            if (_workerProcess != null && !_workerProcess.HasExited)
            {
                try
                {
                    _workerProcess.Kill();
                    _workerProcess.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to kill worker process during disposal");
                }
                finally
                {
                    _workerProcess.Dispose();
                }
            }

            _semaphore.Dispose();
            _disposed = true;
        }
    }

    public class WorkerRequest
    {
        public string Operation { get; set; } = "";
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class WorkerResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
    }
}