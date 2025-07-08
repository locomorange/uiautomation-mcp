using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Server.Services
{
    public class SubprocessBasedElementSearchService : IElementSearchService
    {
        private readonly ILogger<SubprocessBasedElementSearchService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedElementSearchService(
            ILogger<SubprocessBasedElementSearchService> logger,
            SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Finding elements with WindowTitle={WindowTitle}, SearchText={SearchText}, ControlType={ControlType}, ProcessId={ProcessId}",
                    windowTitle, searchText, controlType, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "searchText", searchText ?? "" },
                    { "controlType", controlType ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<List<ElementInfo>>("FindElements", parameters, timeoutSeconds);

                _logger.LogInformation("Found {Count} elements", result.Count);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetWindowsAsync(int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting desktop windows");

                var result = await _executor.ExecuteAsync<List<WindowInfo>>("GetDesktopWindows", null, timeoutSeconds);

                _logger.LogInformation("Found {Count} windows", result.Count);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get windows");
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}