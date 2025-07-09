using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Operations;
using System.Text.Json;
using UIAutomationMCP.Shared;

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
        private readonly TreeNavigationOperations _treeNavigationOperations;
        private readonly ElementInspectionOperations _elementInspectionOperations;
        private readonly GridOperations _gridOperations;
        private readonly TableOperations _tableOperations;
        private readonly MultipleViewOperations _multipleViewOperations;
        private readonly ControlTypeOperations _controlTypeOperations;
        private readonly AccessibilityOperations _accessibilityOperations;
        private readonly CustomPropertyOperations _customPropertyOperations;
        private readonly ScreenshotOperations _screenshotOperations;

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
            TreeNavigationOperations treeNavigationOperations,
            ElementInspectionOperations elementInspectionOperations,
            GridOperations gridOperations,
            TableOperations tableOperations,
            MultipleViewOperations multipleViewOperations,
            ControlTypeOperations controlTypeOperations,
            AccessibilityOperations accessibilityOperations,
            CustomPropertyOperations customPropertyOperations,
            ScreenshotOperations screenshotOperations)
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
            _treeNavigationOperations = treeNavigationOperations;
            _elementInspectionOperations = elementInspectionOperations;
            _gridOperations = gridOperations;
            _tableOperations = tableOperations;
            _multipleViewOperations = multipleViewOperations;
            _controlTypeOperations = controlTypeOperations;
            _accessibilityOperations = accessibilityOperations;
            _customPropertyOperations = customPropertyOperations;
            _screenshotOperations = screenshotOperations;
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

        private Task<WorkerResponse> ProcessRequestAsync(WorkerRequest request)
        {
            try
            {
                _logger.LogDebug("Processing operation: {Operation} with parameters: {Parameters}", 
                    request.Operation, JsonSerializer.Serialize(request.Parameters, JsonSerializationConfig.Options));

                // Extract common parameters with better error context
                var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

                _logger.LogDebug("Extracted parameters - ElementId: {ElementId}, WindowTitle: {WindowTitle}, ProcessId: {ProcessId}", 
                    elementId, windowTitle, processId);

                UIAutomationMCP.Shared.OperationResult result = request.Operation switch
                {
                    // Invoke operations
                    "InvokeElement" => _invokeOperations.InvokeElement(elementId, windowTitle, processId),
                    
                    // Value operations
                    "SetElementValue" => _valueOperations.SetElementValue(elementId, 
                        request.Parameters?.GetValueOrDefault("value")?.ToString() ?? "", windowTitle, processId),
                    "GetElementValue" => _valueOperations.GetElementValue(elementId, windowTitle, processId),
                    "GetValue" => new UIAutomationMCP.Shared.OperationResult 
                    { 
                        Success = _valueOperations.GetValueResult(elementId, windowTitle, processId).Success,
                        Data = _valueOperations.GetValueResult(elementId, windowTitle, processId).Data,
                        Error = _valueOperations.GetValueResult(elementId, windowTitle, processId).Error
                    },
                    "SetValue" => _valueOperations.SetValueResult(elementId, 
                        request.Parameters?.GetValueOrDefault("value")?.ToString() ?? "", windowTitle, processId),
                    "IsReadOnly" => new UIAutomationMCP.Shared.OperationResult 
                    { 
                        Success = _valueOperations.IsReadOnlyResult(elementId, windowTitle, processId).Success,
                        Data = _valueOperations.IsReadOnlyResult(elementId, windowTitle, processId).Data,
                        Error = _valueOperations.IsReadOnlyResult(elementId, windowTitle, processId).Error
                    },
                    
                    // Element search operations
                    "FindElements" => _elementSearchOperations.FindElements(
                        request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("controlType")?.ToString() ?? "",
                        windowTitle, processId),
                    "GetDesktopWindows" => _elementSearchOperations.GetDesktopWindows(),
                    
                    // Toggle operations
                    "ToggleElement" => _toggleOperations.ToggleElement(elementId, windowTitle, processId),
                    "GetToggleState" => _toggleOperations.GetToggleState(elementId, windowTitle, processId),
                    "SetToggleState" => _toggleOperations.SetToggleState(elementId,
                        request.Parameters?.GetValueOrDefault("toggleState")?.ToString() ?? "", windowTitle, processId),
                    
                    // Selection operations
                    "SelectElement" => _selectionOperations.SelectElement(elementId, windowTitle, processId),
                    "SelectItem" => _selectionOperations.SelectItem(elementId, windowTitle, processId),
                    "AddToSelection" => _selectionOperations.AddToSelection(elementId, windowTitle, processId),
                    "RemoveFromSelection" => _selectionOperations.RemoveFromSelection(elementId, windowTitle, processId),
                    "ClearSelection" => _selectionOperations.ClearSelection(
                        request.Parameters?.GetValueOrDefault("containerElementId")?.ToString() ?? "", windowTitle, processId),
                    "GetSelection" => _selectionOperations.GetSelection(
                        request.Parameters?.GetValueOrDefault("containerElementId")?.ToString() ?? "", windowTitle, processId),
                    
                    // Window operations
                    "WindowAction" => _windowOperations.WindowAction(
                        request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "", windowTitle, processId),
                    "TransformElement" => _windowOperations.TransformElement(elementId,
                        request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "",
                        GetDoubleParameter(request.Parameters, "x", 0),
                        GetDoubleParameter(request.Parameters, "y", 0),
                        GetDoubleParameter(request.Parameters, "width", 0),
                        GetDoubleParameter(request.Parameters, "height", 0),
                        windowTitle, processId),
                    
                    // Text operations
                    "GetText" => _textOperations.GetText(elementId, windowTitle, processId),
                    "SelectText" => _textOperations.SelectText(elementId,
                        GetIntParameter(request.Parameters, "startIndex", 0),
                        GetIntParameter(request.Parameters, "length", 0),
                        windowTitle, processId),
                    "FindText" => _textOperations.FindText(elementId,
                        request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "",
                        GetBoolParameter(request.Parameters, "backward", false),
                        GetBoolParameter(request.Parameters, "ignoreCase", true),
                        windowTitle, processId),
                    "GetTextSelection" => _textOperations.GetTextSelection(elementId, windowTitle, processId),
                    "SetText" => _textOperations.SetText(elementId,
                        request.Parameters?.GetValueOrDefault("text")?.ToString() ?? "", windowTitle, processId),
                    "TraverseText" => _textOperations.TraverseText(elementId,
                        request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? "",
                        GetIntParameter(request.Parameters, "count", 1),
                        windowTitle, processId),
                    "GetTextAttributes" => _textOperations.GetTextAttributes(elementId, windowTitle, processId),
                    
                    // Layout operations
                    "ExpandCollapseElement" => _layoutOperations.ExpandCollapseElement(elementId,
                        request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "", windowTitle, processId),
                    "ScrollElement" => _layoutOperations.ScrollElement(elementId,
                        request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? "",
                        GetDoubleParameter(request.Parameters, "amount", 1.0),
                        windowTitle, processId),
                    "ScrollElementIntoView" => _layoutOperations.ScrollElementIntoView(elementId, windowTitle, processId),
                    "DockElement" => _layoutOperations.DockElement(elementId,
                        request.Parameters?.GetValueOrDefault("dockPosition")?.ToString() ?? "", windowTitle, processId),
                    
                    // Range operations
                    "SetRangeValue" => _rangeOperations.SetRangeValue(elementId,
                        GetDoubleParameter(request.Parameters, "value", 0), windowTitle, processId),
                    "GetRangeValue" => _rangeOperations.GetRangeValue(elementId, windowTitle, processId),
                    
                    // Tree navigation operations
                    "GetChildren" => _treeNavigationOperations.GetChildren(elementId, windowTitle, processId),
                    "GetParent" => _treeNavigationOperations.GetParent(elementId, windowTitle, processId),
                    "GetSiblings" => _treeNavigationOperations.GetSiblings(elementId, windowTitle, processId),
                    "GetDescendants" => _treeNavigationOperations.GetDescendants(elementId, windowTitle, processId),
                    "GetAncestors" => _treeNavigationOperations.GetAncestors(elementId, windowTitle, processId),
                    "GetElementTree" => _treeNavigationOperations.GetElementTree(windowTitle, processId,
                        GetIntParameter(request.Parameters, "maxDepth", 3)),
                    
                    // Element inspection operations
                    "GetElementProperties" => _elementInspectionOperations.GetElementProperties(elementId, windowTitle, processId),
                    "GetElementPatterns" => _elementInspectionOperations.GetElementPatterns(elementId, windowTitle, processId),
                    
                    // Grid operations
                    "GetGridInfo" => _gridOperations.GetGridInfo(elementId, windowTitle, processId),
                    "GetGridItem" => _gridOperations.GetGridItem(elementId,
                        GetIntParameter(request.Parameters, "row", 0),
                        GetIntParameter(request.Parameters, "column", 0),
                        windowTitle, processId),
                    "GetRowHeader" => _gridOperations.GetRowHeader(elementId,
                        GetIntParameter(request.Parameters, "row", 0),
                        windowTitle, processId),
                    "GetColumnHeader" => _gridOperations.GetColumnHeader(elementId,
                        GetIntParameter(request.Parameters, "column", 0),
                        windowTitle, processId),
                    
                    // Table operations
                    "GetTableInfo" => _tableOperations.GetTableInfo(elementId, windowTitle, processId),
                    "GetRowHeaders" => _tableOperations.GetRowHeaders(elementId, windowTitle, processId),
                    "GetColumnHeaders" => _tableOperations.GetColumnHeaders(elementId, windowTitle, processId),
                    
                    // Multiple view operations
                    "GetAvailableViews" => _multipleViewOperations.GetAvailableViews(elementId, windowTitle, processId),
                    "SetView" => _multipleViewOperations.SetView(elementId,
                        GetIntParameter(request.Parameters, "viewId", 0),
                        windowTitle, processId),
                    "GetCurrentView" => _multipleViewOperations.GetCurrentView(elementId, windowTitle, processId),
                    "GetViewName" => _multipleViewOperations.GetViewName(elementId,
                        GetIntParameter(request.Parameters, "viewId", 0),
                        windowTitle, processId),
                    
                    // Range operations (additional)
                    "GetRangeProperties" => _rangeOperations.GetRangeProperties(elementId, windowTitle, processId),
                    
                    // Text operations (additional)
                    "AppendText" => _textOperations.AppendText(elementId,
                        request.Parameters?.GetValueOrDefault("text")?.ToString() ?? "", windowTitle, processId),
                    "GetSelectedText" => _textOperations.GetSelectedText(elementId, windowTitle, processId),
                    
                    // Control type operations
                    "ButtonOperation" => _controlTypeOperations.ButtonOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        windowTitle, processId),
                    "CalendarOperation" => _controlTypeOperations.CalendarOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("date")?.ToString(),
                        windowTitle, processId),
                    "ComboBoxOperation" => _controlTypeOperations.ComboBoxOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("itemToSelect")?.ToString(),
                        windowTitle, processId),
                    "HyperlinkOperation" => _controlTypeOperations.HyperlinkOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        windowTitle, processId),
                    "ListOperation" => _controlTypeOperations.ListOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("itemName")?.ToString(),
                        request.Parameters?.GetValueOrDefault("itemIndex") != null ? 
                            GetIntParameter(request.Parameters, "itemIndex", 0) : null,
                        windowTitle, processId),
                    "MenuOperation" => _controlTypeOperations.MenuOperation(
                        request.Parameters?.GetValueOrDefault("menuPath")?.ToString() ?? "",
                        windowTitle, processId),
                    "TabOperation" => _controlTypeOperations.TabOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("tabName")?.ToString(),
                        windowTitle, processId),
                    "TreeViewOperation" => _controlTypeOperations.TreeViewOperation(elementId,
                        request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "",
                        request.Parameters?.GetValueOrDefault("nodePath")?.ToString(),
                        windowTitle, processId),
                    
                    // Accessibility operations
                    "GetAccessibilityInfo" => _accessibilityOperations.GetAccessibilityInfo(elementId, windowTitle, processId),
                    "VerifyAccessibility" => _accessibilityOperations.VerifyAccessibility(
                        string.IsNullOrEmpty(elementId) ? null : elementId, windowTitle, processId),
                    "GetLabeledBy" => _accessibilityOperations.GetLabeledBy(elementId, windowTitle, processId),
                    "GetDescribedBy" => _accessibilityOperations.GetDescribedBy(elementId, windowTitle, processId),
                    
                    // Custom property operations
                    "GetCustomProperties" => _customPropertyOperations.GetCustomProperties(elementId,
                        request.Parameters?.GetValueOrDefault("propertyIds") as string[] ?? new string[0],
                        windowTitle, processId),
                    
                    // Screenshot operations
                    "TakeScreenshot" => _screenshotOperations.TakeScreenshot(
                        string.IsNullOrEmpty(windowTitle) ? null : windowTitle,
                        request.Parameters?.GetValueOrDefault("outputPath")?.ToString(),
                        GetIntParameter(request.Parameters, "maxTokens", 0),
                        processId),
                    
                    _ => new UIAutomationMCP.Shared.OperationResult { Success = false, Error = $"Unknown operation: {request.Operation}" }
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

        private int GetIntParameter(Dictionary<string, object>? parameters, string key, int defaultValue = 0)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && int.TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        private double GetDoubleParameter(Dictionary<string, object>? parameters, string key, double defaultValue = 0.0)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && double.TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        private bool GetBoolParameter(Dictionary<string, object>? parameters, string key, bool defaultValue = false)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && bool.TryParse(value, out var result))
                return result;
            return defaultValue;
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
