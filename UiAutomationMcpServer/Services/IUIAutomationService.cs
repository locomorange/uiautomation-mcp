using UiAutomationMcpServer.Models;

namespace UiAutomationMcpServer.Services
{
    public interface IUIAutomationService
    {
        // Window and Element Discovery
        Task<OperationResult> GetWindowInfoAsync();
        Task<OperationResult> GetElementInfoAsync(string? windowTitle = null, string? controlType = null, int? processId = null);
        Task<OperationResult> FindElementsAsync(string? searchText = null, string? controlType = null, string? windowTitle = null, int? processId = null);
        
        // Application Management
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null);
        Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null);
        
        // Core Interaction Patterns
        Task<OperationResult> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SetElementValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetElementValueAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SelectElementAsync(string elementId, string? windowTitle = null, int? processId = null);
        
        // Layout and Navigation Patterns
        Task<OperationResult> ExpandCollapseElementAsync(string elementId, bool? expand = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementAsync(string elementId, string? direction = null, double? horizontal = null, double? vertical = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null);
        
        // Value and Range Patterns
        Task<OperationResult> SetRangeValueAsync(string elementId, double value, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetRangeValueAsync(string elementId, string? windowTitle = null, int? processId = null);
        
        // Window Management Patterns
        Task<OperationResult> WindowActionAsync(string elementId, string action, string? windowTitle = null, int? processId = null);
        Task<OperationResult> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, double? degrees = null, string? windowTitle = null, int? processId = null);
        Task<OperationResult> DockElementAsync(string elementId, string position, string? windowTitle = null, int? processId = null);
        
        // Advanced Patterns
        Task<OperationResult> ChangeViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> RealizeVirtualizedItemAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> FindItemInContainerAsync(string elementId, string findText, string? windowTitle = null, int? processId = null);
        Task<OperationResult> CancelSynchronizedInputAsync(string elementId, string? windowTitle = null, int? processId = null);
        
        // Text Pattern - Complex text operations
        Task<OperationResult> GetTextAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> SelectTextAsync(string elementId, int startIndex, int length, string? windowTitle = null, int? processId = null);
        Task<OperationResult> FindTextAsync(string elementId, string searchText, bool backward = false, bool ignoreCase = false, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetTextSelectionAsync(string elementId, string? windowTitle = null, int? processId = null);
        
        // Tree Navigation
        Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null);
        Task<OperationResult> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null);
    }
}
