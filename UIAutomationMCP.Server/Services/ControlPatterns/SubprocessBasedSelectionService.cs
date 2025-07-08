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

        public async Task<object> SelectItemAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
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

                await _executor.ExecuteAsync<object>("SelectItem", parameters, timeoutSeconds);

                _logger.LogInformation("Element selected successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element selected successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to select element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetSelectionAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting selection from container: {ContainerId}", containerId);

                var parameters = new Dictionary<string, object>
                {
                    { "containerId", containerId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var selectedItems = await _executor.ExecuteAsync<List<object>>("GetSelection", parameters, timeoutSeconds);

                _logger.LogInformation("Selection retrieved successfully from container: {ContainerId}", containerId);
                return new { Success = true, Data = selectedItems };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get selection from container: {ContainerId}", containerId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> AddToSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Adding element to selection: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("AddToSelection", parameters, timeoutSeconds);

                _logger.LogInformation("Element added to selection successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element added to selection successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add element to selection: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> RemoveFromSelectionAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Removing element from selection: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("RemoveFromSelection", parameters, timeoutSeconds);

                _logger.LogInformation("Element removed from selection successfully: {ElementId}", elementId);
                return new { Success = true, Message = "Element removed from selection successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove element from selection: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> ClearSelectionAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Clearing selection in container: {ContainerId}", containerId);

                var parameters = new Dictionary<string, object>
                {
                    { "containerId", containerId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("ClearSelection", parameters, timeoutSeconds);

                _logger.LogInformation("Selection cleared successfully in container: {ContainerId}", containerId);
                return new { Success = true, Message = "Selection cleared successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear selection in container: {ContainerId}", containerId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}