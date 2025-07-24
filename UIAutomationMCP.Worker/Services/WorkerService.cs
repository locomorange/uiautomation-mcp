using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Shared.ErrorHandling;
using UIAutomationMCP.Worker.Contracts;

namespace UIAutomationMCP.Worker.Services
{
    public class WorkerService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WorkerService(
            ILogger<WorkerService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Worker process started. Waiting for commands...");

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
                            _logger.LogInformation("Standard input closed, shutting down worker process");
                            break;
                        }
                        
                        if (string.IsNullOrEmpty(input))
                        {
                            _logger.LogDebug("Empty input received, continuing");
                            continue;
                        }

                        // Extract operation name from JSON for KeyedService lookup
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
                        _logger.LogInformation("End of stream reached, shutting down worker process");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing request. Input: {Input}", input ?? "null");
                        WriteResponse(new WorkerResponse<object> 
                        { 
                            Success = false, 
                            Error = $"Request processing failed: {ex.Message}",
                            Data = null // Avoid complex anonymous types that cause serialization issues
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in worker main loop");
            }
            finally
            {
                _logger.LogInformation("Worker process is shutting down");
            }
        }

        private async Task<WorkerResponse<object>> ProcessRequestAsync(string operationName, string parametersJson)
        {
            try
            {
                _logger.LogInformation("[Worker] Starting operation: {Operation} at {Time}", operationName, DateTime.UtcNow);
                _logger.LogDebug("[Worker] Parameters: {Parameters}", parametersJson.Length > 200 ? parametersJson.Substring(0, 200) + "..." : parametersJson);

                // Try to get the operation for this request
                var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>(operationName);
                if (operation != null)
                {
                    _logger.LogDebug("[Worker] Operation handler found: {OperationType}", operation.GetType().Name);
                    var operationResult = await operation.ExecuteAsync(parametersJson);
                    
                    _logger.LogInformation("[Worker] Operation completed: {Operation} at {Time}, Success: {Success}, Error: {Error}", 
                        operationName, DateTime.UtcNow, operationResult.Success, operationResult.Error ?? "None");
                    
                    return new WorkerResponse<object> 
                    { 
                        Success = operationResult.Success, 
                        Data = operationResult.Data, 
                        Error = operationResult.Error 
                    };
                }

                // All operations are now handled by operation classes
                // This should never be reached if all operations are registered properly
                return WorkerResponse<object>.CreateError($"No operation found for: {operationName}");
            }
            catch (Exception ex)
            {
                // Use unified error handling
                var errorResult = ErrorHandlerRegistry.HandleException(ex, operationName, 
                    logAction: (exc, op, elemId, excType) => _logger.LogError(exc, "{Operation} operation failed for element: {ElementId}. Exception: {ExceptionType}", op, elemId, excType));
                
                return new WorkerResponse<object> 
                { 
                    Success = false, 
                    Error = errorResult.Error,
                    Data = errorResult
                };
            }
        }

        /// <summary>
        /// Extract operation name from JSON input for KeyedService lookup
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

        private void WriteResponse(WorkerResponse<object> response)
        {
            var json = JsonSerializationHelper.Serialize(response);
            Console.WriteLine(json);
            Console.Out.Flush(); // Ensure immediate output
        }
    }
}
