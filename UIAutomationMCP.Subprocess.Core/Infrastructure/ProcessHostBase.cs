using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
        /// Main process loop for handling stdin/stdout communication with MessagePack
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
                        // Read length-prefixed MessagePack data
                        requestData = await ReadMessagePackRequestAsync(Console.OpenStandardInput());
                        if (requestData == null)
                        {
                            _logger.LogInformation("Standard input closed, waiting for running operations to complete in {ProcessType} process", GetProcessType());
                            _shutdownRequested = true;
                            await WaitForRunningOperationsAsync();
                            _logger.LogInformation("All operations completed, shutting down {ProcessType} process", GetProcessType());
                            break;
                        }
                        
                        _logger.LogDebug("Received MessagePack data: {Length} bytes", requestData.Length);
                        
                        if (requestData.Length == 0)
                        {
                            _logger.LogDebug("Empty MessagePack data received, continuing");
                            continue;
                        }

                        // Check if shutdown was requested
                        if (_shutdownRequested)
                        {
                            _logger.LogWarning("Shutdown requested, rejecting new operation");
                            WriteResponse(new WorkerResponse<object> 
                            { 
                                Success = false, 
                                Error = "Server is shutting down, operation rejected",
                                Data = null
                            });
                            continue;
                        }

                        // Deserialize MessagePack WorkerRequest
                        var workerRequest = MessagePackSerializationHelper.Deserialize<WorkerRequest>(requestData);
                        if (workerRequest == null || string.IsNullOrEmpty(workerRequest.Operation))
                        {
                            _logger.LogWarning("Invalid WorkerRequest received");
                            WriteResponse(WorkerResponse<object>.CreateError("Invalid request format"));
                            continue;
                        }
                        
                        _logger.LogDebug("Successfully extracted operation: {Operation}", workerRequest.Operation);
                        
                        // Use type-safe ProcessRequestAsync with direct object parameters
                        _logger.LogDebug("Using MessagePack parameters for operation: {Operation}", workerRequest.Operation);
                        var response = await ProcessRequestAsync(workerRequest.Operation, workerRequest.Parameters);
                        _logger.LogDebug("Processing completed, writing response: {Success}", response.Success);
                        WriteResponse(response);
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
                        WriteResponse(new WorkerResponse<object> 
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
        /// Process a request with the given operation name and parameters (type-safe)
        /// </summary>
        /// <param name="operationName">Name of the operation to execute</param>
        /// <param name="parameters">Direct object parameters</param>
        /// <returns>Response object</returns>
        protected virtual async Task<WorkerResponse<object>> ProcessRequestAsync(string operationName, object? parameters)
        {
            var operationId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation("[{ProcessType}] Starting operation: {Operation} (ID: {OperationId}) at {Time}", 
                    GetProcessType(), operationName, operationId, DateTime.UtcNow);
                _logger.LogDebug("[{ProcessType}] Parameters type: {ParameterType}", GetProcessType(), 
                    parameters?.GetType().Name ?? "null");

                // Create operation task for tracking
                var operationTask = ExecuteOperationInternalAsync(operationName, parameters);
                
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


        private async Task<WorkerResponse<object>> ExecuteOperationInternalAsync(string operationName, object? parameters)
        {
            // Try to get the operation for this request
            var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>(operationName);
            if (operation != null)
            {
                _logger.LogDebug("[{ProcessType}] Operation handler found: {OperationType}", GetProcessType(), operation.GetType().Name);
                
                // Use type-safe ExecuteAsync with direct object parameters
                var operationResult = await operation.ExecuteAsync(parameters);
                return ConvertOperationResult(operationResult, operationName);
            }

            // Allow derived classes to handle specific operations with object parameters
            var customResult = await HandleCustomOperationAsync(operationName, parameters);
            if (customResult != null)
            {
                return customResult;
            }

            return WorkerResponse<object>.CreateError($"No operation found for: {operationName}");
        }


        /// <summary>
        /// Handle custom operations that are not registered as services (type-safe)
        /// Override in derived classes to provide specific functionality
        /// </summary>
        /// <param name="operationName">Operation name</param>
        /// <param name="parameters">Direct object parameters</param>
        /// <returns>Response or null if operation is not handled</returns>
        protected virtual Task<WorkerResponse<object>?> HandleCustomOperationAsync(string operationName, object? parameters)
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
        /// Read length-prefixed MessagePack data from stream
        /// </summary>
        private async Task<byte[]?> ReadMessagePackRequestAsync(Stream stream)
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

                // Read the actual MessagePack data
                byte[] data = new byte[dataLength];
                totalRead = 0;
                while (totalRead < dataLength)
                {
                    int bytesRead = await stream.ReadAsync(data.AsMemory(totalRead, dataLength - totalRead));
                    if (bytesRead == 0)
                    {
                        _logger.LogError("Unexpected end of stream while reading MessagePack data");
                        return null;
                    }
                    totalRead += bytesRead;
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading MessagePack request");
                return null;
            }
        }

        /// <summary>
        /// Write response to stdout as MessagePack
        /// </summary>
        private void WriteResponse(WorkerResponse<object> response)
        {
            try
            {
                // Serialize to MessagePack binary format
                byte[] responseData = MessagePackSerializationHelper.Serialize(response);
                
                // Write length prefix
                byte[] lengthBytes = BitConverter.GetBytes(responseData.Length);
                Console.OpenStandardOutput().Write(lengthBytes, 0, 4);
                
                // Write MessagePack data
                Console.OpenStandardOutput().Write(responseData, 0, responseData.Length);
                Console.OpenStandardOutput().Flush();
                
                _logger.LogDebug("MessagePack response written successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MessagePack serialization failed");
                throw new InvalidOperationException($"Failed to serialize response to MessagePack: {ex.Message}", ex);
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

