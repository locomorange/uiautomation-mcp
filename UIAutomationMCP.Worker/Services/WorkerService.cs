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
        private readonly GridOperations _gridOperations;
        private readonly TableOperations _tableOperations;
        private readonly MultipleViewOperations _multipleViewOperations;

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
            RangeOperations rangeOperations,
            GridOperations gridOperations,
            TableOperations tableOperations,
            MultipleViewOperations multipleViewOperations)
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
            _gridOperations = gridOperations;
            _tableOperations = tableOperations;
            _multipleViewOperations = multipleViewOperations;
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
                var result = request.Operation switch
                {
                    // Invoke operations
                    "Invoke" => _invokeOperations.Invoke(request.Element),
                    
                    // Value operations
                    "SetValue" => _valueOperations.SetValue(request.Element, request.Parameters?.GetValueOrDefault("value")?.ToString() ?? ""),
                    "GetValue" => _valueOperations.GetValue(request.Element),
                    "IsReadOnly" => _valueOperations.IsReadOnly(request.Element),
                    
                    // Element search operations
                    "GetRootElement" => _elementSearchOperations.GetRootElement(),
                    "FindElements" => _elementSearchOperations.FindElements(request.Element, request.TreeScope, request.Condition),
                    "FindFirstElement" => _elementSearchOperations.FindFirstElement(request.Element, request.TreeScope, request.Condition),
                    "GetDesktopWindows" => _elementSearchOperations.GetDesktopWindows(),
                    
                    // Element property operations
                    "GetName" => _elementPropertyOperations.GetName(request.Element),
                    "GetAutomationId" => _elementPropertyOperations.GetAutomationId(request.Element),
                    "GetClassName" => _elementPropertyOperations.GetClassName(request.Element),
                    "GetControlType" => _elementPropertyOperations.GetControlType(request.Element),
                    "GetProcessId" => _elementPropertyOperations.GetProcessId(request.Element),
                    "GetBoundingRectangle" => _elementPropertyOperations.GetBoundingRectangle(request.Element),
                    "IsEnabled" => _elementPropertyOperations.IsEnabled(request.Element),
                    "IsVisible" => _elementPropertyOperations.IsVisible(request.Element),
                    "GetHelpText" => _elementPropertyOperations.GetHelpText(request.Element),
                    "GetItemType" => _elementPropertyOperations.GetItemType(request.Element),
                    "GetItemStatus" => _elementPropertyOperations.GetItemStatus(request.Element),
                    "GetAcceleratorKey" => _elementPropertyOperations.GetAcceleratorKey(request.Element),
                    "GetAccessKey" => _elementPropertyOperations.GetAccessKey(request.Element),
                    "GetLabeledBy" => _elementPropertyOperations.GetLabeledBy(request.Element),
                    "GetLocalizedControlType" => _elementPropertyOperations.GetLocalizedControlType(request.Element),
                    "GetFrameworkId" => _elementPropertyOperations.GetFrameworkId(request.Element),
                    "GetOrientation" => _elementPropertyOperations.GetOrientation(request.Element),
                    "GetCulture" => _elementPropertyOperations.GetCulture(request.Element),
                    "GetNativeWindowHandle" => _elementPropertyOperations.GetNativeWindowHandle(request.Element),
                    "GetRuntimeId" => _elementPropertyOperations.GetRuntimeId(request.Element),
                    "GetClickablePoint" => _elementPropertyOperations.GetClickablePoint(request.Element),
                    "HasKeyboardFocus" => _elementPropertyOperations.HasKeyboardFocus(request.Element),
                    "IsKeyboardFocusable" => _elementPropertyOperations.IsKeyboardFocusable(request.Element),
                    "IsOffscreen" => _elementPropertyOperations.IsOffscreen(request.Element),
                    "IsRequiredForForm" => _elementPropertyOperations.IsRequiredForForm(request.Element),
                    "IsPassword" => _elementPropertyOperations.IsPassword(request.Element),
                    "IsContentElement" => _elementPropertyOperations.IsContentElement(request.Element),
                    "IsControlElement" => _elementPropertyOperations.IsControlElement(request.Element),
                    
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