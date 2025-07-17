using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ValueService : IValueService
    {
        private readonly ILogger<ValueService> _logger;
        private readonly SubprocessExecutor _executor;

        public ValueService(ILogger<ValueService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> SetValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting element value: {ElementId} = {Value}", elementId, value);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "value", value },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetElementValue", parameters, timeoutSeconds);

                _logger.LogInformation("Element value set successfully: {ElementId}", elementId);
                return new { Success = true, Message = $"Element value set to: {value}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set element value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting element value: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<string>("GetElementValue", parameters, timeoutSeconds);

                _logger.LogInformation("Element value retrieved successfully: {ElementId}", elementId);
                return new { Success = true, Value = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get element value: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> IsReadOnlyAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Checking if element is read-only: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<bool>("IsReadOnly", parameters, timeoutSeconds);

                _logger.LogInformation("Read-only status retrieved successfully: {ElementId}", elementId);
                return new { Success = true, IsReadOnly = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check read-only status: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
