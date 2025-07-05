using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.PatternExecutors;

namespace UiAutomationWorker.Services
{
    /// <summary>
    /// 操作の種類に応じて適切なエグゼキューターに処理を振り分けるクラス
    /// </summary>
    public class OperationExecutor
    {
        private readonly ILogger<OperationExecutor> _logger;
        private readonly ElementSearchExecutor _elementSearchExecutor;
        private readonly CorePatternExecutor _corePatternExecutor;
        private readonly LayoutPatternExecutor _layoutPatternExecutor;
        private readonly TreePatternExecutor _treePatternExecutor;
        private readonly TextPatternExecutor _textPatternExecutor;
        private readonly WindowPatternExecutor _windowPatternExecutor;

        public OperationExecutor(
            ILogger<OperationExecutor> logger,
            ElementSearchExecutor elementSearchExecutor,
            CorePatternExecutor corePatternExecutor,
            LayoutPatternExecutor layoutPatternExecutor,
            TreePatternExecutor treePatternExecutor,
            TextPatternExecutor textPatternExecutor,
            WindowPatternExecutor windowPatternExecutor)
        {
            _logger = logger;
            _elementSearchExecutor = elementSearchExecutor;
            _corePatternExecutor = corePatternExecutor;
            _layoutPatternExecutor = layoutPatternExecutor;
            _treePatternExecutor = treePatternExecutor;
            _textPatternExecutor = textPatternExecutor;
            _windowPatternExecutor = windowPatternExecutor;
        }

        /// <summary>
        /// 操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteOperationAsync(WorkerOperation operation)
        {
            try
            {
                _logger.LogInformation("[OperationExecutor] Executing operation: {Operation}", operation.Operation);

                return operation.Operation.ToLowerInvariant() switch
                {
                    // 要素検索操作
                    "findfirst" => await _elementSearchExecutor.ExecuteFindFirstAsync(operation),
                    "findall" => await _elementSearchExecutor.ExecuteFindAllAsync(operation),
                    "getproperties" => await _elementSearchExecutor.ExecuteGetPropertiesAsync(operation),

                    // Core Pattern Operations
                    "invoke" => await _corePatternExecutor.ExecuteInvokeAsync(operation),
                    "setvalue" => await _corePatternExecutor.ExecuteSetValueAsync(operation),
                    "getvalue" => await _corePatternExecutor.ExecuteGetValueAsync(operation),
                    "toggle" => await _corePatternExecutor.ExecuteToggleAsync(operation),
                    "select" => await _corePatternExecutor.ExecuteSelectAsync(operation),

                    // Layout Pattern Operations
                    "scroll" => await _layoutPatternExecutor.ExecuteScrollAsync(operation),
                    "scrollintoview" => await _layoutPatternExecutor.ExecuteScrollIntoViewAsync(operation),

                    // Text Pattern Operations
                    "gettext" => await _textPatternExecutor.ExecuteGetTextAsync(operation),
                    "selecttext" => await _textPatternExecutor.ExecuteSelectTextAsync(operation),
                    "findtext" => await _textPatternExecutor.ExecuteFindTextAsync(operation),
                    "gettextselection" => await _textPatternExecutor.ExecuteGetTextSelectionAsync(operation),

                    // Window Pattern Operations
                    "setwindowstate" => await _windowPatternExecutor.ExecuteSetWindowStateAsync(operation),
                    "getwindowstate" => await _windowPatternExecutor.ExecuteGetWindowStateAsync(operation),
                    "closewindow" => await _windowPatternExecutor.ExecuteCloseWindowAsync(operation),
                    "waitforwindowstate" => await _windowPatternExecutor.ExecuteWaitForWindowStateAsync(operation),

                    // Tree Operations
                    "gettree" => await _treePatternExecutor.ExecuteGetTreeAsync(operation),
                    "getchildren" => await _treePatternExecutor.ExecuteGetChildrenAsync(operation),

                    // Aliases for backward compatibility
                    "value" => await _corePatternExecutor.ExecuteSetValueAsync(operation),
                    "get_value" => await _corePatternExecutor.ExecuteGetValueAsync(operation),

                    // 未知の操作
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
        /// サポートされている操作の一覧を取得します
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
        /// 操作がサポートされているかチェックします
        /// </summary>
        public static bool IsOperationSupported(string operation)
        {
            return GetSupportedOperations().Contains(operation.ToLowerInvariant());
        }

        /// <summary>
        /// 操作の詳細情報を取得します
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
