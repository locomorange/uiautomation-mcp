using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class AnnotationService : IAnnotationService
    {
        private readonly ILogger<AnnotationService> _logger;
        private readonly SubprocessExecutor _executor;

        public AnnotationService(ILogger<AnnotationService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetAnnotationInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting annotation info for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetAnnotationInfo", parameters, timeoutSeconds);

                _logger.LogInformation("Annotation info retrieved successfully for element: {ElementId}", elementId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get annotation info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetAnnotationTargetAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting annotation target for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetAnnotationTarget", parameters, timeoutSeconds);

                _logger.LogInformation("Annotation target retrieved successfully for element: {ElementId}", elementId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get annotation target for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}