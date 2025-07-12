using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class SubprocessBasedControlTypeService : IControlTypeService
    {
        private readonly ILogger<SubprocessBasedControlTypeService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedControlTypeService(ILogger<SubprocessBasedControlTypeService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetControlTypeInfoAsync(
            string elementId, 
            bool validatePatterns = true, 
            bool includeDefaultProperties = true, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting control type info for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "validatePatterns", validatePatterns },
                    { "includeDefaultProperties", includeDefaultProperties }
                };

                var result = await _executor.ExecuteAsync<object>("GetControlTypeInfo", parameters, timeoutSeconds);

                _logger.LogInformation("Control type info retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get control type info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message, ElementId = elementId };
            }
        }

        public async Task<object> ValidateControlTypePatternsAsync(
            string elementId, 
            string? windowTitle = null, 
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Validating control type patterns for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("ValidateControlTypePatterns", parameters, timeoutSeconds);

                _logger.LogInformation("Control type patterns validated successfully for element: {ElementId}", elementId);
                return new { Success = true, ValidationResult = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate control type patterns for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message, ElementId = elementId };
            }
        }

        public async Task<object> FindElementsByControlTypeAsync(
            string controlType, 
            bool validatePatterns = true, 
            string scope = "descendants", 
            string? windowTitle = null, 
            int? processId = null, 
            int maxResults = 100, 
            int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding elements by control type: {ControlType}", controlType);

                var parameters = new Dictionary<string, object>
                {
                    { "controlType", controlType },
                    { "validatePatterns", validatePatterns },
                    { "scope", scope },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "maxResults", maxResults }
                };

                var result = await _executor.ExecuteAsync<object>("FindElementsByControlType", parameters, timeoutSeconds);

                _logger.LogInformation("Elements found successfully for control type: {ControlType}", controlType);
                return new { Success = true, Elements = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements by control type: {ControlType}", controlType);
                return new { Success = false, Error = ex.Message, ControlType = controlType };
            }
        }
    }
}