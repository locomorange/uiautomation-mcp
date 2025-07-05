using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.ElementTree;
using UiAutomationWorker.Patterns.Interaction;
using UiAutomationWorker.Patterns.Layout;
using UiAutomationWorker.Patterns.Text;
using UiAutomationWorker.Patterns.Window;
using UiAutomationWorker.Patterns.Selection;

namespace UiAutomationWorker.Services
{
    /// <summary>
    /// Dispatches operations to appropriate pattern handlers based on operation type
    /// Core execution logic without timeout management (handled by Server)
    /// </summary>
    public class OperationExecutor
    {
        private readonly ILogger<OperationExecutor> _logger;
        private readonly ElementSearchHandler _elementSearchHandler;
        private readonly InvokePatternHandler _invokePatternHandler;
        private readonly ValuePatternHandler _valuePatternHandler;
        private readonly TogglePatternHandler _togglePatternHandler;
        private readonly SelectionItemPatternHandler _selectionItemPatternHandler;
        private readonly LayoutPatternHandler _layoutPatternHandler;
        private readonly TreeNavigationHandler _treeNavigationHandler;
        private readonly TextPatternHandler _textPatternHandler;
        private readonly WindowPatternHandler _windowPatternHandler;

        public OperationExecutor(
            ILogger<OperationExecutor> logger,
            ElementSearchHandler elementSearchHandler,
            InvokePatternHandler invokePatternHandler,
            ValuePatternHandler valuePatternHandler,
            TogglePatternHandler togglePatternHandler,
            SelectionItemPatternHandler selectionItemPatternHandler,
            LayoutPatternHandler layoutPatternHandler,
            TreeNavigationHandler treeNavigationHandler,
            TextPatternHandler textPatternHandler,
            WindowPatternHandler windowPatternHandler)
        {
            _logger = logger;
            _elementSearchHandler = elementSearchHandler;
            _invokePatternHandler = invokePatternHandler;
            _valuePatternHandler = valuePatternHandler;
            _togglePatternHandler = togglePatternHandler;
            _selectionItemPatternHandler = selectionItemPatternHandler;
            _layoutPatternHandler = layoutPatternHandler;
            _treeNavigationHandler = treeNavigationHandler;
            _textPatternHandler = textPatternHandler;
            _windowPatternHandler = windowPatternHandler;
        }

