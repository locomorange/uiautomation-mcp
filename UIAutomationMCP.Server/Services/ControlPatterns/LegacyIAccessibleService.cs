using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class LegacyIAccessibleService : ILegacyIAccessibleService
    {
        private readonly ILogger<LegacyIAccessibleService> _logger;
        private readonly SubprocessExecutor _executor;

        public LegacyIAccessibleService(ILogger<LegacyIAccessibleService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetLegacyPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting legacy properties for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetLegacyProperties", parameters, timeoutSeconds);

                _logger.LogInformation("Legacy properties retrieved successfully for element: {ElementId}", elementId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get legacy properties for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> DoDefaultActionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing default action on element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("DoLegacyDefaultAction", parameters, timeoutSeconds);

                _logger.LogInformation("Default action performed successfully on element: {ElementId}", elementId);
                return new { Success = true, Message = "Default action performed successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform default action on element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SelectLegacyItemAsync(string elementId, int flagsSelect, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Selecting legacy item: {ElementId} with flags: {Flags}", elementId, flagsSelect);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "flagsSelect", flagsSelect },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SelectLegacyItem", parameters, timeoutSeconds);

                _logger.LogInformation("Legacy item selected successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Legacy item selected successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select legacy item: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetLegacyValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting legacy value for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "value", value },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetLegacyValue", parameters, timeoutSeconds);

                _logger.LogInformation("Legacy value set successfully for element: {ElementId}", elementId);
                return new { Success = true, Message = "Legacy value set successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set legacy value for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetLegacyStateAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting legacy state for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetLegacyState", parameters, timeoutSeconds);

                _logger.LogInformation("Legacy state retrieved successfully for element: {ElementId}", elementId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get legacy state for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}