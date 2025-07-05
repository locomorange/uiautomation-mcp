using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;
using UiAutomationWorker.Core;

namespace UiAutomationWorker.Patterns.Window
{
    /// <summary>
    /// Microsoft UI Automation WindowPattern handler
    /// Provides window state management functionality
    /// </summary>
    public class WindowPatternHandler : BaseAutomationHandler
    {
        public WindowPatternHandler(
            ILogger<WindowPatternHandler> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// SetWindowState操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSetWindowStateAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("SetWindowState");
                }

                // パラメータの取得
                if (!operation.Parameters.TryGetValue("state", out var stateObj))
                {
                    return CreateParameterMissingResult("state", "SetWindowState");
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
                    return CreatePatternNotSupportedResult("WindowPattern");
                }
            }, "SetWindowState");
        }

        /// <summary>
        /// GetWindowState操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetWindowStateAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("GetWindowState");
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
                    return CreatePatternNotSupportedResult("WindowPattern");
                }
            }, "GetWindowState");
        }

        /// <summary>
        /// CloseWindow操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteCloseWindowAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, () =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("CloseWindow");
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
                    return CreatePatternNotSupportedResult("WindowPattern");
                }
            }, "CloseWindow");
        }

        /// <summary>
        /// WaitForWindowState操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteWaitForWindowStateAsync(WorkerOperation operation)
        {
            // パラメータの取得
            if (!operation.Parameters.TryGetValue("expectedState", out var expectedStateObj))
            {
                return CreateParameterMissingResult("expectedState", "WaitForWindowState");
            }

            var expectedState = expectedStateObj.ToString()?.ToLower();
            var waitTimeoutMs = operation.Parameters.TryGetValue("waitTimeout", out var waitTimeoutObj) ? 
                Convert.ToInt32(waitTimeoutObj) : 5000;

            return await ExecuteWithTimeoutAsync(operation, async cancellationToken =>
            {
                var element = FindElementFromOperation(operation);
                if (element == null)
                {
                    return CreateElementNotFoundResult("WaitForWindowState");
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
                        
                        await Task.Delay(100, cancellationToken);
                    }
                    
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"Window did not reach expected state '{expectedState}' within {waitTimeoutMs}ms"
                    };
                }
                else
                {
                    return CreatePatternNotSupportedResult("WindowPattern");
                }
            }, "WaitForWindowState");
        }

    }
}