using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// WindowPatternの操作を実行するクラス
    /// </summary>
    public class WindowPatternExecutor
    {
        private readonly ILogger<WindowPatternExecutor> _logger;
        private readonly AutomationHelper _automationHelper;

        public WindowPatternExecutor(
            ILogger<WindowPatternExecutor> logger,
            AutomationHelper automationHelper)
        {
            _logger = logger;
            _automationHelper = automationHelper;
        }

        /// <summary>
        /// SetWindowState操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSetWindowStateAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[WindowPatternExecutor] Executing SetWindowState operation");

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
                                Error = "Target element not found for SetWindowState operation"
                            };
                        }

                        // パラメータの取得
                        if (!operation.Parameters.TryGetValue("state", out var stateObj))
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "state parameter is required for SetWindowState operation"
                            };
                        }

                        var state = stateObj.ToString()?.ToLower();
                        if (string.IsNullOrEmpty(state))
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "state parameter cannot be empty"
                            };
                        }

                        // WindowPatternの取得と実行
                        if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var patternObj) && 
                            patternObj is WindowPattern windowPattern)
                        {
                            _logger.LogInformation("[WindowPatternExecutor] Setting window state '{State}' for element: {ElementName}", 
                                state, SafeGetElementName(element));

                            switch (state)
                            {
                                case "minimize":
                                case "minimized":
                                    windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                                    break;
                                case "maximize":
                                case "maximized":
                                    windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                                    break;
                                case "normal":
                                case "restore":
                                case "restored":
                                    windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                                    break;
                                case "close":
                                    windowPattern.Close();
                                    break;
                                default:
                                    return new WorkerResult
                                    {
                                        Success = false,
                                        Error = $"Unsupported window state: {state}. Supported states: minimize, maximize, normal, close"
                                    };
                            }
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = $"Window state set to: {state}"
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support WindowPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[WindowPatternExecutor] SetWindowState operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"SetWindowState operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[WindowPatternExecutor] SetWindowState operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"SetWindowState operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// GetWindowState操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetWindowStateAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[WindowPatternExecutor] Executing GetWindowState operation");

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
                                Error = "Target element not found for GetWindowState operation"
                            };
                        }

                        // WindowPatternの取得と実行
                        if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var patternObj) && 
                            patternObj is WindowPattern windowPattern)
                        {
                            _logger.LogInformation("[WindowPatternExecutor] Getting window state for element: {ElementName}", 
                                SafeGetElementName(element));

                            var windowInfo = new Dictionary<string, object>
                            {
                                ["visualState"] = windowPattern.Current.WindowVisualState.ToString(),
                                ["interactionState"] = windowPattern.Current.WindowInteractionState.ToString(),
                                ["isModal"] = windowPattern.Current.IsModal,
                                ["isTopmost"] = windowPattern.Current.IsTopmost,
                                ["canMinimize"] = windowPattern.Current.CanMinimize,
                                ["canMaximize"] = windowPattern.Current.CanMaximize
                            };
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = windowInfo
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support WindowPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[WindowPatternExecutor] GetWindowState operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"GetWindowState operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[WindowPatternExecutor] GetWindowState operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetWindowState operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// CloseWindow操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteCloseWindowAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[WindowPatternExecutor] Executing CloseWindow operation");

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
                                Error = "Target element not found for CloseWindow operation"
                            };
                        }

                        // WindowPatternの取得と実行
                        if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var patternObj) && 
                            patternObj is WindowPattern windowPattern)
                        {
                            _logger.LogInformation("[WindowPatternExecutor] Closing window for element: {ElementName}", 
                                SafeGetElementName(element));

                            windowPattern.Close();
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = "Window closed successfully"
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support WindowPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[WindowPatternExecutor] CloseWindow operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"CloseWindow operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[WindowPatternExecutor] CloseWindow operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"CloseWindow operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// WaitForWindowState操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteWaitForWindowStateAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[WindowPatternExecutor] Executing WaitForWindowState operation");

            try
            {
                // パラメータの取得
                if (!operation.Parameters.TryGetValue("expectedState", out var expectedStateObj))
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "expectedState parameter is required for WaitForWindowState operation"
                    };
                }

                var expectedState = expectedStateObj.ToString()?.ToLower();
                var waitTimeoutMs = operation.Parameters.TryGetValue("waitTimeout", out var waitTimeoutObj) ? 
                    Convert.ToInt32(waitTimeoutObj) : 5000;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(async () =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for WaitForWindowState operation"
                            };
                        }

                        // WindowPatternの取得
                        if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var patternObj) && 
                            patternObj is WindowPattern windowPattern)
                        {
                            _logger.LogInformation("[WindowPatternExecutor] Waiting for window state '{ExpectedState}' for element: {ElementName}", 
                                expectedState, SafeGetElementName(element));

                            var startTime = DateTime.UtcNow;
                            
                            while ((DateTime.UtcNow - startTime).TotalMilliseconds < waitTimeoutMs)
                            {
                                var currentState = windowPattern.Current.WindowVisualState.ToString().ToLower();
                                
                                if (currentState == expectedState)
                                {
                                    return new WorkerResult
                                    {
                                        Success = true,
                                        Data = $"Window reached expected state: {expectedState}"
                                    };
                                }
                                
                                await Task.Delay(100, cts.Token);
                            }
                            
                            return new WorkerResult
                            {
                                Success = false,
                                Error = $"Window did not reach expected state '{expectedState}' within {waitTimeoutMs}ms"
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support WindowPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[WindowPatternExecutor] WaitForWindowState operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"WaitForWindowState operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[WindowPatternExecutor] WaitForWindowState operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"WaitForWindowState operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// 操作からエレメントを取得します
        /// </summary>
        private AutomationElement? FindElementFromOperation(WorkerOperation operation)
        {
            try
            {
                var elementId = operation.Parameters.TryGetValue("elementId", out var elementIdObj) ? 
                    elementIdObj?.ToString() : null;
                var windowTitle = operation.Parameters.TryGetValue("windowTitle", out var windowTitleObj) ? 
                    windowTitleObj?.ToString() : null;
                var processId = operation.Parameters.TryGetValue("processId", out var processIdObj) ? 
                    Convert.ToInt32(processIdObj) : (int?)null;

                if (string.IsNullOrEmpty(elementId))
                {
                    return null;
                }

                // 検索ルートを取得
                var searchOperation = new WorkerOperation
                {
                    Parameters = new Dictionary<string, object>
                    {
                        ["windowTitle"] = windowTitle ?? "",
                        ["processId"] = processId ?? 0
                    }
                };
                var searchRoot = _automationHelper.GetSearchRoot(searchOperation) ?? AutomationElement.RootElement;
                
                return _automationHelper.FindElementById(elementId, searchRoot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WindowPatternExecutor] Error finding element from operation");
                return null;
            }
        }

        /// <summary>
        /// エレメントの名前を安全に取得します
        /// </summary>
        private string SafeGetElementName(AutomationElement element)
        {
            try
            {
                return element.Current.Name ?? element.Current.AutomationId ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}