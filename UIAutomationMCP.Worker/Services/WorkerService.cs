using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using UIAutomationMCP.Shared;
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

            while (true)
            {
                string? input = null;
                try
                {
                    input = await Console.In.ReadLineAsync();
                    _logger.LogDebug("Received input: {Input}", input ?? "null");
                    
                    if (string.IsNullOrEmpty(input))
                    {
                        _logger.LogDebug("Input is null or empty, breaking main loop");
                        break;
                    }

                    var request = JsonSerializer.Deserialize<WorkerRequest>(input, JsonSerializationConfig.Options);
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing request. Input: {Input}", input ?? "null");
                    WriteResponse(new WorkerResponse<object> 
                    { 
                        Success = false, 
                        Error = $"Request processing failed: {ex.Message}",
                        Data = new 
                        { 
                            ExceptionType = ex.GetType().Name,
                            Input = input ?? "null",
                            StackTrace = ex.StackTrace
                        }
                    });
                }
            }
        }

        private async Task<WorkerResponse<object>> ProcessRequestAsync(WorkerRequest request)
        {
            try
            {
                _logger.LogDebug("Processing operation: {Operation} with parameters: {Parameters}", 
                    request.Operation, JsonSerializer.Serialize(request.Parameters, JsonSerializationConfig.Options));

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
                _logger.LogError(ex, "Error executing operation: {Operation} with parameters: {Parameters}. Exception type: {ExceptionType}", 
                    request.Operation, JsonSerializer.Serialize(request.Parameters, JsonSerializationConfig.Options), ex.GetType().Name);

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
                    Data = new 
                    { 
                        ExceptionType = ex.GetType().Name,
                        StackTrace = ex.StackTrace,
                        Operation = request.Operation,
                        Parameters = request.Parameters
                    }
                };
            }
        }

        private void WriteResponse(WorkerResponse<object> response)
        {
            var json = JsonSerializer.Serialize(response, JsonSerializationConfig.Options);
            Console.WriteLine(json);
            Console.Out.Flush(); // Ensure immediate output
        }
    }
}
