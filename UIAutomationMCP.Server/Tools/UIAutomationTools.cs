using ModelContextProtocol.Server;
using System.ComponentModel;
using UIAutomationMCP.Server.Services;

namespace UIAutomationMCP.Server.Tools
{
    [McpServerToolType]
    public class UIAutomationTools
    {
        private readonly IApplicationLauncher _applicationLauncher;
        private readonly IScreenshotService _screenshotService;
        private readonly IElementSearchService _elementSearchService;
        private readonly ITreeNavigationService _treeNavigationService;
        private readonly IInvokeService _invokeService;
        private readonly IValueService _valueService;

        public UIAutomationTools(
            IApplicationLauncher applicationLauncher,
            IScreenshotService screenshotService,
            IElementSearchService elementSearchService,
            ITreeNavigationService treeNavigationService,
            IInvokeService invokeService,
            IValueService valueService)
        {
            _applicationLauncher = applicationLauncher;
            _screenshotService = screenshotService;
            _elementSearchService = elementSearchService;
            _treeNavigationService = treeNavigationService;
            _invokeService = invokeService;
            _valueService = valueService;
        }

        // Window and Element Discovery
        [McpServerTool, Description("Get information about all open windows")]
        public async Task<object> GetWindowInfo([Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _elementSearchService.GetWindowsAsync(timeoutSeconds);

        [McpServerTool, Description("Get information about UI elements in a specific window")]
        public async Task<object> GetElementInfo(
            [Description("Title of the window to search in (optional)")] string? windowTitle = null, 
            [Description("Type of control to filter by (optional)")] string? controlType = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _elementSearchService.FindElementsAsync(windowTitle, null, controlType, processId, timeoutSeconds);

        [McpServerTool, Description("Find UI elements by various criteria")]
        public async Task<object> FindElements(
            [Description("Text to search for in element names or automation IDs (optional)")] string? searchText = null, 
            [Description("Type of control to filter by (optional)")] string? controlType = null, 
            [Description("Title of the window to search in (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _elementSearchService.FindElementsAsync(windowTitle, searchText, controlType, processId, timeoutSeconds);

        [McpServerTool, Description("Get the element tree structure for navigation and analysis")]
        public async Task<object> GetElementTree(
            [Description("Title of the window to get tree for (optional, defaults to all windows)")] string? windowTitle = null, 
            [Description("Maximum depth to traverse (default: 3)")] int maxDepth = 3, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _treeNavigationService.GetElementTreeAsync(windowTitle, processId, maxDepth, timeoutSeconds);

        // GetElementProperties - Method removed due to service deletion

        // GetElementPatterns - Method removed due to service deletion

        // Application Management
        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null, 
            [Description("Path to save the screenshot (optional)")] string? outputPath = null, 
            [Description("Maximum tokens for Base64 response (0 = no limit, auto-optimizes resolution and compression)")] int maxTokens = 0, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, processId, timeoutSeconds);

        [McpServerTool, Description("Launch an application by executable path or name")]
        public async Task<object> LaunchApplication(
            [Description("Path to the executable to launch")] string applicationPath, 
            [Description("Command line arguments (optional)")] string? arguments = null, 
            [Description("Working directory (optional)")] string? workingDirectory = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _applicationLauncher.LaunchApplicationAsync(applicationPath, arguments, workingDirectory, timeoutSeconds);

        // Core Interaction Patterns
        [McpServerTool, Description("Invoke an element (click button, activate menu item) using InvokePattern")]
        public async Task<object> InvokeElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _invokeService.InvokeElementAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Value to set")] string value, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _valueService.SetElementValueAsync(elementId, value, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get the current value of an element using ValuePattern")]
        public async Task<object> GetElementValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _valueService.GetElementValueAsync(elementId, windowTitle, processId, timeoutSeconds);

        // ToggleElement - Method removed due to service deletion

        // SelectElement - Method removed due to service deletion

        // Layout and Navigation Patterns
        // ExpandCollapseElement - Method removed due to service deletion

        // ScrollElement - Method removed due to service deletion

        // ScrollElementIntoView - Method removed due to service deletion

        // Value and Range Patterns
        // SetRangeValue - Method removed due to service deletion

        // GetRangeValue - Method removed due to service deletion

        // Window Management Patterns
        // WindowAction - Method removed due to service deletion

        // TransformElement - Method removed due to service deletion

        // DockElement - Method removed due to service deletion

        // Text Pattern Operations
        // GetText - Method removed due to service deletion

        // SelectText - Method removed due to service deletion

        // FindText - Method removed due to service deletion

        // GetTextSelection - Method removed due to service deletion
    }
}
