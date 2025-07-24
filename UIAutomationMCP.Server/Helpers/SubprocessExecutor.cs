using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Server.Interfaces;

namespace UIAutomationMCP.Server.Helpers
{
    public class SubprocessExecutor : ISubprocessExecutor, IOperationExecutor, IDisposable
    {
        private readonly ILogger<SubprocessExecutor> _logger;
        private readonly string _workerPath;
        private Process? _workerProcess;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;
        private readonly object _lockObject = new object();
        private string _lastStderrOutput = "";
        private string _workerExecutablePath = "";

        public SubprocessExecutor(ILogger<SubprocessExecutor> logger, string workerPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workerPath = !string.IsNullOrWhiteSpace(workerPath) ? workerPath : throw new ArgumentException("Worker path cannot be null or empty", nameof(workerPath));
        }

        /// <summary>
        /// Type-safe unified execute method - eliminates type branching and legacy support
        /// </summary>
        /// <typeparam name="TRequest">The request type</typeparam>
        /// <typeparam name="TResult">The expected result type</typeparam>
        /// <param name="operation">The operation name</param>
        /// <param name="request">The typed request object</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>The operation result</returns>
        public async Task<TResult> ExecuteAsync<TRequest, TResult>(string operation, TRequest request, int timeoutSeconds = 60) 
            where TRequest : notnull 
            where TResult : notnull
        {
            try
            {
                _logger.LogInformation($"[SubprocessExecutor] ExecuteAsync started - Operation: {operation}, RequestType: {typeof(TRequest).Name}, ResultType: {typeof(TResult).Name}, TimeoutSeconds: {timeoutSeconds}");
                
                if (_disposed)
                {
                    _logger.LogError($"[SubprocessExecutor] SubprocessExecutor is disposed");
                    throw new ObjectDisposedException(nameof(SubprocessExecutor));
                }
                
                if (string.IsNullOrWhiteSpace(operation))
                {
                    _logger.LogError($"[SubprocessExecutor] Operation is null or empty");
                    throw new ArgumentException("Operation cannot be null or empty", nameof(operation));
                }
                
                if (timeoutSeconds <= 0)
                {
                    _logger.LogError($"[SubprocessExecutor] Invalid timeout: {timeoutSeconds}");
                    throw new ArgumentException("Timeout must be greater than zero", nameof(timeoutSeconds));
                }
                
                _logger.LogInformation($"[SubprocessExecutor] Waiting for semaphore...");
                await _semaphore.WaitAsync();
                try
                {
                    _logger.LogInformation($"[SubprocessExecutor] Semaphore acquired. Ensuring worker process...");
                    await EnsureWorkerProcessAsync();
                
                    var operationStartTime = DateTime.UtcNow;
                    // Direct type-safe serialization - no branching needed
                    string requestJson = JsonSerializationHelper.Serialize(request);
                    _logger.LogInformation($"[SubprocessExecutor] Request serialized. Length: {requestJson.Length} chars");
                    _logger.LogInformation("Sending request to worker at {StartTime}: {Request}", operationStartTime, requestJson);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                    
                    _logger.LogInformation($"[SubprocessExecutor] Writing request to StandardInput...");
                    await _workerProcess!.StandardInput.WriteLineAsync(requestJson);
                    await _workerProcess.StandardInput.FlushAsync();
                    _logger.LogInformation("Request sent and flushed to worker process");

                    var responseTask = _workerProcess.StandardOutput.ReadLineAsync();
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cts.Token);
                    var completedTask = await Task.WhenAny(responseTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        _logger.LogWarning("Worker operation timed out after {TimeoutSeconds}s: {Operation}. Allowing additional grace time...", timeoutSeconds, operation);
                        
                        // Give additional grace time for slow operations
                        var gracePeriod = Math.Min(timeoutSeconds / 2, 10); // Up to 10 seconds additional
                        var extendedTimeoutTask = Task.Delay(TimeSpan.FromSeconds(gracePeriod));
                        var extendedCompletedTask = await Task.WhenAny(responseTask, extendedTimeoutTask);
                        
                        if (extendedCompletedTask == extendedTimeoutTask)
                        {
                            _logger.LogWarning("Worker operation still timed out after grace period of {GracePeriod}s: {Operation}", gracePeriod, operation);
                            
                            // Advanced hang vs short timeout diagnosis
                            var actualElapsed = (DateTime.UtcNow - operationStartTime).TotalSeconds;
                            var totalTime = timeoutSeconds + gracePeriod;
                            bool processAlive = _workerProcess?.HasExited == false;
                            
                            if (!processAlive)
                            {
                                _logger.LogWarning("DIAGNOSIS: HANG - Worker process crashed or terminated after {Elapsed:F1}s", actualElapsed);
                            }
                            else
                            {
                                // Simple diagnosis based purely on process responsiveness
                                bool processResponding = true;
                                try
                                {
                                    processResponding = _workerProcess!.Responding;
                                }
                                catch
                                {
                                    processResponding = false;
                                }

                                if (processResponding)
                                {
                                    _logger.LogWarning("DIAGNOSIS: SHORT TIMEOUT - Process is responsive, just need more time. Elapsed: {Elapsed:F1}s, Consider increasing timeout", actualElapsed);
                                }
                                else
                                {
                                    _logger.LogWarning("DIAGNOSIS: HANG - Process alive but not responding after {Elapsed:F1}s. Likely deadlock or infinite loop", actualElapsed);
                                }
                            }
                            
                            cts.Cancel();
                            
                            var gracefulShutdownTask = Task.Delay(TimeSpan.FromSeconds(2));
                            var shutdownCompleted = await Task.WhenAny(responseTask, gracefulShutdownTask);
                            
                            if (shutdownCompleted == gracefulShutdownTask)
                            {
                                _logger.LogWarning("Worker process did not respond to cancellation, forcing restart");
                                await RestartWorkerProcessAsync();
                            }
                            
                            throw new TimeoutException($"Worker operation '{operation}' timed out after {timeoutSeconds + gracePeriod} seconds");
                        }
                        else
                        {
                            _logger.LogInformation("Worker operation completed within grace period after initial timeout");
                        }
                    }

                    var responseJson = await responseTask;
                    if (string.IsNullOrEmpty(responseJson))
                    {
                        string errorOutput = "";
                        string processStatus = "";
                        try
                        {
                            // Check if process has exited and get error info if available
                            if (_workerProcess?.HasExited == true)
                            {
                                errorOutput = $"Worker process exited with code: {_workerProcess.ExitCode}";
                                processStatus = $"Process ID was: {_workerProcess.Id}, Start time: {_workerProcess.StartTime}";
                                
                                // Try to get more detailed exit information
                                var exitTime = _workerProcess.ExitTime;
                                var totalRunTime = exitTime - _workerProcess.StartTime;
                                processStatus += $", Exit time: {exitTime}, Total runtime: {totalRunTime.TotalMilliseconds}ms";
                            }
                            else if (_workerProcess != null)
                            {
                                processStatus = $"Process is still running. ID: {_workerProcess.Id}, Responding: {!_workerProcess.HasExited}";
                            }
                            else
                            {
                                processStatus = "Worker process is null";
                            }
                            
                            // Collect stderr data if available
                            if (!string.IsNullOrEmpty(_lastStderrOutput))
                            {
                                errorOutput += $". Last stderr: {_lastStderrOutput}";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to check worker process status");
                            processStatus = $"Failed to get process status: {ex.Message}";
                        }

                        var contextMessage = $"Worker process returned empty response for operation '{operation}'";
                        contextMessage += $" with JSON: {GetParameterSummary(requestJson)}";
                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            contextMessage += $". Error details: {errorOutput}";
                        }
                        if (!string.IsNullOrEmpty(processStatus))
                        {
                            contextMessage += $". Process status: {processStatus}";
                        }
                        
                        _logger.LogError("Empty response received: {Context}", contextMessage);
                        
                        // Log worker executable path for debugging
                        _logger.LogError("Worker executable path: {WorkerPath}, Current directory: {CurrentDir}", 
                            _workerExecutablePath, 
                            Environment.CurrentDirectory);
                            
                        throw new InvalidOperationException(contextMessage);
                    }

                    _logger.LogInformation("Received response from worker (length: {Length}): {Response}", 
                        responseJson.Length, 
                        responseJson.Length > 1000 ? responseJson.Substring(0, 1000) + "..." : responseJson);

                    WorkerResponse<TResult>? response;
                    try
                    {
                        response = JsonSerializationHelper.Deserialize<WorkerResponse<TResult>>(responseJson);
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
                        var errorDetails = new
                        {
                            Operation = operation,
                            RequestJson = requestJson,
                            Error = response.Error,
                            ErrorIsNull = response.Error == null,
                            ErrorIsEmpty = string.IsNullOrEmpty(response.Error),
                            Data = response.Data,
                            Timestamp = DateTime.UtcNow,
                            WorkerPid = _workerProcess?.Id
                        };
                        
                        var errorMessage = string.IsNullOrEmpty(response.Error) ? 
                            "Unknown error occurred" : response.Error;
                        
                        var contextualErrorMessage = $"Worker operation '{operation}' failed: {errorMessage}";
                        contextualErrorMessage += $" (JSON: {GetParameterSummary(requestJson)})";
                        
                        _logger.LogError("Worker operation failed with details: {@ErrorDetails}", errorDetails);
                        
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
                        var dataJson = JsonSerializationHelper.Serialize(response.Data!);
                        var result = JsonSerializationHelper.Deserialize<TResult>(dataJson)!;
                        _logger.LogInformation("Successfully deserialized worker response data to type {ResultType}", typeof(TResult).Name);
                        return result;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize response data to type {ResultType}. Data: {Data}", 
                            typeof(TResult).Name, JsonSerializationHelper.Serialize(response.Data!));
                        throw new InvalidOperationException($"Failed to deserialize response data to {typeof(TResult).Name}: {ex.Message}", ex);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is ObjectDisposedException))
            {
                _logger.LogError(ex, "Unexpected error in ExecuteAsync for operation '{Operation}' with JSON: {RequestJson}", 
                    operation, GetParameterSummary(JsonSerializationHelper.Serialize(request)));
                throw new InvalidOperationException($"Internal server error occurred while executing operation '{operation}': {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Generate a concise summary of JSON parameters for logging purposes
        /// </summary>
        /// <param name="json">The JSON string to summarize</param>
        /// <returns>A safe string representation of the JSON parameters</returns>
        private string GetParameterSummary(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    return "empty";
                
                // Truncate long JSON strings for logging
                if (json.Length > 200)
                {
                    return json.Substring(0, 200) + "...";
                }
                
                return json;
            }
            catch
            {
                return "invalid-json";
            }
        }

        private async Task EnsureWorkerProcessAsync()
        {
            try
            {
                _logger.LogInformation($"[SubprocessExecutor] EnsureWorkerProcessAsync called. Current process status: {(_workerProcess == null ? "null" : _workerProcess.HasExited ? "exited" : "running")}");
                
                if (_workerProcess == null || _workerProcess.HasExited)
                {
                    if (_workerProcess?.HasExited == true)
                    {
                        _logger.LogWarning($"[SubprocessExecutor] Worker process has exited. Exit code: {_workerProcess.ExitCode}");
                    }
                    _logger.LogInformation($"[SubprocessExecutor] Starting new worker process...");
                    await StartWorkerProcessAsync();
                }
                else
                {
                    _logger.LogInformation($"[SubprocessExecutor] Worker process is already running. Process ID: {_workerProcess.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SubprocessExecutor] Failed to ensure worker process is running. Worker path: {_workerPath}");
                throw new InvalidOperationException($"Failed to start or verify worker process: {ex.Message}", ex);
            }
        }

        private async Task StartWorkerProcessAsync()
        {
            try
            {
                _logger.LogInformation($"[SubprocessExecutor] StartWorkerProcessAsync called. Worker path: {_workerPath}");

                ProcessStartInfo startInfo;
                
                // Check if it's a project directory (for development)
                if (Directory.Exists(_workerPath) && File.Exists(Path.Combine(_workerPath, "UIAutomationMCP.Worker.csproj")))
                {
                    _logger.LogInformation($"[SubprocessExecutor] Detected project directory: {_workerPath}");
                    // Use dotnet run for project directory with Release configuration
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run --configuration Release",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = _workerPath
                    };
                    _logger.LogDebug($"[SubprocessExecutor] Starting worker using dotnet run from directory: {_workerPath}");
                }
                else if (_workerPath.EndsWith(".dll"))
                {
                    // For .dll files, use dotnet to run them
                    if (!File.Exists(_workerPath))
                    {
                        throw new FileNotFoundException($"Worker DLL not found at: {_workerPath}");
                    }
                    
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"\"{_workerPath}\"",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_workerPath) ?? ""
                    };
                    _logger.LogDebug("Starting worker DLL: {WorkerPath}", _workerPath);
                }
                else
                {
                    // For executable files
                    if (!File.Exists(_workerPath))
                    {
                        throw new FileNotFoundException($"Worker executable not found at: {_workerPath}");
                    }

                    startInfo = new ProcessStartInfo
                    {
                        FileName = _workerPath,
                        Arguments = "",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_workerPath) ?? ""
                    };
                    _logger.LogDebug("Starting worker executable: {WorkerPath}", _workerPath);
                }

