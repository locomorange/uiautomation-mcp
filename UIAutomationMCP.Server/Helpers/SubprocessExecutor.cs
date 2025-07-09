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
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cts.Token);
                var completedTask = await Task.WhenAny(responseTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logger.LogWarning("Worker operation timed out after {TimeoutSeconds}s: {Operation}", timeoutSeconds, operation);
                    
                    // Give the process a chance to respond to cancellation
                    cts.Cancel();
                    
                    // Wait a short time for graceful shutdown
                    var gracefulShutdownTask = Task.Delay(TimeSpan.FromSeconds(2));
                    var shutdownCompleted = await Task.WhenAny(responseTask, gracefulShutdownTask);
                    
                    if (shutdownCompleted == gracefulShutdownTask)
                    {
                        _logger.LogWarning("Worker process did not respond to cancellation, forcing restart");
                        await RestartWorkerProcessAsync();
                    }
                    
                    throw new TimeoutException($"Worker operation '{operation}' timed out after {timeoutSeconds} seconds");
                }

                var responseJson = await responseTask;
                if (string.IsNullOrEmpty(responseJson))
                {
                    var contextMessage = $"Worker process returned empty response for operation '{operation}'";
                    if (parameters != null)
                    {
                        contextMessage += $" with parameters: {JsonSerializer.Serialize(parameters)}";
                    }
                    _logger.LogError("Empty response received: {Context}", contextMessage);
                    throw new InvalidOperationException(contextMessage);
                }

                _logger.LogDebug("Received response from worker (length: {Length}): {Response}", 
                    responseJson.Length, 
                    responseJson.Length > 500 ? responseJson.Substring(0, 500) + "..." : responseJson);

                WorkerResponse? response;
                try
                {
                    response = JsonSerializer.Deserialize<WorkerResponse>(responseJson);
                }
                catch (JsonException ex)
                {
                    var errorMessage = $"Failed to deserialize worker response for operation '{operation}'. Response: {responseJson}";
                    _logger.LogError(ex, "JSON deserialization failed: {ErrorMessage}", errorMessage);
                    throw new InvalidOperationException(errorMessage, ex);
                }
                
                if (response == null)
                {
                    var errorMessage = $"Deserialized response is null for operation '{operation}'. Raw response: {responseJson}";
                    _logger.LogError("Null response after deserialization: {ErrorMessage}", errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                if (!response.Success)
                {
                    // Enhanced error propagation with operation context
                    var errorDetails = new
                    {
                        Operation = operation,
                        Parameters = parameters,
                        Error = response.Error,
                        Timestamp = DateTime.UtcNow,
                        WorkerPid = _workerProcess?.Id
                    };
                    
                    var contextualErrorMessage = $"Worker operation '{operation}' failed: {response.Error}";
                    if (parameters != null)
                    {
                        contextualErrorMessage += $" (Parameters: {JsonSerializer.Serialize(parameters)})";
                    }
                    
                    _logger.LogError("Worker operation failed with details: {@ErrorDetails}", errorDetails);
                    
                    // Categorize errors for appropriate exception types
                    var errorLower = response.Error?.ToLowerInvariant() ?? "";
                    
                    if (errorLower.Contains("not found") || errorLower.Contains("element") && errorLower.Contains("not") ||
                        errorLower.Contains("invalid") && errorLower.Contains("id"))
                    {
                        throw new ArgumentException(contextualErrorMessage);
                    }
                    else if (errorLower.Contains("not supported") || errorLower.Contains("pattern") && errorLower.Contains("not") ||
                             errorLower.Contains("control") && errorLower.Contains("not"))
                    {
                        throw new NotSupportedException(contextualErrorMessage);
                    }
                    else if (errorLower.Contains("read-only") || errorLower.Contains("access") && errorLower.Contains("denied") ||
                             errorLower.Contains("permission") || errorLower.Contains("unauthorized"))
                    {
                        throw new UnauthorizedAccessException(contextualErrorMessage);
                    }
                    else if (errorLower.Contains("timeout") || errorLower.Contains("time") && errorLower.Contains("out"))
                    {
                        throw new TimeoutException(contextualErrorMessage);
                    }
                    else
                    {
                        throw new InvalidOperationException(contextualErrorMessage);
                    }
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
            
            if (_workerProcess != null)
            {
                try
                {
                    if (!_workerProcess.HasExited)
                    {
                        _logger.LogDebug("Attempting graceful shutdown of worker process PID: {ProcessId}", _workerProcess.Id);
                        
                        // Try graceful shutdown first by closing stdin
                        try
                        {
                            _workerProcess.StandardInput.Close();
                            
                            // Wait up to 3 seconds for graceful exit
                            if (!_workerProcess.WaitForExit(3000))
                            {
                                _logger.LogWarning("Worker process did not exit gracefully, forcing termination");
                                _workerProcess.Kill();
                                
                                // Wait up to 2 more seconds for forced termination
                                if (!_workerProcess.WaitForExit(2000))
                                {
                                    _logger.LogError("Worker process did not respond to Kill signal");
                                }
                            }
                            else
                            {
                                _logger.LogDebug("Worker process exited gracefully with code: {ExitCode}", _workerProcess.ExitCode);
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            // Process may have already exited
                            _logger.LogDebug("Worker process already exited during graceful shutdown attempt");
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Worker process was already exited with code: {ExitCode}", _workerProcess.ExitCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Exception during worker process shutdown");
                }
                finally
                {
                    _workerProcess.Dispose();
                    _workerProcess = null;
                }
            }

            // Add a small delay before restart to prevent rapid restart loops
            await Task.Delay(100);
            
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
