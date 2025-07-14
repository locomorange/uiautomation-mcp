using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared;

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

        public async Task<object> FindElementAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding element with WindowTitle={WindowTitle}, ProcessId={ProcessId}, Name={Name}, AutomationId={AutomationId}, ClassName={ClassName}, ControlType={ControlType}",
                    windowTitle, processId, name, automationId, className, controlType);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "name", name ?? "" },
                    { "automationId", automationId ?? "" },
                    { "className", className ?? "" },
                    { "controlType", controlType ?? "" }
                };

                var result = await _executor.ExecuteAsync<object>("FindElement", parameters, timeoutSeconds);

                _logger.LogInformation("Found element successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> FindAllElementsAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding all elements with WindowTitle={WindowTitle}, ProcessId={ProcessId}, Name={Name}, AutomationId={AutomationId}, ClassName={ClassName}, ControlType={ControlType}",
                    windowTitle, processId, name, automationId, className, controlType);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 },
                    { "name", name ?? "" },
                    { "automationId", automationId ?? "" },
                    { "className", className ?? "" },
                    { "controlType", controlType ?? "" }
                };

                var result = await _executor.ExecuteAsync<object>("FindAllElements", parameters, timeoutSeconds);

                _logger.LogInformation("Found all elements successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find all elements");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> FindElementByXPathAsync(string xpath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding element by XPath: {XPath}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    xpath, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "xpath", xpath },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("FindElementByXPath", parameters, timeoutSeconds);

                _logger.LogInformation("Found element by XPath successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by XPath");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> FindElementsByTagNameAsync(string tagName, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding elements by tag name: {TagName}, WindowTitle={WindowTitle}, ProcessId={ProcessId}",
                    tagName, windowTitle, processId);

                var parameters = new Dictionary<string, object>
                {
                    { "tagName", tagName },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("FindElementsByTagName", parameters, timeoutSeconds);

                _logger.LogInformation("Found elements by tag name successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements by tag name");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetWindowsAsync(int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Getting desktop windows");

                var parameters = new Dictionary<string, object>
                {
                    { "operation", "GetDesktopWindows" },
                    { "includeInvisible", false }
                };

                var result = await _executor.ExecuteAsync<object>("GetDesktopWindows", parameters, timeoutSeconds);

                _logger.LogInformation("Got desktop windows successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get desktop windows");
                return new { Success = false, Error = ex.Message };
            }
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

                var result = await _executor.ExecuteAsync<object>("FindElements", parameters, timeoutSeconds);

                _logger.LogInformation("Found elements successfully");
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
