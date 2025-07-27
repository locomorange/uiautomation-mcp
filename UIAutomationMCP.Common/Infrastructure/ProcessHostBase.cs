using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Collections.Concurrent;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Common.Abstractions;

namespace UIAutomationMCP.Common.Infrastructure
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
        /// Main process loop for handling stdin/stdout communication
        /// </summary>
        public async Task RunAsync()
        {
            _logger.LogInformation("{ProcessType} process started. Waiting for commands...", GetProcessType());

            try
            {
                while (true)
                {
                    string? input = null;
                    try
                    {
                        input = await Console.In.ReadLineAsync();
                        _logger.LogDebug("Received input: {Input}", input ?? "null");
                        
                        // Check if stdin is closed or we received EOF
                        if (input == null)
                        {
                            _logger.LogInformation("Standard input closed, waiting for running operations to complete in {ProcessType} process", GetProcessType());
                            _shutdownRequested = true;
                            await WaitForRunningOperationsAsync();
                            _logger.LogInformation("All operations completed, shutting down {ProcessType} process", GetProcessType());
                            break;
                        }
                        
                        if (string.IsNullOrEmpty(input))
                        {
                            _logger.LogDebug("Empty input received, continuing");
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

                        // Extract operation name from JSON
                        var operation = ExtractOperationName(input);
                        if (string.IsNullOrEmpty(operation))
                        {
                            continue; // Error already logged and response sent
                        }
                        
                        _logger.LogDebug("Successfully extracted operation: {Operation}", operation);
                        var response = await ProcessRequestAsync(operation, input);
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
                        _logger.LogError(ex, "Error processing request. Input: {Input}", input ?? "null");
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
        /// Process a request with the given operation name and parameters
        /// </summary>
        /// <param name="operationName">Name of the operation to execute</param>
        /// <param name="parametersJson">JSON parameters for the operation</param>
        /// <returns>Response object</returns>
        protected virtual async Task<WorkerResponse<object>> ProcessRequestAsync(string operationName, string parametersJson)
        {
            var operationId = Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation("[{ProcessType}] Starting operation: {Operation} (ID: {OperationId}) at {Time}", 
                    GetProcessType(), operationName, operationId, DateTime.UtcNow);
                _logger.LogDebug("[{ProcessType}] Parameters: {Parameters}", GetProcessType(), 
                    parametersJson.Length > 200 ? parametersJson.Substring(0, 200) + "..." : parametersJson);

                // Create operation task for tracking
                var operationTask = ExecuteOperationInternalAsync(operationName, parametersJson);
                
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

        private async Task<WorkerResponse<object>> ExecuteOperationInternalAsync(string operationName, string parametersJson)
        {
            // Try to get the operation for this request
            var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>(operationName);
            if (operation != null)
            {
                _logger.LogDebug("[{ProcessType}] Operation handler found: {OperationType}", GetProcessType(), operation.GetType().Name);
                var operationResult = await operation.ExecuteAsync(parametersJson);
                return ConvertOperationResult(operationResult, operationName);
            }

            // Allow derived classes to handle specific operations
            var customResult = await HandleCustomOperationAsync(operationName, parametersJson);
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
        /// <param name="parametersJson">JSON parameters</param>
        /// <returns>Response or null if operation is not handled</returns>
        protected virtual Task<WorkerResponse<object>?> HandleCustomOperationAsync(string operationName, string parametersJson)
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
        /// Extract operation name from JSON input
        /// </summary>
        /// <param name="input">JSON string containing the operation property</param>
        /// <returns>Operation name, or null if extraction fails</returns>
        private string? ExtractOperationName(string input)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(input);
                var root = jsonDoc.RootElement;
                
                if (!root.TryGetProperty("operation", out var opElement) && !root.TryGetProperty("Operation", out opElement))
                {
                    _logger.LogWarning("Missing operation property in request: {Input}", input);
                    WriteResponse(WorkerResponse<object>.CreateError("Missing operation property"));
                    return null;
                }
                
                var operation = opElement.GetString();
                if (string.IsNullOrEmpty(operation))
                {
                    _logger.LogWarning("Empty operation property in request: {Input}", input);
                    WriteResponse(WorkerResponse<object>.CreateError("Empty operation property"));
                    return null;
                }
                
                return operation;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse JSON input: {Input}", input);
                WriteResponse(WorkerResponse<object>.CreateError($"Invalid JSON: {ex.Message}"));
                return null;
            }
        }

        /// <summary>
        /// Write response to stdout as JSON
        /// </summary>
        private void WriteResponse(WorkerResponse<object> response)
        {
            var json = JsonSerializationHelper.Serialize(response);
            Console.WriteLine(json);
            Console.Out.Flush(); // Ensure immediate output
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