using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class SubprocessBasedTreeNavigationService : ITreeNavigationService
    {
        private readonly ILogger<SubprocessBasedTreeNavigationService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedTreeNavigationService(
            ILogger<SubprocessBasedTreeNavigationService> logger,
            SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetChildrenAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting children for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetChildren", parameters, timeoutSeconds);

                _logger.LogInformation("Got children successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get children for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetParentAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting parent for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetParent", parameters, timeoutSeconds);

                _logger.LogInformation("Got parent successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get parent for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetSiblingsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting siblings for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetSiblings", parameters, timeoutSeconds);

                _logger.LogInformation("Got siblings successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get siblings for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetDescendantsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting descendants for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetDescendants", parameters, timeoutSeconds);

                _logger.LogInformation("Got descendants successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get descendants for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetAncestorsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting ancestors for element: {ElementId}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    elementId, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetAncestors", parameters, timeoutSeconds);

                _logger.LogInformation("Got ancestors successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ancestors for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting element tree with WindowTitle={WindowTitle}, ProcessId={ProcessId}, MaxDepth={MaxDepth}",
                    windowTitle, processId, maxDepth);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "maxDepth", maxDepth }
                };

                var result = await _executor.ExecuteAsync<object>("GetElementTree", parameters, timeoutSeconds);

                _logger.LogInformation("Element tree built successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}