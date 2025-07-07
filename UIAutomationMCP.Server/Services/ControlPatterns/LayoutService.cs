using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ILayoutService
    {
        Task<object> ExpandCollapseElementAsync(string elementId, string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ScrollElementAsync(string elementId, string direction, double amount = 1.0, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> DockElementAsync(string elementId, string dockPosition, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class LayoutService : ILayoutService
    {
        private readonly ILogger<LayoutService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public LayoutService(ILogger<LayoutService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> ExpandCollapseElementAsync(string elementId, string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing expand/collapse action '{Action}' on element: {ElementId}", action, elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) && pattern is ExpandCollapsePattern expandCollapsePattern)
                    {
                        var currentState = expandCollapsePattern.Current.ExpandCollapseState;
                        
                        switch (action.ToLowerInvariant())
                        {
                            case "expand":
                                expandCollapsePattern.Expand();
                                break;
                            case "collapse":
                                expandCollapsePattern.Collapse();
                                break;
                            case "toggle":
                                if (currentState == ExpandCollapseState.Expanded)
                                    expandCollapsePattern.Collapse();
                                else
                                    expandCollapsePattern.Expand();
                                break;
                            default:
                                throw new ArgumentException($"Unsupported expand/collapse action: {action}");
                        }

                        var newState = expandCollapsePattern.Current.ExpandCollapseState;
                        return new { PreviousState = currentState.ToString(), NewState = newState.ToString() };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support ExpandCollapsePattern");
                    }
                }, timeoutSeconds, $"ExpandCollapse_{action}_{elementId}");

                _logger.LogInformation("Expand/collapse action performed successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Element {action}ed successfully", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform expand/collapse action on element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> ScrollElementAsync(string elementId, string direction, double amount = 1.0, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Scrolling element: {ElementId} in direction: {Direction}", elementId, direction);

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
                    if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) && pattern is ScrollPattern scrollPattern)
                    {
                        switch (direction.ToLowerInvariant())
                        {
                            case "up":
                                scrollPattern.ScrollVertical(ScrollAmount.SmallDecrement);
                                break;
                            case "down":
                                scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
                                break;
                            case "left":
                                scrollPattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                                break;
                            case "right":
                                scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                                break;
                            case "pageup":
                                scrollPattern.ScrollVertical(ScrollAmount.LargeDecrement);
                                break;
                            case "pagedown":
                                scrollPattern.ScrollVertical(ScrollAmount.LargeIncrement);
                                break;
                            case "pageleft":
                                scrollPattern.ScrollHorizontal(ScrollAmount.LargeDecrement);
                                break;
                            case "pageright":
                                scrollPattern.ScrollHorizontal(ScrollAmount.LargeIncrement);
                                break;
                            default:
                                throw new ArgumentException($"Unsupported scroll direction: {direction}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support ScrollPattern");
                    }
                }, timeoutSeconds, $"Scroll_{direction}_{elementId}");

                _logger.LogInformation("Element scrolled successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Element scrolled {direction} successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scroll element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Scrolling element into view: {ElementId}", elementId);

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
                    if (element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) && pattern is ScrollItemPattern scrollItemPattern)
                    {
                        scrollItemPattern.ScrollIntoView();
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support ScrollItemPattern");
                    }
                }, timeoutSeconds, $"ScrollIntoView_{elementId}");

                _logger.LogInformation("Element scrolled into view successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element scrolled into view successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scroll element into view: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> DockElementAsync(string elementId, string dockPosition, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Docking element: {ElementId} to position: {DockPosition}", elementId, dockPosition);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) && pattern is DockPattern dockPattern)
                    {
                        var currentPosition = dockPattern.Current.DockPosition;
                        
                        DockPosition newPosition = dockPosition.ToLowerInvariant() switch
                        {
                            "top" => DockPosition.Top,
                            "bottom" => DockPosition.Bottom,
                            "left" => DockPosition.Left,
                            "right" => DockPosition.Right,
                            "fill" => DockPosition.Fill,
                            "none" => DockPosition.None,
                            _ => throw new ArgumentException($"Unsupported dock position: {dockPosition}")
                        };

                        dockPattern.SetDockPosition(newPosition);
                        var updatedPosition = dockPattern.Current.DockPosition;
                        
                        return new { PreviousPosition = currentPosition.ToString(), NewPosition = updatedPosition.ToString() };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support DockPattern");
                    }
                }, timeoutSeconds, $"Dock_{dockPosition}_{elementId}");

                _logger.LogInformation("Element docked successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Element docked to {dockPosition} successfully", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dock element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}