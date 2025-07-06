using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public interface IWindowService
    {
        Task<object> WindowActionAsync(string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class WindowService : IWindowService
    {
        private readonly ILogger<WindowService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public WindowService(ILogger<WindowService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> WindowActionAsync(string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing window action: {Action} on window: {WindowTitle}", action, windowTitle);

                var window = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId);
                    if (searchRoot == null && !string.IsNullOrEmpty(windowTitle))
                    {
                        // Search for window by title
                        var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                        searchRoot = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
                    }
                    return searchRoot;
                }, timeoutSeconds, $"FindWindow_{windowTitle}");

                if (window == null)
                {
                    return new { Success = false, Error = $"Window '{windowTitle}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    if (window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) && pattern is WindowPattern windowPattern)
                    {
                        switch (action.ToLowerInvariant())
                        {
                            case "minimize":
                                windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                                break;
                            case "maximize":
                                windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                                break;
                            case "normal":
                            case "restore":
                                windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                                break;
                            case "close":
                                windowPattern.Close();
                                break;
                            default:
                                throw new ArgumentException($"Unsupported window action: {action}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Window does not support WindowPattern");
                    }
                }, timeoutSeconds, $"WindowAction_{action}");

                _logger.LogInformation("Window action performed successfully: {Action}", action);
                return new { Success = true, Message = $"Window action '{action}' performed successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform window action: {Action}", action);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Transforming element: {ElementId} with action: {Action}", elementId, action);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) && pattern is TransformPattern transformPattern)
                    {
                        switch (action.ToLowerInvariant())
                        {
                            case "move":
                                if (x.HasValue && y.HasValue)
                                {
                                    transformPattern.Move(x.Value, y.Value);
                                }
                                else
                                {
                                    throw new ArgumentException("Move action requires x and y coordinates");
                                }
                                break;
                            case "resize":
                                if (width.HasValue && height.HasValue)
                                {
                                    transformPattern.Resize(width.Value, height.Value);
                                }
                                else
                                {
                                    throw new ArgumentException("Resize action requires width and height");
                                }
                                break;
                            case "rotate":
                                if (x.HasValue) // Use x as rotation degrees
                                {
                                    transformPattern.Rotate(x.Value);
                                }
                                else
                                {
                                    throw new ArgumentException("Rotate action requires rotation degrees in x parameter");
                                }
                                break;
                            default:
                                throw new ArgumentException($"Unsupported transform action: {action}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TransformPattern");
                    }
                }, timeoutSeconds, $"TransformElement_{action}");

                _logger.LogInformation("Element transformed successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Element transformed with action '{action}' successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}