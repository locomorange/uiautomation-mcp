using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class SubprocessBasedHyperlinkService : IHyperlinkService
    {
        private readonly ILogger<SubprocessBasedHyperlinkService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedHyperlinkService(ILogger<SubprocessBasedHyperlinkService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> HyperlinkOperationAsync(string elementId, string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Hyperlink operation: {Operation} on element: {ElementId}", operation, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "operation", operation },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("HyperlinkOperation", parameters, timeoutSeconds);

                _logger.LogInformation("Hyperlink operation completed successfully: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform Hyperlink operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
