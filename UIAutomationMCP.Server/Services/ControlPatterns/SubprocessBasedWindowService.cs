using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedWindowService : IWindowService
    {
        private readonly ILogger<SubprocessBasedWindowService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedWindowService(ILogger<SubprocessBasedWindowService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> WindowActionAsync(string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing window action: {Action} on window: {WindowTitle}", action, windowTitle);

                var parameters = new Dictionary<string, object>
                {
                    { "action", action },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("WindowAction", parameters, timeoutSeconds);

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

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "action", action },
                    { "x", x ?? 0 },
                    { "y", y ?? 0 },
                    { "width", width ?? 0 },
                    { "height", height ?? 0 },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("TransformElement", parameters, timeoutSeconds);

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