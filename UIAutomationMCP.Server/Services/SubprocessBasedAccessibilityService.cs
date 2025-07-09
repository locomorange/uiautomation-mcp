using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class SubprocessBasedAccessibilityService : IAccessibilityService
    {
        private readonly ILogger<SubprocessBasedAccessibilityService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedAccessibilityService(ILogger<SubprocessBasedAccessibilityService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetAccessibilityInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting accessibility info for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetAccessibilityInfo", parameters, timeoutSeconds);

                _logger.LogInformation("Accessibility info retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get accessibility info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> VerifyAccessibilityAsync(string? elementId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Verifying accessibility for element: {ElementId}", elementId ?? "window");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("VerifyAccessibility", parameters, timeoutSeconds);

                _logger.LogInformation("Accessibility verification completed for element: {ElementId}", elementId ?? "window");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify accessibility for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetLabeledByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting labeled by info for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetLabeledBy", parameters, timeoutSeconds);

                _logger.LogInformation("Labeled by info retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get labeled by info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetDescribedByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting described by info for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetDescribedBy", parameters, timeoutSeconds);

                _logger.LogInformation("Described by info retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get described by info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
