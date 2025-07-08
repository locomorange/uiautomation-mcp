using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedToggleService : IToggleService
    {
        private readonly ILogger<SubprocessBasedToggleService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedToggleService(ILogger<SubprocessBasedToggleService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Toggling element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("ToggleElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element toggled successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element toggled successfully", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}