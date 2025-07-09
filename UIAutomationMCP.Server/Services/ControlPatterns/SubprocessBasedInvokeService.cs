using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedInvokeService : IInvokeService
    {
        private readonly ILogger<SubprocessBasedInvokeService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedInvokeService(ILogger<SubprocessBasedInvokeService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Invoking element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("InvokeElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element invoked successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element invoked successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invoke element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
