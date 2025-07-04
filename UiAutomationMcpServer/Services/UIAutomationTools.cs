using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services.Elements;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Tools
{
    [McpServerToolType]
    public class UIAutomationTools
    {
        private readonly IWindowService _windowService;
        private readonly IElementDiscoveryService _elementDiscoveryService;
        private readonly IElementTreeService _elementTreeService;
        private readonly IElementPropertiesService _elementPropertiesService;
        private readonly IUIAutomationWorker _uiAutomationWorker;
        private readonly IScreenshotService _screenshotService;
        private readonly ILogger<UIAutomationTools> _logger;

        public UIAutomationTools(
            IWindowService windowService,
            IElementDiscoveryService elementDiscoveryService,
            IElementTreeService elementTreeService,
            IElementPropertiesService elementPropertiesService,
            IUIAutomationWorker uiAutomationWorker,
            IScreenshotService screenshotService,
            ILogger<UIAutomationTools> logger)
        {
            _windowService = windowService;
            _elementDiscoveryService = elementDiscoveryService;
            _elementTreeService = elementTreeService;
            _elementPropertiesService = elementPropertiesService;
            _uiAutomationWorker = uiAutomationWorker;
            _screenshotService = screenshotService;
            _logger = logger;
        }

        #region Window and Element Discovery

        [McpServerTool, Description("Get information about all open windows")]
        public async Task<object> GetWindowInfo()
        {
            return await _windowService.GetWindowInfoAsync();
        }

        [McpServerTool, Description("Get information about UI elements in a specific window")]
        public async Task<object> GetElementInfo(
            [Description("Title of the window to search in (optional)")] string? windowTitle = null,
            [Description("Type of control to filter by (optional)")] string? controlType = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _elementDiscoveryService.GetElementInfoAsync(windowTitle, controlType, processId);
        }

        [McpServerTool, Description("Find UI elements by various criteria")]
        public async Task<object> FindElements(
            [Description("Text to search for in element names or automation IDs (optional)")] string? searchText = null,
            [Description("Type of control to filter by (optional)")] string? controlType = null,
            [Description("Title of the window to search in (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _elementDiscoveryService.FindElementsAsync(searchText, controlType, windowTitle, processId);
        }

        #endregion

        #region Application Management

        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null,
            [Description("Path to save the screenshot (optional)")] string? outputPath = null,
            [Description("Maximum tokens for Base64 response (0 = no limit, auto-optimizes resolution and compression)")] int maxTokens = 0,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, processId);
        }

        [McpServerTool, Description("Launch an application by executable path or name")]
        public async Task<object> LaunchApplication(
            [Description("Path to the executable to launch")] string applicationPath,
            [Description("Command line arguments (optional)")] string? arguments = null,
            [Description("Working directory (optional)")] string? workingDirectory = null)
        {
            return await _windowService.LaunchApplicationAsync(applicationPath, arguments, workingDirectory);
        }

        #endregion

        #region Core Interaction Patterns

        [McpServerTool, Description("Invoke an element (click button, activate menu item) using InvokePattern")]
        public async Task<object> InvokeElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.InvokeElementAsync(elementId, windowTitle, processId);
        }

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Value to set")] string value,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.SetElementValueAsync(elementId, value, windowTitle, processId);
        }

        [McpServerTool, Description("Get the current value of an element using ValuePattern")]
        public async Task<object> GetElementValue(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.GetElementValueAsync(elementId, windowTitle, processId);
        }

        [McpServerTool, Description("Toggle an element (checkbox, toggle button) using TogglePattern")]
        public async Task<object> ToggleElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.ToggleElementAsync(elementId, windowTitle, processId);
        }

        [McpServerTool, Description("Select an element (list item, tab item) using SelectionItemPattern")]
        public async Task<object> SelectElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.SelectElementAsync(elementId, windowTitle, processId);
        }

        #endregion

        #region Layout and Navigation Patterns

        [McpServerTool, Description("Expand or collapse an element (tree item, menu) using ExpandCollapsePattern")]
        public async Task<object> ExpandCollapseElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("True to expand, false to collapse, null to toggle (optional)")] bool? expand = null,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.ExpandCollapseElementAsync(elementId, expand, windowTitle, processId);
        }

        [McpServerTool, Description("Scroll an element using ScrollPattern")]
        public async Task<object> ScrollElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Direction to scroll: up, down, left, right (optional)")] string? direction = null,
            [Description("Horizontal scroll percentage (0-100, optional)")] double? horizontal = null,
            [Description("Vertical scroll percentage (0-100, optional)")] double? vertical = null,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.ScrollElementAsync(elementId, direction, horizontal, vertical, windowTitle, processId);
        }

        [McpServerTool, Description("Scroll an element into view using ScrollItemPattern")]
        public async Task<object> ScrollElementIntoView(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.ScrollElementIntoViewAsync(elementId, windowTitle, processId);
        }

        #endregion

        #region Value and Range Patterns

        [McpServerTool, Description("Set the value of a range control (slider, progress bar) using RangeValuePattern")]
        public async Task<object> SetRangeValue(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Value to set within the range")] double value,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.SetRangeValueAsync(elementId, value, windowTitle, processId);
        }

        [McpServerTool, Description("Get the current value and range information of a range control using RangeValuePattern")]
        public async Task<object> GetRangeValue(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.GetRangeValueAsync(elementId, windowTitle, processId);
        }

        #endregion

        #region Window Management Patterns

        [McpServerTool, Description("Perform window actions (close, minimize, maximize, normal) using WindowPattern")]
        public async Task<object> WindowAction(
            [Description("Automation ID or name of the window element")] string elementId,
            [Description("Action to perform: close, minimize, maximize, normal")] string action,
            [Description("Title of the window (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.SetWindowStateAsync(elementId, action, windowTitle, processId);
        }

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
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.TransformElementAsync(elementId, action, x, y, width, height, degrees, windowTitle, processId);
        }

        [McpServerTool, Description("Dock an element to a specific position using DockPattern")]
        public async Task<object> DockElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Position to dock: top, bottom, left, right, fill, none")] string position,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.DockElementAsync(elementId, position, windowTitle, processId);
        }

        #endregion

        #region Advanced Patterns


        #endregion

        #region Text Pattern - Complex text operations

        [McpServerTool, Description("Get text content from an element using TextPattern")]
        public async Task<object> GetText(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.GetTextAsync(elementId, windowTitle, processId);
        }

        [McpServerTool, Description("Select text in an element using TextPattern")]
        public async Task<object> SelectText(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Start index of the text selection")] int startIndex,
            [Description("Length of the text selection")] int length,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.SelectTextAsync(elementId, startIndex, length, windowTitle, processId);
        }

        [McpServerTool, Description("Find text within an element using TextPattern")]
        public async Task<object> FindText(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Text to search for")] string searchText,
            [Description("Search backward from current position (optional)")] bool backward = false,
            [Description("Ignore case during search (optional)")] bool ignoreCase = false,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.FindTextAsync(elementId, searchText, backward, ignoreCase, windowTitle, processId);
        }

        [McpServerTool, Description("Get current text selection from an element using TextPattern")]
        public async Task<object> GetTextSelection(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _uiAutomationWorker.GetTextSelectionAsync(elementId, windowTitle, processId);
        }

        #endregion

        #region Tree Navigation

        [McpServerTool, Description("Get the element tree structure for navigation and analysis")]
        public async Task<object> GetElementTree(
            [Description("Title of the window to get tree for (optional, defaults to all windows)")] string? windowTitle = null,
            [Description("Tree view type: raw, control, content (default: control)")] string treeView = "control",
            [Description("Maximum depth to traverse (default: 3)")] int maxDepth = 3,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _elementTreeService.GetElementTreeAsync(windowTitle, treeView, maxDepth, processId);
        }

        [McpServerTool, Description("Get detailed properties of a specific element")]
        public async Task<object> GetElementProperties(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _elementPropertiesService.GetElementPropertiesAsync(elementId, windowTitle, processId);
        }

        [McpServerTool, Description("Get available control patterns for a specific element")]
        public async Task<object> GetElementPatterns(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
        {
            return await _elementPropertiesService.GetElementPatternsAsync(elementId, windowTitle, processId);
        }

        #endregion
    }
}