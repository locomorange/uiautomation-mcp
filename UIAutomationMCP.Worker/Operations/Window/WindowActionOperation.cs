using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WindowActionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public WindowActionOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<WindowActionResult>> ExecuteAsync(WorkerRequest request)
        {
            // 型安全なリクエストを試行し、失敗した場合は従来の方法にフォールバック
            var typedRequest = request.GetTypedRequest<WindowActionRequest>(_options);
            
            string action, windowTitle;
            int processId;
            
            if (typedRequest != null)
            {
                // 型安全なパラメータアクセス
                action = typedRequest.Action;
                windowTitle = typedRequest.WindowTitle ?? "";
                processId = typedRequest.ProcessId ?? 0;
            }
            else
            {
                // 従来の方法（後方互換性のため）
                action = request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            }

            var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
            if (window == null)
                return Task.FromResult(new OperationResult<WindowActionResult> 
                { 
                    Success = false, 
                    Error = "Window not found",
                    Data = new WindowActionResult { ActionName = action }
                });

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                return Task.FromResult(new OperationResult<WindowActionResult> 
                { 
                    Success = false, 
                    Error = "WindowPattern not supported",
                    Data = new WindowActionResult { ActionName = action }
                });

            try
            {
                // Get the current state before action
                var previousState = windowPattern.Current.WindowVisualState.ToString();
                var windowHandle = window.Current.NativeWindowHandle;
                
                var result = new WindowActionResult
                {
                    ActionName = action,
                    WindowTitle = window.Current.Name,
                    WindowHandle = windowHandle,
                    PreviousState = previousState,
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow
                };

                switch (action.ToLowerInvariant())
                {
                    case "minimize":
                        windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                        result.CurrentState = "Minimized";
                        break;
                    case "maximize":
                        windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                        result.CurrentState = "Maximized";
                        break;
                    case "normal":
                    case "restore":
                        windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                        result.CurrentState = "Normal";
                        break;
                    case "close":
                        windowPattern.Close();
                        result.CurrentState = "Closed";
                        break;
                    default:
                        return Task.FromResult(new OperationResult<WindowActionResult> 
                        { 
                            Success = false, 
                            Error = $"Unsupported window action: {action}",
                            Data = result
                        });
                }

                return Task.FromResult(new OperationResult<WindowActionResult> 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<WindowActionResult> 
                { 
                    Success = false, 
                    Error = $"Failed to perform window action: {ex.Message}",
                    Data = new WindowActionResult 
                    { 
                        ActionName = action,
                        WindowTitle = window.Current.Name,
                        Completed = false
                    }
                });
            }
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}