using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class CustomPropertyService : ICustomPropertyService
    {
        private readonly ILogger<CustomPropertyService> _logger;
        private readonly SubprocessExecutor _executor;

        public CustomPropertyService(ILogger<CustomPropertyService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetCustomPropertiesAsync(string elementId, string[] propertyIds, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting custom properties for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "propertyIds", propertyIds },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetCustomProperties", parameters, timeoutSeconds);

                _logger.LogInformation("Custom properties retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get custom properties for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetCustomPropertyAsync(string elementId, string propertyId, object value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting custom property for element: {ElementId}, Property: {PropertyId}", elementId, propertyId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "propertyId", propertyId },
                    { "value", value },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("SetCustomProperty", parameters, timeoutSeconds);

                _logger.LogInformation("Custom property set successfully for element: {ElementId}, Property: {PropertyId}", elementId, propertyId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set custom property {PropertyId} for element: {ElementId}", propertyId, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
