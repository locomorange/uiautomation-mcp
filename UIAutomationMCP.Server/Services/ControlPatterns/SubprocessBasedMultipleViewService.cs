using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedMultipleViewService : IMultipleViewService
    {
        private readonly ILogger<SubprocessBasedMultipleViewService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedMultipleViewService(ILogger<SubprocessBasedMultipleViewService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetAvailableViewsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting available views for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetAvailableViews", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "viewId", viewId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("SetView", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetCurrentView", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "viewId", viewId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetViewName", parameters, timeoutSeconds);

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