                _workerProcess = new Process { StartInfo = startInfo };
                
                // Store the worker executable path for debugging
                _workerExecutablePath = startInfo.FileName + " " + startInfo.Arguments;
                
                // Set up async stderr monitoring
                _workerProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogDebug("Worker stderr: {ErrorData}", e.Data);
                        _lastStderrOutput = e.Data; // Store last stderr output for debugging
                    }
                };
                
                try
                {
                    _workerProcess.Start();
                    _workerProcess.BeginErrorReadLine(); // Start async stderr reading
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start worker process: {WorkerPath}. StartInfo: FileName={FileName}, Arguments={Arguments}", 
                        _workerPath, startInfo.FileName, startInfo.Arguments);
                    throw new InvalidOperationException($"Failed to start worker process: {_workerPath}", ex);
                }

                // Wait longer for the process to start properly
                await Task.Delay(500);

                if (_workerProcess.HasExited)
                {
                    var exitCode = _workerProcess.ExitCode;
                    _logger.LogError("Worker process exited immediately with code: {ExitCode}", exitCode);
                    throw new InvalidOperationException($"Worker process failed to start (exit code: {exitCode})");
                }

                _logger.LogInformation("Worker process started with PID: {ProcessId}", _workerProcess.Id);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                _logger.LogError(ex, "Unexpected error while starting worker process: {WorkerPath}", _workerPath);
                throw new InvalidOperationException($"Failed to start worker process due to unexpected error: {ex.Message}", ex);
            }
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

            lock (_lockObject)
            {
                if (_disposed) return;
                _disposed = true;
            }

            _logger.LogInformation("Disposing SubprocessExecutor - terminating worker processes");

            if (_workerProcess != null)
            {
                try
                {
                    if (!_workerProcess.HasExited)
                    {
                        _logger.LogInformation("Terminating worker process PID: {ProcessId}", _workerProcess.Id);
                        
                        // Try graceful shutdown first by closing stdin
                        try
                        {
                            _workerProcess.StandardInput.Close();
                            _logger.LogDebug("Closed standard input for worker process");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error closing standard input");
                        }
                        
                        // Give it more time to exit gracefully
                        if (!_workerProcess.WaitForExit(3000))
                        {
                            _logger.LogWarning("Worker process did not exit gracefully within 3 seconds, forcing termination");
                            
                            try
                            {
                                // Kill the entire process tree to ensure cleanup
                                _workerProcess.Kill(entireProcessTree: true);
                                _logger.LogDebug("Kill signal sent to worker process tree");
                                
                                // Wait for forced termination to complete
                                if (!_workerProcess.WaitForExit(2000))
                                {
                                    _logger.LogError("Worker process did not respond to Kill signal within 2 seconds");
                                }
                                else
                                {
                                    _logger.LogInformation("Worker process terminated successfully");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error during forced termination of worker process");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Worker process exited gracefully with code: {ExitCode}", _workerProcess.ExitCode);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Worker process was already exited with code: {ExitCode}", _workerProcess.ExitCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during worker process termination");
                }
                finally
                {
                    try
                    {
                        _workerProcess.Dispose();
                        _logger.LogDebug("Worker process disposed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error disposing worker process");
                    }
                    _workerProcess = null;
                }
            }

            try
            {
                _semaphore?.Dispose();
                _logger.LogDebug("Semaphore disposed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing semaphore");
            }

            _logger.LogInformation("SubprocessExecutor disposal completed");
        }

        /// <summary>
        /// IOperationExecutor implementation - provides type-safe operation execution with ServiceOperationResult wrapper
        /// </summary>
        async Task<ServiceOperationResult<TResult>> IOperationExecutor.ExecuteAsync<TRequest, TResult>(
            string operationName, 
            TRequest request, 
            int timeoutSeconds)
        {
            try
            {
                var result = await ExecuteAsync<TRequest, TResult>(operationName, request, timeoutSeconds);
                return ServiceOperationResult<TResult>.FromSuccess(result);
            }
            catch (ArgumentException ex)
            {
                return ServiceOperationResult<TResult>.FromException(ex, "InvalidArgument");
            }
            catch (NotSupportedException ex)
            {
                return ServiceOperationResult<TResult>.FromException(ex, "NotSupported");
            }
            catch (UnauthorizedAccessException ex)
            {
                return ServiceOperationResult<TResult>.FromException(ex, "Unauthorized");
            }
            catch (TimeoutException ex)
            {
                return ServiceOperationResult<TResult>.FromException(ex, "Timeout");
            }
            catch (InvalidOperationException ex)
            {
                return ServiceOperationResult<TResult>.FromException(ex, "InvalidOperation");
            }
            catch (Exception ex)
            {
                return ServiceOperationResult<TResult>.FromException(ex, "UnhandledException");
            }
        }

    }
}