using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Shared;

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

        public async Task<TResult> ExecuteAsync<TResult>(string operation, object? parameters = null, int timeoutSeconds = 60)
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
                _logger.LogInformation("Sending request to worker: {Request}", requestJson);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                await _workerProcess!.StandardInput.WriteLineAsync(requestJson);
                await _workerProcess.StandardInput.FlushAsync();
                _logger.LogInformation("Request sent and flushed to worker process");

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
                    // Check stderr for error output
                    string errorOutput = "";
                    try
                    {
                        if (_workerProcess?.StandardError != null && _workerProcess.StandardError.Peek() >= 0)
                        {
                            errorOutput = await _workerProcess.StandardError.ReadToEndAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to read stderr from worker process");
                    }

                    var contextMessage = $"Worker process returned empty response for operation '{operation}'";
                    if (parameters != null)
                    {
                        contextMessage += $" with parameters: {JsonSerializer.Serialize(parameters)}";
                    }
                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        contextMessage += $". Stderr: {errorOutput}";
                    }
                    _logger.LogError("Empty response received: {Context}", contextMessage);
                    throw new InvalidOperationException(contextMessage);
                }

                _logger.LogInformation("Received response from worker (length: {Length}): {Response}", 
                    responseJson.Length, 
                    responseJson.Length > 1000 ? responseJson.Substring(0, 1000) + "..." : responseJson);

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
                        ErrorIsNull = response.Error == null,
                        ErrorIsEmpty = string.IsNullOrEmpty(response.Error),
                        Data = response.Data,
                        Timestamp = DateTime.UtcNow,
                        WorkerPid = _workerProcess?.Id
                    };
                    
                    // Fix empty error message handling
                    var errorMessage = string.IsNullOrEmpty(response.Error) ? 
                        "Unknown error occurred" : response.Error;
                    
                    var contextualErrorMessage = $"Worker operation '{operation}' failed: {errorMessage}";
                    if (parameters != null)
                    {
                        contextualErrorMessage += $" (Parameters: {JsonSerializer.Serialize(parameters)})";
                    }
                    
                    _logger.LogError("Worker operation failed with details: {@ErrorDetails}", errorDetails);
                    
                    // Categorize errors for appropriate exception types
                    var errorLower = errorMessage.ToLowerInvariant();
                    
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

                try
                {
                    var result = JsonSerializer.Deserialize<TResult>(JsonSerializer.Serialize(response.Data))!;
                    _logger.LogInformation("Successfully deserialized worker response data to type {ResultType}", typeof(TResult).Name);
                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize response data to type {ResultType}. Data: {Data}", 
                        typeof(TResult).Name, JsonSerializer.Serialize(response.Data));
                    throw new InvalidOperationException($"Failed to deserialize response data to {typeof(TResult).Name}: {ex.Message}", ex);
                }
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

            // Check if it's a .dll file or executable
            ProcessStartInfo startInfo;
            
            if (_workerPath.EndsWith(".dll"))
            {
                // For .dll files, use dotnet to run them
                startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = _workerPath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_workerPath)
                };
            }
            else if (_workerPath.Contains("UIAutomationMCP.Worker") && !File.Exists(_workerPath))
            {
                // For project directory, use dotnet run
                var projectDir = Path.GetDirectoryName(_workerPath);
                if (projectDir != null && Directory.Exists(projectDir))
                {
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run --project UIAutomationMCP.Worker",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Directory.GetParent(projectDir)?.FullName ?? projectDir
                    };
                }
                else
                {
                    _logger.LogError("Worker project directory not found: {WorkerPath}", _workerPath);
                    throw new FileNotFoundException($"Worker project directory not found: {_workerPath}");
                }
            }
            else
            {
                // For executable files
                if (!File.Exists(_workerPath))
                {
                    _logger.LogError("Worker executable not found at: {WorkerPath}", _workerPath);
                    throw new FileNotFoundException($"Worker executable not found at: {_workerPath}");
                }

                startInfo = new ProcessStartInfo
                {
                    FileName = _workerPath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_workerPath)
                };
            }

            _workerProcess = new Process { StartInfo = startInfo };
            
            // Set up async stderr monitoring
            _workerProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogDebug("Worker stderr: {ErrorData}", e.Data);
                }
            };
            
            try
            {
                _workerProcess.Start();
                _workerProcess.BeginErrorReadLine(); // Start async stderr reading
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start worker process: {WorkerPath}", _workerPath);
                throw new InvalidOperationException($"Failed to start worker process: {_workerPath}", ex);
            }

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
}
