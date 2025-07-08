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

                var tree = await _executor.ExecuteAsync<Dictionary<string, object>>("GetElementTree", parameters, timeoutSeconds);

                _logger.LogInformation("Element tree built successfully");
                return new { Success = true, Data = tree };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}