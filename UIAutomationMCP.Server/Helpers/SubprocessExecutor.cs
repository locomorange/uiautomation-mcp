using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Logging;

namespace UIAutomationMCP.Server.Helpers
{
    public class SubprocessExecutor : IOperationExecutor, IDisposable, IAsyncDisposable
    {
        private readonly ILogger<SubprocessExecutor> _logger;
        private readonly string _workerPath;
        private readonly CancellationTokenSource _shutdownCts;
        private Process? _workerProcess;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _disposed = false;
        private readonly object _lockObject = new object();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingOperations = new();

        // Log relay callback
        private Func<string, Task>? _logMessageCallback;

        public SubprocessExecutor(ILogger<SubprocessExecutor> logger, string workerPath, CancellationTokenSource shutdownCts)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workerPath = !string.IsNullOrWhiteSpace(workerPath) ? workerPath : throw new ArgumentException("Worker path cannot be null or empty", nameof(workerPath));
            _shutdownCts = shutdownCts ?? throw new ArgumentNullException(nameof(shutdownCts));
        }

        /// <summary>
        /// Set callback for handling log messages from subprocess
        /// </summary>
        public void SetLogMessageCallback(Func<string, Task> callback)
        {
            _logMessageCallback = callback;
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
            var operationId = Guid.NewGuid().ToString();
            var operationTcs = new TaskCompletionSource<bool>();

            try
            {
                if (_disposed)
                {
                    _logger.LogError("[SubprocessExecutor] SubprocessExecutor is disposed for operation: {Operation}", operation);
                    throw new ObjectDisposedException(nameof(SubprocessExecutor));
                }

                // Track this operation
                _pendingOperations.TryAdd(operationId, operationTcs);
                _logger.LogDebug("[SubprocessExecutor] Operation {Operation} (ID: {OperationId}) started. Total pending: {Count}",
                    operation, operationId, _pendingOperations.Count);

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

                await _semaphore.WaitAsync();
                try
                {
                    await EnsureWorkerProcessAsync();

                    var operationStartTime = DateTime.UtcNow;
                    // Serialize to UTF-8 JSON byte array
                    byte[] requestData = JsonUtf8SerializationHelper.SerializeToUtf8Bytes(request);

                    // Write length-prefixed UTF-8 JSON data
                    byte[] lengthBytes = BitConverter.GetBytes(requestData.Length);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _shutdownCts.Token);

                    await _workerProcess!.StandardInput.BaseStream.WriteAsync(lengthBytes, combinedCts.Token);
                    await _workerProcess.StandardInput.BaseStream.WriteAsync(requestData, combinedCts.Token);
                    await _workerProcess.StandardInput.BaseStream.FlushAsync(combinedCts.Token);

                    // Read length-prefixed UTF-8 JSON response
                    var responseTask = ReadUtf8JsonResponseAsync(_workerProcess.StandardOutput.BaseStream, combinedCts.Token);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), combinedCts.Token);
                    var completedTask = await Task.WhenAny(responseTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        if (combinedCts.Token.IsCancellationRequested && _shutdownCts.Token.IsCancellationRequested)
                        {
                            _logger.LogInformation("Worker operation cancelled due to shutdown: {Operation}", operation);
                            throw new OperationCanceledException("Operation cancelled due to server shutdown");
                        }

                        _logger.LogWarning("Worker operation could not complete within {TimeoutSeconds}s: {Operation}", timeoutSeconds, operation);
                        cts.Cancel();

                        // Wait for the responseTask to complete or be cancelled to avoid stream conflicts
                        try
                        {
                            await responseTask;
                        }
                        catch (OperationCanceledException)
                        {
                            // Expected when the task is cancelled due to timeout
                            _logger.LogDebug("Response task cancelled due to timeout for operation: {Operation}", operation);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Exception while waiting for response task completion: {Operation}", operation);
                        }

                        // Check if worker process crashed
                        if (_workerProcess?.HasExited == true)
                        {
                            _logger.LogWarning("Worker process crashed during operation, restarting");
                            await RestartWorkerProcessAsync();
                        }

                        throw new TimeoutException($"Worker operation '{operation}' could not complete within {timeoutSeconds} seconds. Consider increasing the timeout duration.");
                    }

                    byte[] responseData;
                    try
                    {
                        responseData = await responseTask;
                    }
                    catch (OperationCanceledException) when (combinedCts.Token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Operation was cancelled");
                    }

                    if (responseData == null || responseData.Length == 0)
                    {
                        var exitCode = _workerProcess?.HasExited == true ? _workerProcess.ExitCode : (int?)null;
                        var errorMessage = $"Worker process returned empty response for operation '{operation}'";

                        if (exitCode.HasValue)
                        {
                            errorMessage += $" (process exited with code: {exitCode})";
                        }

                        _logger.LogError("{ErrorMessage}", errorMessage);
                        throw new InvalidOperationException(errorMessage);
                    }


