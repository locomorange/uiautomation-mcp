using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedLayoutService : ILayoutService
    {
        private readonly ILogger<SubprocessBasedLayoutService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedLayoutService(ILogger<SubprocessBasedLayoutService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> ExpandCollapseElementAsync(string elementId, string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing expand/collapse action '{Action}' on element: {ElementId}", action, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "action", action },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("ExpandCollapseElement", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "direction", direction },
                    { "amount", amount },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("ScrollElement", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("ScrollElementIntoView", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "dockPosition", dockPosition },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("DockElement", parameters, timeoutSeconds);

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