        /// <summary>
        /// Executes the specified UI Automation operation
        /// </summary>
        public async Task<WorkerResult> ExecuteOperationAsync(WorkerOperation operation)
        {
            try
            {
                _logger.LogInformation("[OperationExecutor] Executing operation: {Operation}", operation.Operation);

                return operation.Operation.ToLowerInvariant() switch
                {
                    // 要素検索操作
                    "findfirst" => await _elementSearchHandler.ExecuteFindFirstAsync(operation),
                    "findall" => await _elementSearchHandler.ExecuteFindAllAsync(operation),
                    "getproperties" => await _elementSearchHandler.ExecuteGetPropertiesAsync(operation),

                    // Core Pattern Operations
                    "invoke" => await _invokePatternHandler.ExecuteInvokeAsync(operation),
                    "setvalue" => await _valuePatternHandler.ExecuteSetValueAsync(operation),
                    "getvalue" => await _valuePatternHandler.ExecuteGetValueAsync(operation),
                    "toggle" => await _togglePatternHandler.ExecuteToggleAsync(operation),
                    "select" => await _selectionItemPatternHandler.ExecuteSelectAsync(operation),

                    // Layout Pattern Operations
                    "scroll" => await _layoutPatternHandler.ExecuteScrollAsync(operation),
                    "scrollintoview" => await _layoutPatternHandler.ExecuteScrollIntoViewAsync(operation),

                    // Text Pattern Operations
                    "gettext" => await _textPatternHandler.ExecuteGetTextAsync(operation),
                    "selecttext" => await _textPatternHandler.ExecuteSelectTextAsync(operation),
                    "findtext" => await _textPatternHandler.ExecuteFindTextAsync(operation),
                    "gettextselection" => await _textPatternHandler.ExecuteGetTextSelectionAsync(operation),

                    // Window Pattern Operations
                    "setwindowstate" => await _windowPatternHandler.ExecuteSetWindowStateAsync(operation),
                    "getwindowstate" => await _windowPatternHandler.ExecuteGetWindowStateAsync(operation),
                    "closewindow" => await _windowPatternHandler.ExecuteCloseWindowAsync(operation),
                    "waitforwindowstate" => await _windowPatternHandler.ExecuteWaitForWindowStateAsync(operation),

                    // Tree Operations
                    "gettree" => await _treeNavigationHandler.ExecuteGetTreeAsync(operation),
                    "getchildren" => await _treeNavigationHandler.ExecuteGetChildrenAsync(operation),

                    // Aliases for backward compatibility
                    "value" => await _valuePatternHandler.ExecuteSetValueAsync(operation),
                    "get_value" => await _valuePatternHandler.ExecuteGetValueAsync(operation),

                    // Unknown operation
                    _ => new WorkerResult
                    {
                        Success = false,
                        Error = $"Unknown operation: {operation.Operation}. " +
                                "Supported operations: findfirst, findall, getproperties, invoke, setvalue, getvalue, " +
                                "toggle, select, scroll, scrollintoview, gettext, selecttext, findtext, gettextselection, " +
                                "setwindowstate, getwindowstate, closewindow, waitforwindowstate, gettree, getchildren"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OperationExecutor] Operation execution failed for: {Operation}", operation.Operation);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Operation execution failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets the list of supported operations
        /// </summary>
        public static string[] GetSupportedOperations()
        {
            return new[]
            {
                // 要素検索操作
                "findfirst",
                "findall", 
                "getproperties",

                // Core Pattern Operations
                "invoke",
                "setvalue",
                "getvalue",
                "toggle",
                "select",

                // Layout Pattern Operations
                "scroll",
                "scrollintoview",

                // Text Pattern Operations
                "gettext",
                "selecttext",
                "findtext",
                "gettextselection",

                // Window Pattern Operations
                "setwindowstate",
                "getwindowstate",
                "closewindow",
                "waitforwindowstate",

                // Tree Operations
                "gettree",
                "getchildren",

                // 後方互換性のためのエイリアス
                "value", // setvalue のエイリアス
                "get_value"
            };
        }

        /// <summary>
        /// Checks if the operation is supported
        /// </summary>
        public static bool IsOperationSupported(string operation)
        {
            return GetSupportedOperations().Contains(operation.ToLowerInvariant());
        }

        /// <summary>
        /// Gets detailed information about an operation
        /// </summary>
        public static Dictionary<string, object> GetOperationInfo(string operation)
        {
            var operationLower = operation.ToLowerInvariant();
            
            var info = new Dictionary<string, object>
            {
                ["operation"] = operationLower,
                ["supported"] = IsOperationSupported(operationLower)
            };

            if (!IsOperationSupported(operationLower))
            {
                info["error"] = "Operation not supported";
                return info;
            }

            // 各操作の詳細情報
            info["category"] = operationLower switch
            {
                "findfirst" or "findall" or "getproperties" => "Element Search",
                "invoke" or "setvalue" or "getvalue" or "value" or "get_value" or "toggle" or "select" => "Core Patterns",
                "scroll" or "scrollintoview" => "Layout Patterns",
                "gettree" or "getchildren" => "Tree Operations",
                _ => "Unknown"
            };

            info["description"] = operationLower switch
            {
                "findfirst" => "Find the first element matching the specified criteria",
                "findall" => "Find all elements matching the specified criteria",
                "getproperties" => "Get detailed properties of an element",
                "invoke" => "Invoke an element (click button, activate menu item)",
                "setvalue" => "Set the value of an element (text input, etc.)",
                "getvalue" => "Get the current value of an element",
                "value" => "Set the value of an element (alias for setvalue)",
                "get_value" => "Get the current value of an element (legacy)",
                "toggle" => "Toggle an element (checkbox, toggle button)",
                "select" => "Select an element (list item, tab item, legacy)",
                "selectitem" => "Select an item (list item, tab item)",
                "setwindowstate" => "Set the window state (minimize, maximize, etc.)",
                "setrangevalue" => "Set the value of a range control (slider, progress bar)",
                "getrangevalue" => "Get the current value and range information of a range control",
                "gettext" => "Get text content from an element",
                "selecttext" => "Select text in an element",
                "expandcollapse" => "Expand or collapse an element (tree item, menu)",
                "transform" => "Transform an element (move, resize, rotate)",
                "dock" => "Dock an element to a specific position",
                "scroll" => "Scroll an element",
                "scrollintoview" => "Scroll an element into view",
                "gettree" => "Get the element tree structure for navigation and analysis",
                "getchildren" => "Get the direct children of an element",
                _ => "No description available"
            };

            info["required_parameters"] = operationLower switch
            {
                "setvalue" or "value" => new[] { "ElementId", "Value" },
                "setrangevalue" => new[] { "ElementId", "Value" },
                "selecttext" => new[] { "ElementId", "StartIndex", "Length" },
                "transform" => new[] { "ElementId", "Action" },
                "dock" => new[] { "ElementId", "Position" },
                "setwindowstate" => new[] { "ElementId", "State" },
                _ => new[] { "ElementId" }
            };

            info["optional_parameters"] = operationLower switch
            {
                "findfirst" or "findall" => new[] { "Name", "AutomationId", "ControlType", "WindowTitle", "ProcessId", "Scope" },
                "gettree" => new[] { "WindowTitle", "ProcessId", "MaxDepth" },
                "expandcollapse" => new[] { "Expand", "WindowTitle", "ProcessId" },
                "scroll" => new[] { "Direction", "Horizontal", "Vertical", "WindowTitle", "ProcessId" },
                "transform" => new[] { "X", "Y", "Width", "Height", "Degrees", "WindowTitle", "ProcessId" },
                _ => new[] { "WindowTitle", "ProcessId" }
            };

            return info;
        }

        /// <summary>
        /// WorkerOperationをDictionary<string, object>に変換します
        /// </summary>
        private Dictionary<string, object> ConvertOperationToParameters(WorkerOperation operation)
        {
            // WorkerOperationのParametersをそのまま返す
            return new Dictionary<string, object>(operation.Parameters);
        }

        /// <summary>
        /// TreePatternExecutorの結果をWorkerResultに変換します
        /// </summary>
        private async Task<WorkerResult> ConvertToWorkerResult(Task<object> operationTask)
        {
            try
            {
                var result = await operationTask;
                
                if (result is Dictionary<string, object> dict)
                {
                    var success = dict.ContainsKey("Success") && (bool)dict["Success"];
                    return new WorkerResult
                    {
                        Success = success,
                        Data = success ? dict : null,
                        Error = success ? null : dict.ContainsKey("Error") ? dict["Error"]?.ToString() : "Unknown error"
                    };
                }
                
                // 結果が辞書でない場合は、成功として扱う
                return new WorkerResult
                {
                    Success = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting operation result to WorkerResult");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}
