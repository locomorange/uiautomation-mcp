using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SynchronizedInputService : ISynchronizedInputService
    {
        private readonly ILogger<SynchronizedInputService> _logger;
        private readonly SubprocessExecutor _executor;

        public SynchronizedInputService(ILogger<SynchronizedInputService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> StartListeningAsync(string elementId, string inputType, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Starting synchronized input listening for element: {ElementId} with input type: {InputType}", elementId, inputType);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "inputType", inputType },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("StartSynchronizedInput", parameters, timeoutSeconds);

                _logger.LogInformation("Synchronized input listening started successfully for element: {ElementId}", elementId);
                return new { Success = true, Message = "Synchronized input listening started" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start synchronized input listening for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> CancelAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Canceling synchronized input for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("CancelSynchronizedInput", parameters, timeoutSeconds);

                _logger.LogInformation("Synchronized input canceled successfully for element: {ElementId}", elementId);
                return new { Success = true, Message = "Synchronized input canceled" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel synchronized input for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}