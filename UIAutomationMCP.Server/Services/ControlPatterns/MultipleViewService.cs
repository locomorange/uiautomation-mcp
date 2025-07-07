using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class MultipleViewService : IMultipleViewService
    {
        private readonly ILogger<MultipleViewService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public MultipleViewService(ILogger<MultipleViewService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> GetAvailableViewsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting available views for element: {ElementId}", elementId);

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
                    if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) &&
                        multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                    {
                        var supportedViews = multipleViewPattern.Current.GetSupportedViews();
                        var currentView = multipleViewPattern.Current.CurrentView;

                        var views = supportedViews.Select(viewId => new
                        {
                            viewId,
                            viewName = multipleViewPattern.GetViewName(viewId),
                            isCurrent = viewId == currentView
                        }).ToArray();

                        return new
                        {
                            elementId,
                            currentView,
                            availableViews = views,
                            count = views.Length,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support MultipleViewPattern");
                    }
                }, timeoutSeconds, $"GetViews_{elementId}");

                _logger.LogInformation("Available views retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available views for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting view {ViewId} for element: {ElementId}", viewId, elementId);

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
                    if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) &&
                        multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                    {
                        var supportedViews = multipleViewPattern.Current.GetSupportedViews();
                        if (!supportedViews.Contains(viewId))
                        {
                            throw new InvalidOperationException($"View ID {viewId} is not supported. Available views: {string.Join(", ", supportedViews)}");
                        }

                        multipleViewPattern.SetCurrentView(viewId);
                        
                        // Verify the view was set
                        var currentView = multipleViewPattern.Current.CurrentView;
                        var viewName = multipleViewPattern.GetViewName(viewId);

                        return new
                        {
                            elementId,
                            requestedViewId = viewId,
                            currentViewId = currentView,
                            viewName,
                            success = currentView == viewId,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support MultipleViewPattern");
                    }
                }, timeoutSeconds, $"SetView_{elementId}");

                _logger.LogInformation("View set successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set view {ViewId} for element {ElementId}", viewId, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetCurrentViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting current view for element: {ElementId}", elementId);

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
                    if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) &&
                        multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                    {
                        var currentView = multipleViewPattern.Current.CurrentView;
                        var viewName = multipleViewPattern.GetViewName(currentView);

                        return new
                        {
                            elementId,
                            currentViewId = currentView,
                            viewName,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support MultipleViewPattern");
                    }
                }, timeoutSeconds, $"GetCurrentView_{elementId}");

                _logger.LogInformation("Current view retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current view for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetViewNameAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting view name for view {ViewId} in element: {ElementId}", viewId, elementId);

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
                    if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) &&
                        multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                    {
                        var viewName = multipleViewPattern.GetViewName(viewId);

                        return new
                        {
                            elementId,
                            viewId,
                            viewName,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support MultipleViewPattern");
                    }
                }, timeoutSeconds, $"GetViewName_{elementId}");

                _logger.LogInformation("View name retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get view name for view {ViewId} in element {ElementId}", viewId, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
