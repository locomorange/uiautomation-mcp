using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class VirtualizedItemService : IVirtualizedItemService
    {
        private readonly ILogger<VirtualizedItemService> _logger;
        private readonly ISubprocessExecutor _executor;

        public VirtualizedItemService(ILogger<VirtualizedItemService> logger, ISubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> RealizeItemAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Realizing virtualized item: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("RealizeVirtualizedItem", parameters, timeoutSeconds);

                _logger.LogInformation("Virtualized item realized successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Virtualized item realized successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to realize virtualized item: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}