                    WorkerResponse<TResult>? response;
                    try
                    {
                        response = JsonUtf8SerializationHelper.DeserializeFromUtf8Bytes<WorkerResponse<TResult>>(responseData);
                    }
                    catch (JsonException ex)
                    {
                        var responseString = System.Text.Encoding.UTF8.GetString(responseData);
                        var errorMessage = $"Failed to deserialize worker response for operation '{operation}'. Response: {responseString}";
                        _logger.LogError(ex, "UTF-8 JSON deserialization failed: {ErrorMessage}", errorMessage);
                        throw new InvalidOperationException(errorMessage, ex);
                    }

                    if (response == null)
                    {
                        var responseString = System.Text.Encoding.UTF8.GetString(responseData);
                        var errorMessage = $"Deserialized response is null for operation '{operation}'. Raw response: {responseString}";
                        _logger.LogError("Null response after deserialization: {ErrorMessage}", errorMessage);
                        throw new InvalidOperationException(errorMessage);
                    }

                    if (!response.Success)
                    {
                        var errorMessage = response.Error ?? "Unknown error occurred";
                        var contextualErrorMessage = $"Worker operation '{operation}' failed: {errorMessage}";

                        _logger.LogError("Worker operation failed: {Operation} for element: {ElementId} with category: {ErrorCategory}",
                            operation,
                            response.ErrorDetails?.AutomationId ?? "unknown",
                            response.ErrorDetails?.ErrorCategory ?? "unknown");

                        // Use structured error information from ErrorDetails
                        var errorCategory = response.ErrorDetails?.ErrorCategory?.ToLowerInvariant();

                        switch (errorCategory)
                        {
                            case "invalidargument":
                            case "validation":
                            case "elementnotfound":
                                throw new ArgumentException(contextualErrorMessage);

                            case "notsupported":
                                throw new NotSupportedException(contextualErrorMessage);

                            case "invalidoperation":
                                throw new InvalidOperationException(contextualErrorMessage);

                            case "unauthorized":
                                throw new UnauthorizedAccessException(contextualErrorMessage);

                            case "timeout":
                                throw new TimeoutException(contextualErrorMessage);

                            default:
                                throw new InvalidOperationException(contextualErrorMessage);
                        }
                    }

