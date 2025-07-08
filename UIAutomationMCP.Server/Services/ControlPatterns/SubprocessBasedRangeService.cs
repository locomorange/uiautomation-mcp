using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedRangeService : IRangeService
    {
        private readonly ILogger<SubprocessBasedRangeService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedRangeService(ILogger<SubprocessBasedRangeService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting range value: {ElementId} = {Value}", elementId, value);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "value", value },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("SetRangeValue", parameters, timeoutSeconds);

                _logger.LogInformation("Range value set successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Range value set to: {value}", Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set range value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting range value: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var rangeInfo = await _executor.ExecuteAsync<object>("GetRangeValue", parameters, timeoutSeconds);

                _logger.LogInformation("Range value retrieved successfully: {ElementId}", elementId);
                return new { Success = true, Data = rangeInfo };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get range value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetRangePropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting range properties: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var rangeProperties = await _executor.ExecuteAsync<object>("GetRangeProperties", parameters, timeoutSeconds);

                _logger.LogInformation("Range properties retrieved successfully: {ElementId}", elementId);
                return new { Success = true, Data = rangeProperties };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get range properties: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}