using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Server.Interfaces;

namespace UIAutomationMCP.Server.Helpers
{
    public class SubprocessExecutor : ISubprocessExecutor, IDisposable
    {
        private readonly ILogger<SubprocessExecutor> _logger;
        private readonly string _workerPath;
        private Process? _workerProcess;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;
        private readonly object _lockObject = new object();

        public SubprocessExecutor(ILogger<SubprocessExecutor> logger, string workerPath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workerPath = !string.IsNullOrWhiteSpace(workerPath) ? workerPath : throw new ArgumentException("Worker path cannot be null or empty", nameof(workerPath));
        }

        /// <summary>
        /// Type-safe overload for TypedWorkerRequest - Tools Level Serialization pattern
        /// Accepts pre-serialized JSON string to avoid generic type inference issues
        /// </summary>
        /// <typeparam name="TResult">The expected result type</typeparam>
        /// <param name="operation">The operation name</param>
        /// <param name="requestJson">Pre-serialized JSON string of the request</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>The operation result</returns>
        public async Task<TResult> ExecuteAsync<TResult>(string operation, string requestJson, int timeoutSeconds = 60) where TResult : notnull
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SubprocessExecutor));
                
                if (string.IsNullOrWhiteSpace(operation))
                    throw new ArgumentException("Operation cannot be null or empty", nameof(operation));
                
                if (string.IsNullOrWhiteSpace(requestJson))
                    throw new ArgumentException("Request JSON cannot be null or empty", nameof(requestJson));
                
                if (timeoutSeconds <= 0)
                    throw new ArgumentException("Timeout must be greater than zero", nameof(timeoutSeconds));
                
                await _semaphore.WaitAsync();
                try
                {
                    await EnsureWorkerProcessAsync();
                
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
                        
                        cts.Cancel();
                        
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
                        contextMessage += $" with JSON: {GetParameterSummary(requestJson)}";
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

                    WorkerResponse<object>? response;
                    try
                    {
                        response = JsonSerializationHelper.Deserialize<WorkerResponse<object>>(responseJson);
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
                    operation, GetParameterSummary(requestJson));
                throw new InvalidOperationException($"Internal server error occurred while executing operation '{operation}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Legacy overload for backward compatibility - handles object parameters
        /// For new code, prefer the JSON string overload for better type safety
        /// </summary>
        /// <typeparam name="TResult">The expected result type</typeparam>
        /// <param name="operation">The operation name</param>
        /// <param name="parameters">The parameters object (Dictionary or TypedWorkerRequest)</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>The operation result</returns>
        public async Task<TResult> ExecuteAsync<TResult>(string operation, object? parameters = null, int timeoutSeconds = 60) where TResult : notnull
        {
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(SubprocessExecutor));
                
                if (string.IsNullOrWhiteSpace(operation))
                    throw new ArgumentException("Operation cannot be null or empty", nameof(operation));
                
                if (timeoutSeconds <= 0)
                    throw new ArgumentException("Timeout must be greater than zero", nameof(timeoutSeconds));
                
                await _semaphore.WaitAsync();
                try
                {
                    await EnsureWorkerProcessAsync();
                
                string requestJson;
                
                if (parameters is Dictionary<string, object> dict)
                {
                    // Legacy path: Wrap in WorkerRequest
                    var request = new WorkerRequest
                    {
                        Operation = operation,
                        Parameters = dict
                    };
                    requestJson = JsonSerializationHelper.Serialize(request);
                }
                else
                {
                    // Legacy path: Current behavior
                    var request = new WorkerRequest
                    {
                        Operation = operation,
                        Parameters = parameters as Dictionary<string, object>
                    };
                    requestJson = JsonSerializationHelper.Serialize(request);
                }
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
                    
                    cts.Cancel();
                    
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
                        contextMessage += $" with parameters: {GetParameterSummary(parameters)}";
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

                WorkerResponse<object>? response;
                try
                {
                    response = JsonSerializationHelper.Deserialize<WorkerResponse<object>>(responseJson);
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
                        Parameters = parameters,
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
                    if (parameters != null)
                    {
                        contextualErrorMessage += $" (Parameters: {GetParameterSummary(parameters)})";
                    }
                    
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
                _logger.LogError(ex, "Unexpected error in ExecuteAsync for operation '{Operation}' with parameters: {Parameters}", 
                    operation, parameters != null ? $"Type: {parameters.GetType().Name}" : "null");
                throw new InvalidOperationException($"Internal server error occurred while executing operation '{operation}': {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Generate a concise summary of parameters for logging purposes
        /// </summary>
        /// <param name="parameters">The parameters object to summarize</param>
        /// <returns>A safe string representation of the parameters</returns>
        private string GetParameterSummary(object parameters)
        {
            try
            {
                return parameters switch
                {
                    FindElementsRequest findReq => $"FindElementsRequest(SearchText={findReq.SearchText}, WindowTitle={findReq.WindowTitle})",
                    InvokeElementRequest invokeReq => $"InvokeElementRequest(ElementId={invokeReq.ElementId})",
                    TypedWorkerRequest typedReq => $"{typedReq.GetType().Name}(Operation={typedReq.Operation})",
                    string json => GetParameterSummary(json),
                    _ => parameters.GetType().Name
                };
            }
            catch
            {
                return parameters.GetType().Name;
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
                if (_workerProcess == null || _workerProcess.HasExited)
                {
                    await StartWorkerProcessAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure worker process is running. Worker path: {WorkerPath}", _workerPath);
                throw new InvalidOperationException($"Failed to start or verify worker process: {ex.Message}", ex);
            }
        }

        private async Task StartWorkerProcessAsync()
        {
            try
            {
                _logger.LogInformation("Starting worker process: {WorkerPath}", _workerPath);

                ProcessStartInfo startInfo;
                
                // Check if it's a project directory (for development)
                if (Directory.Exists(_workerPath) && File.Exists(Path.Combine(_workerPath, "UIAutomationMCP.Worker.csproj")))
                {
                    // Use dotnet run for project directory
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "run --project .",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = _workerPath
                    };
                    _logger.LogDebug("Starting worker using dotnet run from directory: {WorkerPath}", _workerPath);
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
                        WorkingDirectory = Path.GetDirectoryName(_workerPath)
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
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_workerPath)
                    };
                    _logger.LogDebug("Starting worker executable: {WorkerPath}", _workerPath);
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
                    _logger.LogError(ex, "Failed to start worker process: {WorkerPath}. StartInfo: FileName={FileName}, Arguments={Arguments}", 
                        _workerPath, startInfo.FileName, startInfo.Arguments);
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
                        
                        // Give it a brief moment to exit gracefully
                        if (!_workerProcess.WaitForExit(1000))
                        {
                            _logger.LogWarning("Worker process did not exit gracefully within 1 second, forcing termination");
                            
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
    }
}
