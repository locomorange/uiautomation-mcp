using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// Strongly typed wrapper around SubprocessExecutor that eliminates object type usage
    /// </summary>
    public class TypedSubprocessExecutor : ITypedSubprocessExecutor
    {
        private readonly ISubprocessExecutor _executor;
        private readonly ILogger<TypedSubprocessExecutor> _logger;

        public TypedSubprocessExecutor(ISubprocessExecutor executor, ILogger<TypedSubprocessExecutor> logger)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Element Search Operations
        public async Task<ElementSearchResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("searchText", searchText),
                ("controlType", controlType),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ElementSearchResult>("FindElements", parameters, timeoutSeconds);
        }

        public async Task<ElementSearchResult> FindElementsByControlTypeAsync(string controlType, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("controlType", controlType),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ElementSearchResult>("FindElementsByControlType", parameters, timeoutSeconds);
        }

        // Element Actions
        public async Task<ActionResult> InvokeElementAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("InvokeElement", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> ToggleElementAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("ToggleElement", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> SelectElementAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("SelectElement", parameters, timeoutSeconds);
        }

        // Element Values
        public async Task<ElementValueResult> GetElementValueAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ElementValueResult>("GetElementValue", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("value", value),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("SetElementValue", parameters, timeoutSeconds);
        }

        // Window Operations
        public async Task<WindowInfoResult> GetWindowInfoAsync(string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<WindowInfoResult>("GetWindowInfo", parameters, timeoutSeconds);
        }

        public async Task<WindowInteractionStateResult> GetWindowInteractionStateAsync(string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<WindowInteractionStateResult>("GetWindowInteractionState", parameters, timeoutSeconds);
        }

        public async Task<WindowCapabilitiesResult> GetWindowCapabilitiesAsync(string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<WindowCapabilitiesResult>("GetWindowCapabilities", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> WindowActionAsync(string action, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("action", action),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("WindowAction", parameters, timeoutSeconds);
        }

        // Range Operations
        public async Task<ElementValueResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ElementValueResult>("GetRangeValue", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("value", value),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("SetRangeValue", parameters, timeoutSeconds);
        }

        // Text Operations
        public async Task<TextInfoResult> GetTextAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<TextInfoResult>("GetText", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> SetTextAsync(string elementId, string text, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("text", text),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("SetText", parameters, timeoutSeconds);
        }

        // Grid Operations
        public async Task<GridInfoResult> GetGridInfoAsync(string gridElementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("gridElementId", gridElementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<GridInfoResult>("GetGridInfo", parameters, timeoutSeconds);
        }

        public async Task<ElementSearchResult> GetGridItemAsync(string gridElementId, int row, int column, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("gridElementId", gridElementId),
                ("row", row),
                ("column", column),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ElementSearchResult>("GetGridItem", parameters, timeoutSeconds);
        }

        // Table Operations
        public async Task<TableInfoResult> GetTableInfoAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<TableInfoResult>("GetTableInfo", parameters, timeoutSeconds);
        }

        // Selection Operations
        public async Task<SelectionInfoResult> GetSelectionAsync(string containerElementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("containerElementId", containerElementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<SelectionInfoResult>("GetSelection", parameters, timeoutSeconds);
        }

        public async Task<BooleanResult> IsSelectedAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<BooleanResult>("IsSelected", parameters, timeoutSeconds);
        }

        public async Task<BooleanResult> CanSelectMultipleAsync(string containerElementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("containerElementId", containerElementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<BooleanResult>("CanSelectMultiple", parameters, timeoutSeconds);
        }

        // Scroll Operations
        public async Task<ScrollInfoResult> GetScrollInfoAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ScrollInfoResult>("GetScrollInfo", parameters, timeoutSeconds);
        }

        public async Task<ActionResult> ScrollElementAsync(string elementId, string direction, double amount = 1.0, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30)
        {
            var parameters = CreateParameterDictionary(
                ("elementId", elementId),
                ("direction", direction),
                ("amount", amount),
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<ActionResult>("ScrollElement", parameters, timeoutSeconds);
        }

        // Screenshot
        public async Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, int processId = 0, string? outputPath = null, int maxTokens = 0, int timeoutSeconds = 60)
        {
            var parameters = CreateParameterDictionary(
                ("windowTitle", windowTitle),
                ("processId", processId),
                ("outputPath", outputPath),
                ("maxTokens", maxTokens),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<UIAutomationMCP.Shared.ScreenshotResult>("TakeScreenshot", parameters, timeoutSeconds);
        }

        // Process Operations
        public async Task<UIAutomationMCP.Shared.Results.ProcessResult> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60)
        {
            var parameters = CreateParameterDictionary(
                ("applicationName", applicationName),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<UIAutomationMCP.Shared.Results.ProcessResult>("LaunchApplicationByName", parameters, timeoutSeconds);
        }

        public async Task<UIAutomationMCP.Shared.Results.ProcessResult> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60)
        {
            var parameters = CreateParameterDictionary(
                ("applicationPath", applicationPath),
                ("arguments", arguments),
                ("workingDirectory", workingDirectory),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<UIAutomationMCP.Shared.Results.ProcessResult>("LaunchWin32Application", parameters, timeoutSeconds);
        }

        public async Task<UIAutomationMCP.Shared.Results.ProcessResult> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60)
        {
            var parameters = CreateParameterDictionary(
                ("appsFolderPath", appsFolderPath),
                ("timeoutSeconds", timeoutSeconds)
            );
            
            return await _executor.ExecuteAsync<UIAutomationMCP.Shared.Results.ProcessResult>("LaunchUWPApplication", parameters, timeoutSeconds);
        }

        // Helper method to create parameter dictionary while filtering out null values
        private static Dictionary<string, object> CreateParameterDictionary(params (string key, object? value)[] parameters)
        {
            var result = new Dictionary<string, object>();
            
            foreach (var (key, value) in parameters)
            {
                if (value != null)
                {
                    result[key] = value;
                }
            }
            
            return result;
        }
    }
}
