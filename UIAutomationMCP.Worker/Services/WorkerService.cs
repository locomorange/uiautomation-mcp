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
                    if (string.IsNullOrEmpty(input))
                    {
                        break;
                    }

                    var request = JsonSerializer.Deserialize<WorkerRequest>(input, JsonSerializationConfig.Options);
                    if (request == null)
                    {
                        WriteResponse(new WorkerResponse { Success = false, Error = "Invalid request format" });
                        continue;
                    }

                    var response = await ProcessRequestAsync(request);
                    WriteResponse(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing request. Input: {Input}", input ?? "null");
                    WriteResponse(new WorkerResponse 
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

        private async Task<WorkerResponse> ProcessRequestAsync(WorkerRequest request)
        {
            try
            {
                _logger.LogDebug("Processing operation: {Operation} with parameters: {Parameters}", 
                    request.Operation, JsonSerializer.Serialize(request.Parameters, JsonSerializationConfig.Options));

                // Try to get the handler for this operation
                var handler = _serviceProvider.GetKeyedService<IUIAutomationOperation>(request.Operation);
                if (handler != null)
                {
                    var result = await handler.ExecuteAsync(request);
                    return new WorkerResponse 
                    { 
                        Success = result.Success, 
                        Data = result.Data, 
                        Error = result.Error 
                    };
                }

                // All operations are now handled by handlers
                // This should never be reached if all handlers are registered properly
                var result = new UIAutomationMCP.Shared.OperationResult 
                { 
                    Success = false, 
                    Error = $"No handler found for operation: {request.Operation}" 
                };

                return Task.FromResult(new WorkerResponse 
                { 
                    Success = result.Success, 
                    Data = result.Data, 
                    Error = result.Error 
                });
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

                return Task.FromResult(new WorkerResponse 
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
                });
            }
        }

        private void WriteResponse(WorkerResponse response)
        {
            var json = JsonSerializer.Serialize(response, JsonSerializationConfig.Options);
            Console.WriteLine(json);
        }
    }

    public class WorkerRequest
    {
        public string Operation { get; set; } = "";
        public System.Windows.Automation.AutomationElement? Element { get; set; }
        public System.Windows.Automation.TreeScope TreeScope { get; set; }
        public System.Windows.Automation.Condition? Condition { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class WorkerResponse
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? Error { get; set; }
    }
}
