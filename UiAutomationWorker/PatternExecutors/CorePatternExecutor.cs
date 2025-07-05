using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// コアパターン（Invoke、Value、Toggle、Select）を実行するクラス
    /// </summary>
    public class CorePatternExecutor : BasePatternExecutor
    {
        public CorePatternExecutor(
            ILogger<CorePatternExecutor> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// Invoke操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteInvokeAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Invoke");
                }

                // InvokePatternの取得と実行
                if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var patternObj) && 
                    patternObj is InvokePattern invokePattern)
                {
                    _logger.LogInformation("[CorePatternExecutor] Invoking element: {ElementName}", 
                        SafeGetElementName(element));
                    
                    invokePattern.Invoke();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element invoked successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("InvokePattern");
                }
            }, "Invoke");
        }

        /// <summary>
        /// SetValue操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSetValueAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("SetValue");
                }

                // 値の取得
                if (!operation.Parameters.TryGetValue("value", out var valueObj) || valueObj == null)
                {
                    return CreateParameterMissingResult("Value", "SetValue");
                }

                var value = valueObj.ToString() ?? "";

                // ValuePatternの取得と実行
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObj) && 
                    patternObj is ValuePattern valuePattern)
                {
                    if (valuePattern.Current.IsReadOnly)
                    {
                        return new WorkerResult
                        {
                            Success = false,
                            Error = "Element is read-only and cannot be modified"
                        };
                    }

                    _logger.LogInformation("[CorePatternExecutor] Setting value on element: {ElementName} to: {Value}", 
                        SafeGetElementName(element), value);
                    
                    valuePattern.SetValue(value);
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = $"Value set to: {value}"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("ValuePattern");
                }
            }, "SetValue");
        }

        /// <summary>
        /// GetValue操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetValueAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("GetValue");
                }

                // ValuePatternの取得と実行
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObj) && 
                    patternObj is ValuePattern valuePattern)
                {
                    var currentValue = valuePattern.Current.Value ?? "";
                    
                    _logger.LogInformation("[CorePatternExecutor] Got value from element: {ElementName}, value: {Value}", 
                        SafeGetElementName(element), currentValue);
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = new Dictionary<string, object>
                        {
                            ["Value"] = currentValue,
                            ["IsReadOnly"] = valuePattern.Current.IsReadOnly
                        }
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("ValuePattern");
                }
            }, "GetValue");
        }

        /// <summary>
        /// Toggle操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteToggleAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Toggle");
                }

                // TogglePatternの取得と実行
                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var patternObj) && 
                    patternObj is TogglePattern togglePattern)
                {
                    var currentState = togglePattern.Current.ToggleState;
                    
                    _logger.LogInformation("[CorePatternExecutor] Toggling element: {ElementName}, current state: {State}", 
                        SafeGetElementName(element), currentState);
                    
                    togglePattern.Toggle();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = $"Element toggled from {currentState} state"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("TogglePattern");
                }
            }, "Toggle");
        }

        /// <summary>
        /// Select操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSelectAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("Select");
                }

                // SelectionItemPatternの取得と実行
                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObj) && 
                    patternObj is SelectionItemPattern selectionItemPattern)
                {
                    _logger.LogInformation("[CorePatternExecutor] Selecting element: {ElementName}", 
                        SafeGetElementName(element));
                    
                    selectionItemPattern.Select();
                    
                    return new WorkerResult
                    {
                        Success = true,
                        Data = "Element selected successfully"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("SelectionItemPattern");
                }
            }, "Select");
        }

    }
}
