using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// コアパターン（Invoke、Value、Toggle、Select）を実行するクラス
    /// </summary>
    public class CorePatternExecutor
    {
        private readonly ILogger<CorePatternExecutor> _logger;
        private readonly AutomationHelper _automationHelper;

        public CorePatternExecutor(
            ILogger<CorePatternExecutor> logger,
            AutomationHelper automationHelper)
        {
            _logger = logger;
            _automationHelper = automationHelper;
        }

        /// <summary>
        /// Invoke操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteInvokeAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[CorePatternExecutor] Executing Invoke operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for Invoke operation"
                            };
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
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support InvokePattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[CorePatternExecutor] Invoke operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Invoke operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CorePatternExecutor] Invoke operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Invoke operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// SetValue操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSetValueAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[CorePatternExecutor] Executing SetValue operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for SetValue operation"
                            };
                        }

                        // 値の取得
                        if (!operation.Parameters.TryGetValue("value", out var valueObj) || valueObj == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Value parameter is required for SetValue operation"
                            };
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
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support ValuePattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[CorePatternExecutor] SetValue operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"SetValue operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CorePatternExecutor] SetValue operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"SetValue operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// GetValue操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetValueAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[CorePatternExecutor] Executing GetValue operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for GetValue operation"
                            };
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
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support ValuePattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[CorePatternExecutor] GetValue operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"GetValue operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CorePatternExecutor] GetValue operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetValue operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// Toggle操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteToggleAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[CorePatternExecutor] Executing Toggle operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for Toggle operation"
                            };
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
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support TogglePattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[CorePatternExecutor] Toggle operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Toggle operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CorePatternExecutor] Toggle operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Toggle operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// Select操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSelectAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[CorePatternExecutor] Executing Select operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for Select operation"
                            };
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
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support SelectionItemPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[CorePatternExecutor] Select operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"Select operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[CorePatternExecutor] Select operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"Select operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// 操作から対象要素を検索します
        /// </summary>
        private AutomationElement? FindElementFromOperation(WorkerOperation operation)
        {
            try
            {
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    _logger.LogError("[CorePatternExecutor] Failed to get search root");
                    return null;
                }

                // ElementIdパラメータから要素を検索
                if (operation.Parameters.TryGetValue("ElementId", out var elementIdObj) && 
                    elementIdObj?.ToString() is string elementId && !string.IsNullOrEmpty(elementId))
                {
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }

                // 他の検索条件でも試行
                var condition = _automationHelper.BuildCondition(operation);
                if (condition != null)
                {
                    return searchRoot.FindFirst(TreeScope.Descendants, condition);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CorePatternExecutor] Failed to find element from operation");
                return null;
            }
        }

        /// <summary>
        /// 要素名の安全な取得
        /// </summary>
        private string SafeGetElementName(AutomationElement element)
        {
            try
            {
                return element?.Current.Name ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}
