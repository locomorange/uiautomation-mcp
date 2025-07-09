using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class SubprocessBasedTabService : ITabService
    {
        private readonly ILogger<SubprocessBasedTabService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedTabService(ILogger<SubprocessBasedTabService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> TabOperationAsync(string elementId, string operation, string? tabName = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Tab operation: {Operation} on element: {ElementId}", operation, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "operation", operation },
                    { "tabName", tabName ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("TabOperation", parameters, timeoutSeconds);

                _logger.LogInformation("Tab operation completed successfully: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform Tab operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
