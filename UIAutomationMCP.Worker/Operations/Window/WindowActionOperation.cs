using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WindowActionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<WindowActionOperation> _logger;

        public WindowActionOperation(
            ElementFinderService elementFinderService, 
            ILogger<WindowActionOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<WindowActionRequest>(parametersJson)!;
                
                var action = typedRequest.Action;
                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;

                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Window not found",
                        Data = new WindowActionResult { ActionName = action }
                    });
                }

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "WindowPattern not supported",
                        Data = new WindowActionResult { ActionName = action }
                    });
                }

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
                    case "setfocus":
                        window.SetFocus();
                        result.CurrentState = windowPattern.Current.WindowVisualState.ToString();
                        break;
                    default:
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = false, 
                            Error = $"Unsupported window action: {action}",
                            Data = result
                        });
                }

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WindowAction operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to perform window action: {ex.Message}",
                    Data = new WindowActionResult 
                    { 
                        ActionName = "",
                        Completed = false
                    }
                });
            }
        }
    }
}