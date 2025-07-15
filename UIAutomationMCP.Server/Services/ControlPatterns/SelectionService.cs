using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SelectionService : ISelectionService
    {
        private readonly ILogger<SelectionService> _logger;
        private readonly ISubprocessExecutor _executor;

        public SelectionService(ILogger<SelectionService> logger, ISubprocessExecutor executor)
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

        public async Task<object> CanSelectMultipleAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Checking if container supports multiple selection: {ContainerId}", containerId);

                var parameters = new Dictionary<string, object>
                {
                    { "containerId", containerId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("CanSelectMultiple", parameters, timeoutSeconds);

                _logger.LogInformation("Multiple selection check completed for container: {ContainerId}", containerId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check multiple selection support for container: {ContainerId}", containerId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> IsSelectionRequiredAsync(string containerId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Checking if selection is required for container: {ContainerId}", containerId);

                var parameters = new Dictionary<string, object>
                {
                    { "containerId", containerId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("IsSelectionRequired", parameters, timeoutSeconds);

                _logger.LogInformation("Selection requirement check completed for container: {ContainerId}", containerId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check selection requirement for container: {ContainerId}", containerId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> IsSelectedAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Checking if element is selected: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("IsSelected", parameters, timeoutSeconds);

                _logger.LogInformation("Selection status check completed for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check selection status for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetSelectionContainerAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting selection container for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetSelectionContainer", parameters, timeoutSeconds);

                _logger.LogInformation("Selection container retrieved for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get selection container for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
