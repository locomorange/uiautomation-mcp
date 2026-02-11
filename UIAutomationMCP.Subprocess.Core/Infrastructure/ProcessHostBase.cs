using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Collections.Concurrent;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Subprocess.Core.Abstractions;

namespace UIAutomationMCP.Subprocess.Core.Infrastructure
{
    /// <summary>
    /// Base class for process hosts that handle stdin/stdout communication
    /// Provides common functionality for Worker and Monitor processes
    /// </summary>
    public abstract class ProcessHostBase
    {
        protected readonly ILogger _logger;
        protected readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, Task> _runningOperations = new();
        private volatile bool _shutdownRequested = false;

        protected ProcessHostBase(ILogger logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Main process loop for handling stdin/stdout communication with UTF-8 JSON
        /// </summary>
        public async Task RunAsync()
        {
            _logger.LogInformation("{ProcessType} process started. Waiting for commands...", GetProcessType());

            try
            {
                while (true)
                {
                    byte[]? requestData = null;
                    try
                    {
                        // Read length-prefixed UTF-8 JSON data
                        requestData = await ReadUtf8JsonRequestAsync(Console.OpenStandardInput());
                        if (requestData == null)
                        {
                            _logger.LogInformation("Standard input closed, waiting for running operations to complete in {ProcessType} process", GetProcessType());
                            _shutdownRequested = true;
                            await WaitForRunningOperationsAsync();
                            _logger.LogInformation("All operations completed, shutting down {ProcessType} process", GetProcessType());
                            break;
                        }

                        _logger.LogDebug("Received UTF-8 JSON data: {Length} bytes", requestData.Length);

                        if (requestData.Length == 0)
                        {
                            _logger.LogDebug("Empty UTF-8 JSON data received, continuing");
                            continue;
                        }

                        // Check if shutdown was requested
                        if (_shutdownRequested)
                        {
                            _logger.LogWarning("Shutdown requested, rejecting new operation");
                            await WriteUtf8JsonResponseAsync(new WorkerResponse<object>
                            {
                                Success = false,
                                Error = "Server is shutting down, operation rejected",
                                Data = null
                            });
                            continue;
                        }

                        // Extract operation name from UTF-8 JSON data
                        var operation = await ExtractOperationNameFromUtf8JsonAsync(requestData);
                        if (string.IsNullOrEmpty(operation))
                        {
                            continue; // Error already logged and response sent
                        }

                        _logger.LogDebug("Successfully extracted operation: {Operation}", operation);
                        var response = await ProcessRequestAsync(operation, requestData);
                        _logger.LogDebug("Processing completed, writing response: {Success}", response.Success);
                        await WriteUtf8JsonResponseAsync(response);
                        _logger.LogDebug("Response written to stdout");
                    }
                    catch (EndOfStreamException)
                    {
                        _logger.LogInformation("End of stream reached, shutting down {ProcessType} process", GetProcessType());
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing request. Data length: {Length}", requestData?.Length ?? 0);
                        await WriteUtf8JsonResponseAsync(new WorkerResponse<object>
                        {
                            Success = false,
                            Error = $"Request processing failed: {ex.Message}",
                            Data = null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in {ProcessType} main loop", GetProcessType());
            }
            finally
            {
                _logger.LogInformation("{ProcessType} process is shutting down", GetProcessType());
            }
        }

        /// <summary>
        /// Process a request with the given operation name and UTF-8 JSON parameters
        /// </summary>
        /// <param name="operationName">Name of the operation to execute</param>
        /// <param name="utf8JsonData">UTF-8 encoded JSON parameters for the operation</param>
        /// <returns>Response object</returns>
        protected virtual async Task<WorkerResponse<object>> ProcessRequestAsync(string operationName, byte[] utf8JsonData)
        {
            var operationId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("[{ProcessType}] Starting operation: {Operation} (ID: {OperationId}) at {Time}",
                    GetProcessType(), operationName, operationId, DateTime.UtcNow);
                _logger.LogDebug("[{ProcessType}] UTF-8 JSON data length: {Length} bytes", GetProcessType(), utf8JsonData.Length);

                // Create operation task for tracking
                var operationTask = ExecuteOperationInternalAsync(operationName, utf8JsonData);

                // Track the operation
                _runningOperations.TryAdd(operationId, operationTask);
                _logger.LogDebug("[{ProcessType}] Operation {Operation} (ID: {OperationId}) added to tracking. Total running: {Count}",
                    GetProcessType(), operationName, operationId, _runningOperations.Count);

                try
                {
                    var result = await operationTask;
                    _logger.LogInformation("[{ProcessType}] Operation completed: {Operation} (ID: {OperationId}) at {Time}, Success: {Success}",
                        GetProcessType(), operationName, operationId, DateTime.UtcNow, result.Success);
                    return result;
                }
                finally
                {
                    // Remove from tracking when completed
                    _runningOperations.TryRemove(operationId, out _);
                    _logger.LogDebug("[{ProcessType}] Operation {Operation} (ID: {OperationId}) removed from tracking. Total running: {Count}",
                        GetProcessType(), operationName, operationId, _runningOperations.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ProcessType}] Operation {Operation} (ID: {OperationId}) failed", GetProcessType(), operationName, operationId);
                _runningOperations.TryRemove(operationId, out _);
                return WorkerResponse<object>.CreateError($"Operation failed: {ex.Message}");
            }
        }


        private async Task<WorkerResponse<object>> ExecuteOperationInternalAsync(string operationName, byte[] utf8JsonData)
        {
            // Convert UTF-8 JSON data to string for existing operation interfaces
            var parametersJson = System.Text.Encoding.UTF8.GetString(utf8JsonData);

            // Try to get the operation for this request
            var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>(operationName);
            if (operation != null)
            {
                _logger.LogDebug("[{ProcessType}] Operation handler found: {OperationType}", GetProcessType(), operation.GetType().Name);
                var operationResult = await operation.ExecuteAsync(parametersJson);
                return ConvertOperationResult(operationResult, operationName);
            }

            // Allow derived classes to handle specific operations with UTF-8 data
            var customResult = await HandleCustomOperationAsync(operationName, utf8JsonData);
            if (customResult != null)
            {
                return customResult;
            }

            return WorkerResponse<object>.CreateError($"No operation found for: {operationName}");
        }


        /// <summary>
        /// Handle custom operations that are not registered as services
        /// Override in derived classes to provide specific functionality
        /// </summary>
        /// <param name="operationName">Operation name</param>
        /// <param name="utf8JsonData">UTF-8 encoded JSON parameters</param>
        /// <returns>Response or null if operation is not handled</returns>
        protected virtual Task<WorkerResponse<object>?> HandleCustomOperationAsync(string operationName, byte[] utf8JsonData)
        {
            return Task.FromResult<WorkerResponse<object>?>(null);
        }


        /// <summary>
        /// Convert OperationResult to WorkerResponse
        /// </summary>
        private WorkerResponse<object> ConvertOperationResult(OperationResult operationResult, string operationName)
        {
            if (operationResult.Success)
            {
                return WorkerResponse<object>.CreateSuccess(operationResult.Data!);
            }
            else
            {
                // If the operation result data is ErrorResult, use it; otherwise create a generic error
                if (operationResult.Data is ErrorResult errorResult)
                {
                    return WorkerResponse<object>.CreateError(errorResult);
                }
                else
                {
                    var genericError = ErrorResult.CreateGenericError(
                        operationName,
                        "",
                        "OperationFailure",
                        operationResult.Error);
                    return WorkerResponse<object>.CreateError(genericError);
                }
            }
        }

        /// <summary>
        /// Extract operation name from UTF-8 JSON data
        /// </summary>
        /// <param name="utf8JsonData">UTF-8 encoded JSON data containing the operation property</param>
        /// <returns>Operation name, or null if extraction fails</returns>
        private async Task<string?> ExtractOperationNameFromUtf8JsonAsync(byte[] utf8JsonData)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(utf8JsonData);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("operation", out var opElement) && !root.TryGetProperty("Operation", out opElement))
                {
                    _logger.LogWarning("Missing operation property in request. Data length: {Length}", utf8JsonData.Length);
                    await WriteUtf8JsonResponseAsync(WorkerResponse<object>.CreateError("Missing operation property"));
                    return null;
                }

                var operation = opElement.GetString();
                if (string.IsNullOrEmpty(operation))
                {
                    _logger.LogWarning("Empty operation property in request. Data length: {Length}", utf8JsonData.Length);
                    await WriteUtf8JsonResponseAsync(WorkerResponse<object>.CreateError("Empty operation property"));
                    return null;
                }

                return operation;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse UTF-8 JSON data. Data length: {Length}", utf8JsonData.Length);
                await WriteUtf8JsonResponseAsync(WorkerResponse<object>.CreateError($"Invalid JSON: {ex.Message}"));
                return null;
            }
        }

        /// <summary>
        /// Read length-prefixed UTF-8 JSON data from stream
        /// </summary>
        private async Task<byte[]?> ReadUtf8JsonRequestAsync(Stream stream)
        {
            try
            {
                // Read 4-byte length prefix
                byte[] lengthBytes = new byte[4];
                int totalRead = 0;
                while (totalRead < 4)
                {
                    int bytesRead = await stream.ReadAsync(lengthBytes.AsMemory(totalRead, 4 - totalRead));
                    if (bytesRead == 0)
                        return null; // End of stream
                    totalRead += bytesRead;
                }

                int dataLength = BitConverter.ToInt32(lengthBytes, 0);
                if (dataLength <= 0 || dataLength > 10 * 1024 * 1024) // 10MB limit
                {
                    _logger.LogError("Invalid data length: {Length}", dataLength);
                    return null;
                }

                // Read the actual UTF-8 JSON data
                byte[] data = new byte[dataLength];
                totalRead = 0;
                while (totalRead < dataLength)
                {
                    int bytesRead = await stream.ReadAsync(data.AsMemory(totalRead, dataLength - totalRead));
                    if (bytesRead == 0)
                    {
                        _logger.LogError("Unexpected end of stream while reading UTF-8 JSON data");
                        return null;
                    }
                    totalRead += bytesRead;
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading UTF-8 JSON request");
                return null;
            }
        }

        /// <summary>
        /// Write response to stdout as length-prefixed UTF-8 JSON
        /// </summary>
        private async Task WriteUtf8JsonResponseAsync(WorkerResponse<object> response)
        {
            try
            {
                // Serialize to UTF-8 JSON byte array
                byte[] responseData = JsonUtf8SerializationHelper.SerializeToUtf8Bytes(response);

                // Write length prefix
                byte[] lengthBytes = BitConverter.GetBytes(responseData.Length);
                await Console.OpenStandardOutput().WriteAsync(lengthBytes, 0, 4);

                // Write UTF-8 JSON data
                await Console.OpenStandardOutput().WriteAsync(responseData, 0, responseData.Length);
                await Console.OpenStandardOutput().FlushAsync();

                _logger.LogDebug("UTF-8 JSON response written successfully. Length: {Length} bytes", responseData.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UTF-8 JSON serialization failed");
                throw new InvalidOperationException($"Failed to serialize response to UTF-8 JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Wait for all running operations to complete with timeout protection
        /// </summary>
        private async Task WaitForRunningOperationsAsync()
        {
            if (_runningOperations.IsEmpty)
            {
                _logger.LogInformation("[{ProcessType}] No running operations to wait for", GetProcessType());
                return;
            }

            _logger.LogInformation("[{ProcessType}] Waiting for {Count} running operations to complete",
                GetProcessType(), _runningOperations.Count);

            var timeout = TimeSpan.FromSeconds(30); // Maximum wait time
            var allOperations = _runningOperations.Values.ToArray();

            try
            {
                var completionTask = Task.WhenAll(allOperations);
                var timeoutTask = Task.Delay(timeout);

                var completedTask = await Task.WhenAny(completionTask, timeoutTask);

                if (completedTask == completionTask)
                {
                    _logger.LogInformation("[{ProcessType}] All {Count} operations completed successfully",
                        GetProcessType(), allOperations.Length);
                }
                else
                {
                    _logger.LogWarning("[{ProcessType}] Timeout reached after {Timeout}s, {Running} operations still running",
                        GetProcessType(), timeout.TotalSeconds, _runningOperations.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ProcessType}] Error while waiting for operations to complete", GetProcessType());
            }

            // Clear any remaining operations
            var remainingCount = _runningOperations.Count;
            if (remainingCount > 0)
            {
                _logger.LogWarning("[{ProcessType}] Clearing {Count} remaining operations", GetProcessType(), remainingCount);
                _runningOperations.Clear();
            }
        }

        /// <summary>
        /// Get the process type name for logging
        /// </summary>
        /// <returns>Process type identifier</returns>
        protected abstract string GetProcessType();
    }
}

