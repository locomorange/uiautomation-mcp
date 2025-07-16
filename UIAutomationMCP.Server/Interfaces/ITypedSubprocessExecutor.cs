using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed subprocess executor interface that eliminates object type usage
    /// </summary>
    public interface ITypedSubprocessExecutor
    {
        // Element Search Operations
        Task<ElementSearchResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ElementSearchResult> FindElementsByControlTypeAsync(string controlType, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Element Actions
        Task<ActionResult> InvokeElementAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> ToggleElementAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> SelectElementAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Element Values
        Task<ElementValueResult> GetElementValueAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Window Operations
        Task<WindowInfoResult> GetWindowInfoAsync(string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<WindowInteractionStateResult> GetWindowInteractionStateAsync(string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<WindowCapabilitiesResult> GetWindowCapabilitiesAsync(string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> WindowActionAsync(string action, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Range Operations
        Task<ElementValueResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Text Operations
        Task<TextInfoResult> GetTextAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> SetTextAsync(string elementId, string text, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Grid Operations
        Task<GridInfoResult> GetGridInfoAsync(string gridElementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ElementSearchResult> GetGridItemAsync(string gridElementId, int row, int column, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Table Operations
        Task<TableInfoResult> GetTableInfoAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Selection Operations
        Task<SelectionInfoResult> GetSelectionAsync(string containerElementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<BooleanResult> IsSelectedAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<BooleanResult> CanSelectMultipleAsync(string containerElementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Scroll Operations
        Task<ScrollInfoResult> GetScrollInfoAsync(string elementId, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        Task<ActionResult> ScrollElementAsync(string elementId, string direction, double amount = 1.0, string? windowTitle = null, int processId = 0, int timeoutSeconds = 30);
        
        // Screenshot
        Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, int processId = 0, string? outputPath = null, int maxTokens = 0, int timeoutSeconds = 60);
        
        // Process Operations
        Task<ProcessResult> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60);
        Task<ProcessResult> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60);
        Task<ProcessResult> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60);
    }
}
