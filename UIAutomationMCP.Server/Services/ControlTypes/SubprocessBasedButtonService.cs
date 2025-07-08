using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class SubprocessBasedButtonService : IButtonService
    {
        private readonly ILogger<SubprocessBasedButtonService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedButtonService(ILogger<SubprocessBasedButtonService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> ButtonOperationAsync(string elementId, string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Button operation: {Operation} on element: {ElementId}", operation, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "operation", operation },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("ButtonOperation", parameters, timeoutSeconds);

                _logger.LogInformation("Button operation completed successfully: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform Button operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}