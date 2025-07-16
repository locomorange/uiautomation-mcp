using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
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

                        var request = JsonSerializationHelper.DeserializeWorkerRequest(input);
                        if (request == null)
                        {
                            _logger.LogWarning("Failed to deserialize request: {Input}", input);
                            WriteResponse(WorkerResponse<object>.CreateError("Invalid request format"));
                            continue;
                        }

                        _logger.LogDebug("Successfully deserialized request for operation: {Operation}", request.Operation);
                        var response = await ProcessRequestAsync(request);
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

        private async Task<WorkerResponse<object>> ProcessRequestAsync(WorkerRequest request)
        {
            try
            {
                _logger.LogDebug("Processing operation: {Operation} with parameters: {Parameters}", 
                    request.Operation, request.Parameters != null ? "present" : "null");

                // Try to get the operation for this request
                var operation = _serviceProvider.GetKeyedService<IUIAutomationOperation>(request.Operation);
                if (operation != null)
                {
                    var operationResult = await operation.ExecuteAsync(request);
                    return new WorkerResponse<object> 
                    { 
                        Success = operationResult.Success, 
                        Data = operationResult.Data, 
                        Error = operationResult.Error 
                    };
                }

                // All operations are now handled by operation classes
                // This should never be reached if all operations are registered properly
                return WorkerResponse<object>.CreateError($"No operation found for: {request.Operation}");
            }
            catch (Exception ex)
            {
                // Enhanced error logging with operation context
                _logger.LogError(ex, "Error executing operation: {Operation}. Exception type: {ExceptionType}", 
                    request.Operation, ex.GetType().Name);

                // Provide detailed error information for better debugging
                var detailedError = $"Operation '{request.Operation}' failed: {ex.Message}";
                if (ex.InnerException != null)
                {
                    detailedError += $" Inner exception: {ex.InnerException.Message}";
                }

                return new WorkerResponse<object> 
                { 
                    Success = false, 
                    Error = detailedError,
                    Data = null // Avoid complex anonymous types that cause serialization issues
                };
            }
        }

        private void WriteResponse(WorkerResponse<object> response)
        {
            var json = JsonSerializationHelper.SerializeWorkerResponse(response);
            Console.WriteLine(json);
            Console.Out.Flush(); // Ensure immediate output
        }
    }
}
