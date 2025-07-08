using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedSelectionService : ISelectionService
    {
        private readonly ILogger<SubprocessBasedSelectionService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedSelectionService(ILogger<SubprocessBasedSelectionService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Selecting element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SelectElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element selected successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element selected successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetSelectionAsync(string containerElementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting selection from container: {ContainerElementId}", containerElementId);

                var parameters = new Dictionary<string, object>
                {
                    { "containerElementId", containerElementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var selectedItems = await _executor.ExecuteAsync<List<object>>("GetSelection", parameters, timeoutSeconds);

                _logger.LogInformation("Selection retrieved successfully from container: {ContainerElementId}", containerElementId);
                return new { Success = true, Data = selectedItems };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get selection from container: {ContainerElementId}", containerElementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}