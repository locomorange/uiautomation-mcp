using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class SubprocessBasedElementInspectionService : IElementInspectionService
    {
        private readonly ILogger<SubprocessBasedElementInspectionService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedElementInspectionService(
            ILogger<SubprocessBasedElementInspectionService> logger,
            SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting properties for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var properties = await _executor.ExecuteAsync<object>("GetElementProperties", parameters, timeoutSeconds);

                _logger.LogInformation("Properties retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = properties };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get properties for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting patterns for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var patterns = await _executor.ExecuteAsync<List<object>>("GetElementPatterns", parameters, timeoutSeconds);

                _logger.LogInformation("Patterns retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = patterns };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get patterns for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
