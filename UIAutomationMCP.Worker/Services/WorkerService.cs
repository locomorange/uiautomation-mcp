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
        private readonly WindowOperations _windowOperations;
        private readonly TextOperations _textOperations;
        private readonly LayoutOperations _layoutOperations;
        private readonly RangeOperations _rangeOperations;

        public WorkerService(
            ILogger<WorkerService> logger,
            InvokeOperations invokeOperations,
            ValueOperations valueOperations,
            ElementSearchOperations elementSearchOperations,
            ElementPropertyOperations elementPropertyOperations,
            ToggleOperations toggleOperations,
            SelectionOperations selectionOperations,
            WindowOperations windowOperations,
            TextOperations textOperations,
            LayoutOperations layoutOperations,
            RangeOperations rangeOperations)
        {
            _logger = logger;
            _invokeOperations = invokeOperations;
            _valueOperations = valueOperations;
            _elementSearchOperations = elementSearchOperations;
            _elementPropertyOperations = elementPropertyOperations;
            _toggleOperations = toggleOperations;
            _selectionOperations = selectionOperations;
            _windowOperations = windowOperations;
            _textOperations = textOperations;
            _layoutOperations = layoutOperations;
            _rangeOperations = rangeOperations;
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
                    
                    // Window operations
                    "WindowAction" => _windowOperations.WindowAction(
                        request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "", windowTitle, processId),
                    "TransformElement" => _windowOperations.TransformElement(elementId,
                        request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "",
                        Convert.ToDouble(request.Parameters?.GetValueOrDefault("x") ?? 0),
                        Convert.ToDouble(request.Parameters?.GetValueOrDefault("y") ?? 0),
                        Convert.ToDouble(request.Parameters?.GetValueOrDefault("width") ?? 0),
                        Convert.ToDouble(request.Parameters?.GetValueOrDefault("height") ?? 0),
                        windowTitle, processId),
                    
                    // Text operations
                    "GetText" => _textOperations.GetText(elementId, windowTitle, processId),
                    "SelectText" => _textOperations.SelectText(elementId,
                        Convert.ToInt32(request.Parameters?.GetValueOrDefault("startIndex") ?? 0),
                        Convert.ToInt32(request.Parameters?.GetValueOrDefault("length") ?? 0),
                        windowTitle, processId),
                    "FindText" => _textOperations.FindText(elementId,
                        request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "",
                        Convert.ToBoolean(request.Parameters?.GetValueOrDefault("backward") ?? false),
                        Convert.ToBoolean(request.Parameters?.GetValueOrDefault("ignoreCase") ?? true),
                        windowTitle, processId),
                    "GetTextSelection" => _textOperations.GetTextSelection(elementId, windowTitle, processId),
                    "SetText" => _textOperations.SetText(elementId,
                        request.Parameters?.GetValueOrDefault("text")?.ToString() ?? "", windowTitle, processId),
                    "TraverseText" => _textOperations.TraverseText(elementId,
                        request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? "",
                        Convert.ToInt32(request.Parameters?.GetValueOrDefault("count") ?? 1),
                        windowTitle, processId),
                    "GetTextAttributes" => _textOperations.GetTextAttributes(elementId, windowTitle, processId),
                    
                    // Layout operations
                    "ExpandCollapseElement" => _layoutOperations.ExpandCollapseElement(elementId,
                        request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "", windowTitle, processId),
                    "ScrollElement" => _layoutOperations.ScrollElement(elementId,
                        request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? "",
                        Convert.ToDouble(request.Parameters?.GetValueOrDefault("amount") ?? 1.0),
                        windowTitle, processId),
                    "ScrollElementIntoView" => _layoutOperations.ScrollElementIntoView(elementId, windowTitle, processId),
                    "DockElement" => _layoutOperations.DockElement(elementId,
                        request.Parameters?.GetValueOrDefault("dockPosition")?.ToString() ?? "", windowTitle, processId),
                    
                    // Range operations
                    "SetRangeValue" => _rangeOperations.SetRangeValue(elementId,
                        Convert.ToDouble(request.Parameters?.GetValueOrDefault("value") ?? 0), windowTitle, processId),
                    "GetRangeValue" => _rangeOperations.GetRangeValue(elementId, windowTitle, processId),
                    
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