                    try
                    {
                        var dataBytes = JsonUtf8SerializationHelper.SerializeToUtf8Bytes(response.Data!);
                        var result = JsonUtf8SerializationHelper.DeserializeFromUtf8Bytes<TResult>(dataBytes)!;
                        return result;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize response data to type {ResultType}. Data: {Data}",
                            typeof(TResult).Name, JsonUtf8SerializationHelper.SerializeToString(response.Data!));
                        throw new InvalidOperationException($"Failed to deserialize response data to {typeof(TResult).Name}: {ex.Message}", ex);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (OperationCanceledException ex) when (_shutdownCts.Token.IsCancellationRequested)
            {
                _logger.LogInformation("Operation '{Operation}' cancelled due to server shutdown", operation);
                throw new OperationCanceledException("Operation cancelled due to server shutdown", ex);
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is ObjectDisposedException))
            {
                _logger.LogError(ex, "Unexpected error in ExecuteAsync for operation '{Operation}'", operation);
                throw new InvalidOperationException($"Internal server error occurred while executing operation '{operation}': {ex.Message}", ex);
            }
            finally
            {
                // Complete and remove operation tracking
                operationTcs.TrySetResult(true);
                _pendingOperations.TryRemove(operationId, out _);
                _logger.LogDebug("[SubprocessExecutor] Operation {Operation} (ID: {OperationId}) completed. Total pending: {Count}",
                    operation, operationId, _pendingOperations.Count);
            }
        }

        /// <summary>
        /// Reads length-prefixed UTF-8 JSON data from stream
        /// </summary>
        private async Task<byte[]> ReadUtf8JsonResponseAsync(Stream stream, CancellationToken cancellationToken)
        {
            // Read 4-byte length prefix
            byte[] lengthBytes = new byte[4];
            int totalRead = 0;
            while (totalRead < 4)
            {
                int bytesRead = await stream.ReadAsync(lengthBytes.AsMemory(totalRead, 4 - totalRead), cancellationToken);
                if (bytesRead == 0)
                    throw new EndOfStreamException("Unexpected end of stream while reading length prefix");
                totalRead += bytesRead;
            }

            int dataLength = BitConverter.ToInt32(lengthBytes, 0);
            if (dataLength <= 0 || dataLength > 10 * 1024 * 1024) // 10MB limit
                throw new InvalidDataException($"Invalid data length: {dataLength}");

            // Read the actual UTF-8 JSON data
            byte[] data = new byte[dataLength];
            totalRead = 0;
            while (totalRead < dataLength)
            {
                int bytesRead = await stream.ReadAsync(data.AsMemory(totalRead, dataLength - totalRead), cancellationToken);
                if (bytesRead == 0)
                    throw new EndOfStreamException("Unexpected end of stream while reading UTF-8 JSON data");
                totalRead += bytesRead;
            }

            return data;
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
                _logger.LogError(ex, $"[SubprocessExecutor] Failed to ensure worker process is running. Worker path: {_workerPath}");
                throw new InvalidOperationException($"Failed to start or verify worker process: {ex.Message}", ex);
            }
        }

        private async Task StartWorkerProcessAsync()
        {
            try
            {
                ProcessStartInfo startInfo;

                // Check if it's a project directory (for development)
                // Note: _workerPath can be either Worker or Monitor project directory
                // Only treat as valid if the directory contains a Worker or Monitor project file
                var projectFiles = Directory.Exists(_workerPath)
                    ? Directory.GetFiles(_workerPath, "*Worker.csproj")
                        .Concat(Directory.GetFiles(_workerPath, "*Monitor.csproj"))
                        .ToArray()
                    : Array.Empty<string>();
                if (projectFiles.Length > 0)
                {
                    // Detect configuration from the calling server's base directory
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var config = baseDir.Contains("Debug") ? "Debug" : "Release";
                    
                    // Use dotnet run for project directory with detected configuration
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --configuration {config}",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = _workerPath
                    };
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
                }

                _workerProcess = new Process { StartInfo = startInfo };

                // Set up async stderr monitoring with log message processing
                _workerProcess.ErrorDataReceived += async (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        // Check if this is a log message for relay
                        if (e.Data.StartsWith("[MCP_LOG]"))
                        {
                            var logJson = e.Data.Substring("[MCP_LOG]".Length);
                            if (_logMessageCallback != null)
                            {
                                try
                                {
                                    await _logMessageCallback(logJson);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to process log message from subprocess: {LogJson}", logJson);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Worker stderr: {ErrorData}", e.Data);
                        }
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
                        }
                        catch (InvalidOperationException)
                        {
                            // Process may have already exited during graceful shutdown attempt
                        }
                    }
                    // Worker process was already exited
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


        /// <summary>
        /// Wait for all pending operations to complete with timeout protection
        /// </summary>
        public async Task WaitForPendingOperationsAsync(int timeoutSeconds = 30)
        {
            if (_pendingOperations.IsEmpty)
            {
                _logger.LogInformation("No pending operations to wait for");
                return;
            }

            _logger.LogInformation("Waiting for {Count} pending operations to complete", _pendingOperations.Count);

            var timeout = TimeSpan.FromSeconds(timeoutSeconds);
            var allOperations = _pendingOperations.Values.ToArray();

            try
            {
                var completionTask = Task.WhenAll(allOperations.Select(tcs => tcs.Task));
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(completionTask, timeoutTask);

                if (completedTask == completionTask)
                {
                    _logger.LogInformation("All {Count} pending operations completed successfully", allOperations.Length);
                }
                else
                {
                    _logger.LogWarning("Timeout reached after {Timeout}s, {Pending} operations still pending",
                        timeout.TotalSeconds, _pendingOperations.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while waiting for pending operations to complete");
            }
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

                                // Wait for forced termination to complete
                                if (!_workerProcess.WaitForExit(2000))
                                {
                                    _logger.LogError("Worker process did not respond to Kill signal within 2 seconds");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error during forced termination of worker process");
                            }
                        }
                    }
                    // Worker process was already exited
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

        /// <summary>
        /// Async disposal that waits for all pending operations to complete
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            lock (_lockObject)
            {
                if (_disposed) return;
                _disposed = true;
            }

            _logger.LogInformation("[SubprocessExecutor] Async disposal started - waiting for {Count} pending operations", _pendingOperations.Count);

            // Wait for all pending operations to complete with timeout protection
            if (!_pendingOperations.IsEmpty)
            {
                var timeout = TimeSpan.FromSeconds(30);
                var allOperations = _pendingOperations.Values.ToArray();

                try
                {
                    var completionTask = Task.WhenAll(allOperations.Select(tcs => tcs.Task));
                    var timeoutTask = Task.Delay(timeout);

                    var completedTask = await Task.WhenAny(completionTask, timeoutTask);

                    if (completedTask == completionTask)
                    {
                        _logger.LogInformation("[SubprocessExecutor] All {Count} pending operations completed successfully", allOperations.Length);
                    }
                    else
                    {
                        _logger.LogWarning("[SubprocessExecutor] Timeout reached after {Timeout}s, {Remaining} operations still pending",
                            timeout.TotalSeconds, _pendingOperations.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SubprocessExecutor] Error while waiting for pending operations to complete");
                }

                // Clear any remaining operations
                var remainingCount = _pendingOperations.Count;
                if (remainingCount > 0)
                {
                    _logger.LogWarning("[SubprocessExecutor] Clearing {Count} remaining pending operations", remainingCount);
                    _pendingOperations.Clear();
                }
            }

            // Dispose synchronously as normal
            Dispose();

            _logger.LogInformation("[SubprocessExecutor] Async disposal completed");
        }

    }
}
