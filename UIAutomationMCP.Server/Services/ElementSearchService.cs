using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
using System.Text.Json;

namespace UIAutomationMCP.Server.Services
{
    public class ElementSearchService : IElementSearchService
    {
        private readonly ILogger<ElementSearchService> _logger;
        private readonly SubprocessExecutor _executor;

        public ElementSearchService(
            ILogger<ElementSearchService> logger,
            SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<JsonElement> FindElementAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30)
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
                return ConvertToJsonElement(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element");
                var errorResult = SubprocessErrorHandler.HandleError(ex, "FindElement", "", timeoutSeconds, _logger);
                return ConvertToJsonElement(errorResult);
            }
        }

        public async Task<JsonElement> FindAllElementsAsync(string? windowTitle = null, int? processId = null, string? name = null, string? automationId = null, string? className = null, string? controlType = null, int timeoutSeconds = 30)
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
                return ConvertToJsonElement(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find all elements");
                var errorResult = SubprocessErrorHandler.HandleError(ex, "FindAllElements", "", timeoutSeconds, _logger);
                return ConvertToJsonElement(errorResult);
            }
        }

        public async Task<JsonElement> FindElementByXPathAsync(string xpath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding element by XPath: {XPath}", xpath);

                var parameters = new Dictionary<string, object>
                {
                    { "xpath", xpath },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("FindElementByXPath", parameters, timeoutSeconds);

                _logger.LogInformation("Found element by XPath successfully");
                return ConvertToJsonElement(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by XPath");
                var errorResult = SubprocessErrorHandler.HandleError(ex, "FindElementByXPath", "", timeoutSeconds, _logger);
                return ConvertToJsonElement(errorResult);
            }
        }

        public async Task<JsonElement> FindElementsByTagNameAsync(string tagName, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding elements by tag name: {TagName}", tagName);

                var parameters = new Dictionary<string, object>
                {
                    { "tagName", tagName },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("FindElementsByTagName", parameters, timeoutSeconds);

                _logger.LogInformation("Found elements by tag name successfully");
                return ConvertToJsonElement(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements by tag name");
                var errorResult = SubprocessErrorHandler.HandleError(ex, "FindElementsByTagName", "", timeoutSeconds, _logger);
                return ConvertToJsonElement(errorResult);
            }
        }

        public async Task<JsonElement> GetWindowsAsync(int timeoutSeconds = 60)
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
                return ConvertToJsonElement(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get desktop windows");
                var errorResult = SubprocessErrorHandler.HandleError(ex, "GetDesktopWindows", "", timeoutSeconds, _logger);
                return ConvertToJsonElement(errorResult);
            }
        }

        public async Task<JsonElement> FindElementsAsync(string? windowTitle = null, string? searchText = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
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
                return ConvertToJsonElement(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                var errorResult = SubprocessErrorHandler.HandleError(ex, "FindElements", "", timeoutSeconds, _logger);
                return ConvertToJsonElement(errorResult);
            }
        }

        private static JsonElement ConvertToJsonElement(object obj)
        {
            var json = JsonSerializationHelper.SerializeObject(obj);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
    }
}
