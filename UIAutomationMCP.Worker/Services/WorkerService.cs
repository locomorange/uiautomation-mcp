using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Operations;
using System.Text.Json;

namespace UIAutomationMCP.Worker.Services
{
    public class WorkerService
    {
        private readonly ILogger<WorkerService> _logger;
        private readonly InvokeOperations _invokeOperations;
        private readonly ValueOperations _valueOperations;
        private readonly ElementSearchOperations _elementSearchOperations;
        private readonly ElementPropertyOperations _elementPropertyOperations;
        private readonly ToggleOperations _toggleOperations;
        private readonly SelectionOperations _selectionOperations;

        public WorkerService(
            ILogger<WorkerService> logger,
            InvokeOperations invokeOperations,
            ValueOperations valueOperations,
            ElementSearchOperations elementSearchOperations,
            ElementPropertyOperations elementPropertyOperations,
            ToggleOperations toggleOperations,
            SelectionOperations selectionOperations)
        {
            _logger = logger;
            _invokeOperations = invokeOperations;
            _valueOperations = valueOperations;
            _elementSearchOperations = elementSearchOperations;
            _elementPropertyOperations = elementPropertyOperations;
            _toggleOperations = toggleOperations;
            _selectionOperations = selectionOperations;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Worker process started. Waiting for commands...");

            while (true)
            {
                try
                {
                    var input = await Console.In.ReadLineAsync();
                    if (string.IsNullOrEmpty(input))
                    {
                        break;
                    }

                    var request = JsonSerializer.Deserialize<WorkerRequest>(input);
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
                    _logger.LogError(ex, "Error processing request");
                    WriteResponse(new WorkerResponse { Success = false, Error = ex.Message });
                }
            }
        }

        private async Task<WorkerResponse> ProcessRequestAsync(WorkerRequest request)
        {
            try
            {
                // Extract common parameters
                var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = request.Parameters?.GetValueOrDefault("processId") != null ? 
                    Convert.ToInt32(request.Parameters["processId"]) : 0;

                var result = request.Operation switch
                {
                    // Invoke operations
                    "InvokeElement" => _invokeOperations.InvokeElement(elementId, windowTitle, processId),
                    
                    // Value operations
                    "SetElementValue" => _valueOperations.SetElementValue(elementId, 
                        request.Parameters?.GetValueOrDefault("value")?.ToString() ?? "", windowTitle, processId),
                    "GetElementValue" => _valueOperations.GetElementValue(elementId, windowTitle, processId),
                    
                    // Element search operations
                    "FindElements" => _elementSearchOperations.FindElements(
                        request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("controlType")?.ToString() ?? "",
                        windowTitle, processId),
                    "GetDesktopWindows" => _elementSearchOperations.GetDesktopWindows(),
                    
                    // Toggle operations
                    "ToggleElement" => _toggleOperations.ToggleElement(elementId, windowTitle, processId),
                    
                    // Selection operations
                    "SelectElement" => _selectionOperations.SelectElement(elementId, windowTitle, processId),
                    "GetSelection" => _selectionOperations.GetSelection(
                        request.Parameters?.GetValueOrDefault("containerElementId")?.ToString() ?? "", windowTitle, processId),
                    
                    _ => new UIAutomationMCP.Models.OperationResult { Success = false, Error = $"Unknown operation: {request.Operation}" }
                };

                return new WorkerResponse 
                { 
                    Success = result.Success, 
                    Data = result.Data, 
                    Error = result.Error 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing operation: {Operation}", request.Operation);
                return new WorkerResponse { Success = false, Error = ex.Message };
            }
        }

        private void WriteResponse(WorkerResponse response)
        {
            var json = JsonSerializer.Serialize(response);
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