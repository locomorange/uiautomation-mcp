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
        private readonly IRangeService _rangeService;
        private readonly ISelectionService _selectionService;
        private readonly ITextService _textService;
        private readonly IToggleService _toggleService;
        private readonly IWindowService _windowService;
        private readonly ILayoutService _layoutService;

        public UIAutomationTools(
            IApplicationLauncher applicationLauncher,
            IScreenshotService screenshotService,
            IElementSearchService elementSearchService,
            ITreeNavigationService treeNavigationService,
            IInvokeService invokeService,
            IValueService valueService,
            IRangeService rangeService,
            ISelectionService selectionService,
            ITextService textService,
            IToggleService toggleService,
            IWindowService windowService,
            ILayoutService layoutService)
        {
            _applicationLauncher = applicationLauncher;
            _screenshotService = screenshotService;
            _elementSearchService = elementSearchService;
            _treeNavigationService = treeNavigationService;
            _invokeService = invokeService;
            _valueService = valueService;
            _rangeService = rangeService;
            _selectionService = selectionService;
            _textService = textService;
            _toggleService = toggleService;
            _windowService = windowService;
            _layoutService = layoutService;
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

        // Application Management
        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null, 
            [Description("Path to save the screenshot (optional)")] string? outputPath = null, 
            [Description("Maximum tokens for Base64 response (0 = no limit, auto-optimizes resolution and compression)")] int maxTokens = 0, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, processId, timeoutSeconds);


        [McpServerTool, Description("Launch a Win32 application by executable path")]
        public async Task<object> LaunchWin32Application(
            [Description("Path to the executable to launch")] string applicationPath,
            [Description("Command line arguments (optional)")] string? arguments = null,
            [Description("Working directory (optional)")] string? workingDirectory = null,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _applicationLauncher.LaunchWin32ApplicationAsync(applicationPath, arguments, workingDirectory, timeoutSeconds);

        [McpServerTool, Description("Launch a UWP application by shell:AppsFolder path")]
        public async Task<object> LaunchUWPApplication(
            [Description("shell:AppsFolder path to the UWP app")] string appsFolderPath,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _applicationLauncher.LaunchUWPApplicationAsync(appsFolderPath, timeoutSeconds);

        [McpServerTool, Description("Launch an application by searching for its display name")]
        public async Task<object> LaunchApplicationByName(
            [Description("Display name of the application to launch")] string applicationName,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => await _applicationLauncher.LaunchApplicationByNameAsync(applicationName, timeoutSeconds);

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

        [McpServerTool, Description("Toggle a checkbox or toggle element using TogglePattern")]
        public async Task<object> ToggleElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _toggleService.ToggleElementAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Select an element in a list, tab, or tree using SelectionItemPattern")]
        public async Task<object> SelectElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _selectionService.SelectElementAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get the current selection from a container using SelectionPattern")]
        public async Task<object> GetSelection(
            [Description("Automation ID or name of the container element")] string containerElementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _selectionService.GetSelectionAsync(containerElementId, windowTitle, processId, timeoutSeconds);

        // Layout and Navigation Patterns
        [McpServerTool, Description("Expand or collapse an element using ExpandCollapsePattern")]
        public async Task<object> ExpandCollapseElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Action to perform: expand, collapse, toggle")] string action, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _layoutService.ExpandCollapseElementAsync(elementId, action, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Scroll an element using ScrollPattern")]
        public async Task<object> ScrollElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Direction to scroll: up, down, left, right, pageup, pagedown, pageleft, pageright")] string direction, 
            [Description("Amount to scroll (default: 1.0)")] double amount = 1.0, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _layoutService.ScrollElementAsync(elementId, direction, amount, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Scroll an element into view using ScrollItemPattern")]
        public async Task<object> ScrollElementIntoView(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _layoutService.ScrollElementIntoViewAsync(elementId, windowTitle, processId, timeoutSeconds);

        // Value and Range Patterns
        [McpServerTool, Description("Set the value of a range element (slider, progress bar) using RangeValuePattern")]
        public async Task<object> SetRangeValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Value to set within the element's range")] double value, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _rangeService.SetRangeValueAsync(elementId, value, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get the current value and range information from a range element using RangeValuePattern")]
        public async Task<object> GetRangeValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _rangeService.GetRangeValueAsync(elementId, windowTitle, processId, timeoutSeconds);

        // Window Management Patterns
        [McpServerTool, Description("Perform window actions (minimize, maximize, close, etc.) using WindowPattern")]
        public async Task<object> WindowAction(
            [Description("Action to perform: minimize, maximize, normal, restore, close")] string action, 
            [Description("Title of the window (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _windowService.WindowActionAsync(action, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Transform an element (move, resize, rotate) using TransformPattern")]
        public async Task<object> TransformElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Action to perform: move, resize, rotate")] string action, 
            [Description("X coordinate for move, or rotation degrees for rotate")] double x = 0, 
            [Description("Y coordinate for move")] double y = 0, 
            [Description("Width for resize")] double width = 0, 
            [Description("Height for resize")] double height = 0, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _windowService.TransformElementAsync(elementId, action, x, y, width, height, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Dock an element to a specific position using DockPattern")]
        public async Task<object> DockElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Dock position: top, bottom, left, right, fill, none")] string dockPosition, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _layoutService.DockElementAsync(elementId, dockPosition, windowTitle, processId, timeoutSeconds);

        // Text Pattern Operations
        [McpServerTool, Description("Get text content from an element using TextPattern")]
        public async Task<object> GetText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.GetTextAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Select text in an element using TextPattern")]
        public async Task<object> SelectText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Start index of the text to select")] int startIndex, 
            [Description("Length of text to select")] int length, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.SelectTextAsync(elementId, startIndex, length, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Find text in an element using TextPattern")]
        public async Task<object> FindText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Text to search for")] string searchText, 
            [Description("Search backward (default: false)")] bool backward = false, 
            [Description("Ignore case (default: true)")] bool ignoreCase = true, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.FindTextAsync(elementId, searchText, backward, ignoreCase, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get the current text selection from an element using TextPattern")]
        public async Task<object> GetTextSelection(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.GetTextSelectionAsync(elementId, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Set text content in an element using ValuePattern")]
        public async Task<object> SetText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Text to set")] string text, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.SetTextAsync(elementId, text, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Traverse text using TextPattern with various navigation units")]
        public async Task<object> TraverseText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Direction to traverse: character, word, line, paragraph, page, document (add '-back' suffix for backward movement)")] string direction, 
            [Description("Number of units to move (default: 1)")] int count = 1, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.TraverseTextAsync(elementId, direction, count, windowTitle, processId, timeoutSeconds);

        [McpServerTool, Description("Get text formatting attributes using TextPattern")]
        public async Task<object> GetTextAttributes(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => await _textService.GetTextAttributesAsync(elementId, windowTitle, processId, timeoutSeconds);
    }
}
