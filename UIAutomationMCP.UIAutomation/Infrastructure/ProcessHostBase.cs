using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.UIAutomation.Abstractions;

namespace UIAutomationMCP.UIAutomation.Infrastructure
{
    /// <summary>
    /// Base class for process hosts that handle stdin/stdout communication
    /// Provides common functionality for Worker and Monitor processes
    /// </summary>
    public abstract class ProcessHostBase
    {
        protected readonly ILogger _logger;
        protected readonly IServiceProvider _serviceProvider;

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
                            _logger.LogInformation("Standard input closed, shutting down {ProcessType} process", GetProcessType());
                            break;
                        }
                        
                        if (string.IsNullOrEmpty(input))
                        {
                            _logger.LogDebug("Empty input received, continuing");
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
            try
            {
                _logger.LogInformation("[{ProcessType}] Starting operation: {Operation} at {Time}", GetProcessType(), operationName, DateTime.UtcNow);
                _logger.LogDebug("[{ProcessType}] Parameters: {Parameters}", GetProcessType(), parametersJson.Length > 200 ? parametersJson.Substring(0, 200) + "..." : parametersJson);

                // Try to get the operation for this request
                var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>(operationName);
                if (operation != null)
                {
                    _logger.LogDebug("[{ProcessType}] Operation handler found: {OperationType}", GetProcessType(), operation.GetType().Name);
                    var operationResult = await operation.ExecuteAsync(parametersJson);
                    
                    _logger.LogInformation("[{ProcessType}] Operation completed: {Operation} at {Time}, Success: {Success}, Error: {Error}", 
                        GetProcessType(), operationName, DateTime.UtcNow, operationResult.Success, operationResult.Error ?? "None");
                    
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ProcessType}] Operation {Operation} failed", GetProcessType(), operationName);
                return WorkerResponse<object>.CreateError($"Operation failed: {ex.Message}");
            }
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
                
                if (!root.TryGetProperty("operation", out var opElement))
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
        /// Get the process type name for logging
        /// </summary>
        /// <returns>Process type identifier</returns>
        protected abstract string GetProcessType();
    }
}