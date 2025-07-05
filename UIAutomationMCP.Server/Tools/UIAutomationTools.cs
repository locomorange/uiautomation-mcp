using ModelContextProtocol.Server;
using System.ComponentModel;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Tools
{
    [McpServerToolType]
    public class UIAutomationTools
    {
        private readonly IApplicationLauncher _applicationLauncher;
        private readonly IUIAutomationService _uiAutomationService;
        private readonly IScreenshotService _screenshotService;

        public UIAutomationTools(
            IApplicationLauncher applicationLauncher,
            IUIAutomationService uiAutomationService,
            IScreenshotService screenshotService)
        {
            _applicationLauncher = applicationLauncher;
            _uiAutomationService = uiAutomationService;
            _screenshotService = screenshotService;
        }

        // Window and Element Discovery
        [McpServerTool, Description("Get information about all open windows")]
        public async Task<object> GetWindowInfo([Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _uiAutomationService.GetWindowsAsync(timeoutSeconds);

        [McpServerTool, Description("Get information about UI elements in a specific window")]
        public async Task<object> GetElementInfo(
            [Description("Title of the window to search in (optional)")] string? windowTitle = null, 
            [Description("Type of control to filter by (optional)")] string? controlType = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _uiAutomationService.FindElementsAsync(windowTitle, null, controlType, processId, timeoutSeconds);

        [McpServerTool, Description("Find UI elements by various criteria")]
        public async Task<object> FindElements(
            [Description("Text to search for in element names or automation IDs (optional)")] string? searchText = null, 
            [Description("Type of control to filter by (optional)")] string? controlType = null, 
            [Description("Title of the window to search in (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _uiAutomationService.FindElementsAsync(windowTitle, searchText, controlType, processId, timeoutSeconds);

        [McpServerTool, Description("Get the element tree structure for navigation and analysis")]
        public async Task<object> GetElementTree(
            [Description("Title of the window to get tree for (optional, defaults to all windows)")] string? windowTitle = null, 
            [Description("Maximum depth to traverse (default: 3)")] int maxDepth = 3, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _uiAutomationService.GetElementTreeAsync(windowTitle, processId, maxDepth, timeoutSeconds);

        [McpServerTool, Description("Get detailed properties of a specific element")]
        public async Task<object> GetElementProperties(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _uiAutomationService.GetElementPropertiesAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get available control patterns for a specific element")]
        public async Task<object> GetElementPatterns(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _uiAutomationService.GetElementPatternsAsync(elementId, windowTitle, processId, timeoutSeconds);

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
            => await _uiAutomationService.InvokeElementAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Value to set")] string value, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.SetElementValueAsync(elementId, value, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get the current value of an element using ValuePattern")]
        public async Task<object> GetElementValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.GetElementValueAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Toggle an element (checkbox, toggle button) using TogglePattern")]
        public async Task<object> ToggleElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.ToggleElementAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Select an element (list item, tab item) using SelectionItemPattern")]
        public async Task<object> SelectElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.SelectElementAsync(elementId, windowTitle, processId, timeoutSeconds);

        // Layout and Navigation Patterns
        [McpServerTool, Description("Expand or collapse an element (tree item, menu) using ExpandCollapsePattern")]
        public async Task<object> ExpandCollapseElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("True to expand, false to collapse, null to toggle (optional)")] bool? expand = null, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.ExpandCollapseElementAsync(elementId, expand, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Scroll an element using ScrollPattern")]
        public async Task<object> ScrollElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Direction to scroll: up, down, left, right (optional)")] string? direction = null, 
            [Description("Horizontal scroll percentage (0-100, optional)")] double? horizontal = null, 
            [Description("Vertical scroll percentage (0-100, optional)")] double? vertical = null, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.ScrollElementAsync(elementId, direction, horizontal, vertical, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Scroll an element into view using ScrollItemPattern")]
        public async Task<object> ScrollElementIntoView(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.ScrollElementIntoViewAsync(elementId, windowTitle, processId, timeoutSeconds);

        // Value and Range Patterns
        [McpServerTool, Description("Set the value of a range control (slider, progress bar) using RangeValuePattern")]
        public async Task<object> SetRangeValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Value to set within the range")] double value, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.SetRangeValueAsync(elementId, value, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get the current value and range information of a range control using RangeValuePattern")]
        public async Task<object> GetRangeValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.GetRangeValueAsync(elementId, windowTitle, processId, timeoutSeconds);

        // Window Management Patterns
        [McpServerTool, Description("Perform window actions (close, minimize, maximize, normal) using WindowPattern")]
        public async Task<object> WindowAction(
            [Description("Automation ID or name of the window element")] string elementId, 
            [Description("Action to perform: close, minimize, maximize, normal")] string action, 
            [Description("Title of the window (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.SetWindowStateAsync(elementId, action, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Transform an element (move, resize, rotate) using TransformPattern")]
        public async Task<object> TransformElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Action to perform: move, resize, rotate")] string action, 
            [Description("X coordinate for move action (optional)")] double? x = null, 
            [Description("Y coordinate for move action (optional)")] double? y = null, 
            [Description("Width for resize action (optional)")] double? width = null, 
            [Description("Height for resize action (optional)")] double? height = null, 
            [Description("Degrees for rotate action (optional)")] double? degrees = null, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.TransformElementAsync(elementId, action, x, y, width, height, degrees, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Dock an element to a specific position using DockPattern")]
        public async Task<object> DockElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Position to dock: top, bottom, left, right, fill, none")] string position, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.DockElementAsync(elementId, position, windowTitle, processId, timeoutSeconds);

        // Text Pattern Operations
        [McpServerTool, Description("Get text content from an element using TextPattern")]
        public async Task<object> GetText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.GetTextAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Select text in an element using TextPattern")]
        public async Task<object> SelectText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Start index of the text selection")] int startIndex, 
            [Description("Length of the text selection")] int length, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.SelectTextAsync(elementId, startIndex, length, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Find text within an element using TextPattern")]
        public async Task<object> FindText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Text to search for")] string searchText, 
            [Description("Search backward from current position (optional)")] bool backward = false, 
            [Description("Ignore case during search (optional)")] bool ignoreCase = false, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.FindTextAsync(elementId, searchText, backward, ignoreCase, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get current text selection from an element using TextPattern")]
        public async Task<object> GetTextSelection(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _uiAutomationService.GetTextSelectionAsync(elementId, windowTitle, processId, timeoutSeconds);
    }
}
