using ModelContextProtocol.Server;
using System.ComponentModel;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Serialization;

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
        private readonly IGridService _gridService;
        private readonly ITableService _tableService;
        private readonly IMultipleViewService _multipleViewService;
        private readonly IAccessibilityService _accessibilityService;
        private readonly ICustomPropertyService _customPropertyService;
        private readonly IControlTypeService _controlTypeService;
        private readonly ITransformService _transformService;
        private readonly IVirtualizedItemService _virtualizedItemService;
        private readonly IItemContainerService _itemContainerService;
        private readonly ISynchronizedInputService _synchronizedInputService;
        private readonly IEventMonitorService _eventMonitorService;
        private readonly ISubprocessExecutor _subprocessExecutor;

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
            ILayoutService layoutService,
            IGridService gridService,
            ITableService tableService,
            IMultipleViewService multipleViewService,
            IAccessibilityService accessibilityService,
            ICustomPropertyService customPropertyService,
            IControlTypeService controlTypeService,
            ITransformService transformService,
            IVirtualizedItemService virtualizedItemService,
            IItemContainerService itemContainerService,
            ISynchronizedInputService synchronizedInputService,
            IEventMonitorService eventMonitorService,
            ISubprocessExecutor subprocessExecutor)
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
            _gridService = gridService;
            _tableService = tableService;
            _multipleViewService = multipleViewService;
            _accessibilityService = accessibilityService;
            _customPropertyService = customPropertyService;
            _controlTypeService = controlTypeService;
            _transformService = transformService;
            _virtualizedItemService = virtualizedItemService;
            _itemContainerService = itemContainerService;
            _synchronizedInputService = synchronizedInputService;
            _eventMonitorService = eventMonitorService;
            _subprocessExecutor = subprocessExecutor;
        }

        // Window and Element Discovery
        [McpServerTool, Description("Get information about all open windows")]
        public async Task<object> GetWindows([Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _elementSearchService.GetWindowsAsync(timeoutSeconds));



        [McpServerTool, Description("Find UI elements by various criteria with optional pattern validation (comprehensive element search tool)")]
        public async Task<object> FindElements(
            [Description("Text to search for in element names or automation IDs (optional)")] string? searchText = null, 
            [Description("Type of control to filter by (optional)")] string? controlType = null, 
            [Description("Title of the window to search in (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Search scope: children, descendants, subtree (default: descendants)")] string scope = "descendants",
            [Description("Validate control type patterns for quality assurance (default: true)")] bool validatePatterns = true,
            [Description("Maximum number of elements to return (default: 100)")] int maxResults = 100,
            [Description("Use caching for performance (default: true)")] bool useCache = true,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _elementSearchService.FindElementsAsync(windowTitle, searchText, controlType, processId, scope, validatePatterns, maxResults, useCache, timeoutSeconds));

        [McpServerTool, Description("Get the element tree structure for navigation and analysis")]
        public async Task<object> GetElementTree(
            [Description("Title of the window to get tree for (optional, defaults to all windows)")] string? windowTitle = null, 
            [Description("Maximum depth to traverse (default: 3)")] int maxDepth = 3, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _treeNavigationService.GetElementTreeAsync(windowTitle, processId, maxDepth, timeoutSeconds));


        // Application Management
        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null, 
            [Description("Path to save the screenshot (optional)")] string? outputPath = null, 
            [Description("Maximum tokens for Base64 response (0 = no limit, auto-optimizes resolution and compression)")] int maxTokens = 0, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, processId, timeoutSeconds));


        [McpServerTool, Description("Launch a Win32 application by executable path")]
        public async Task<object> LaunchWin32Application(
            [Description("Path to the executable to launch")] string applicationPath,
            [Description("Command line arguments (optional)")] string? arguments = null,
            [Description("Working directory (optional)")] string? workingDirectory = null,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _applicationLauncher.LaunchWin32ApplicationAsync(applicationPath, arguments, workingDirectory, timeoutSeconds));


        [McpServerTool, Description("Launch a UWP application by shell:AppsFolder path")]
        public async Task<object> LaunchUWPApplication(
            [Description("shell:AppsFolder path to the UWP app")] string appsFolderPath,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _applicationLauncher.LaunchUWPApplicationAsync(appsFolderPath, timeoutSeconds));


        [McpServerTool, Description("Launch an application by searching for its display name")]
        public async Task<object> LaunchApplicationByName(
            [Description("Display name of the application to launch")] string applicationName,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _applicationLauncher.LaunchApplicationByNameAsync(applicationName, timeoutSeconds));

        // Core Interaction Patterns
        [McpServerTool, Description("Invoke an element (click button, activate menu item) using InvokePattern")]
        public async Task<object> InvokeElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _invokeService.InvokeElementAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Value to set")] string value, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _valueService.SetValueAsync(elementId, value, windowTitle, processId, timeoutSeconds));



        [McpServerTool, Description("Toggle a checkbox or toggle element using TogglePattern")]
        public async Task<object> ToggleElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _toggleService.ToggleElementAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Select an element in a list, tab, or tree using SelectionItemPattern")]
        public async Task<object> SelectElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.SelectItemAsync(elementId, windowTitle, processId, timeoutSeconds));

        // IsElementSelected and GetSelectionContainer merged into FindElements Properties field



        [McpServerTool, Description("Add element to selection using SelectionItemPattern")]
        public async Task<object> AddToSelection(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.AddToSelectionAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Remove element from selection using SelectionItemPattern")]
        public async Task<object> RemoveFromSelection(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.RemoveFromSelectionAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Clear all selections in a container")]
        public async Task<object> ClearSelection(
            [Description("Automation ID or name of the container element")] string containerElementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.ClearSelectionAsync(containerElementId, windowTitle, processId, timeoutSeconds));

        // Layout and Navigation Patterns
        [McpServerTool, Description("Expand or collapse an element using ExpandCollapsePattern")]
        public async Task<object> ExpandCollapseElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Action to perform: expand, collapse, toggle")] string action, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _layoutService.ExpandCollapseElementAsync(elementId, action, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Scroll an element using ScrollPattern")]
        public async Task<object> ScrollElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Direction to scroll: up, down, left, right, pageup, pagedown, pageleft, pageright")] string direction, 
            [Description("Amount to scroll (default: 1.0)")] double amount = 1.0, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _layoutService.ScrollElementAsync(elementId, direction, amount, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Scroll an element into view using ScrollItemPattern")]
        public async Task<object> ScrollElementIntoView(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _layoutService.ScrollElementIntoViewAsync(elementId, windowTitle, processId, timeoutSeconds));


        [McpServerTool, Description("Set scroll position by percentage using ScrollPattern")]
        public async Task<object> SetScrollPercent(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Horizontal scroll percentage (0-100, -1 for no change)")] double horizontalPercent,
            [Description("Vertical scroll percentage (0-100, -1 for no change)")] double verticalPercent,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _layoutService.SetScrollPercentAsync(elementId, horizontalPercent, verticalPercent, windowTitle, processId, timeoutSeconds));

        // Value and Range Patterns
        [McpServerTool, Description("Set the value of a range element (slider, progress bar) using RangeValuePattern")]
        public async Task<object> SetRangeValue(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Value to set within the element's range")] double value, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _rangeService.SetRangeValueAsync(elementId, value, windowTitle, processId, timeoutSeconds));


        // Window Management Patterns
        [McpServerTool, Description("Perform window actions (minimize, maximize, close, etc.) using WindowPattern")]
        public async Task<object> WindowAction(
            [Description("Action to perform: minimize, maximize, normal, restore, close")] string action, 
            [Description("Title of the window (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _windowService.WindowOperationAsync(action, windowTitle, processId, timeoutSeconds));

        // GetWindowInteractionState and GetWindowCapabilities merged into FindElements Properties field

        [McpServerTool, Description("Wait for window to become idle using WindowPattern")]
        public async Task<object> WaitForWindowInputIdle(
            [Description("Maximum time to wait in milliseconds (default: 10000)")] int timeoutMilliseconds = 10000,
            [Description("Title of the window (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds for operation (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _windowService.WaitForInputIdleAsync(timeoutMilliseconds, windowTitle, processId, timeoutSeconds));


        [McpServerTool, Description("Move an element to new coordinates using TransformPattern")]
        public async Task<object> MoveElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("X coordinate for move")] double x,
            [Description("Y coordinate for move")] double y,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _transformService.MoveElementAsync(elementId, x, y, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Resize an element using TransformPattern")]
        public async Task<object> ResizeElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("New width")] double width,
            [Description("New height")] double height,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _transformService.ResizeElementAsync(elementId, width, height, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Rotate an element using TransformPattern")]
        public async Task<object> RotateElement(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Rotation degrees")] double degrees,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _transformService.RotateElementAsync(elementId, degrees, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Dock an element to a specific position using DockPattern")]
        public async Task<object> DockElement(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Dock position: top, bottom, left, right, fill, none")] string dockPosition, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _layoutService.DockElementAsync(elementId, dockPosition, windowTitle, processId, timeoutSeconds));

        // Text Pattern Operations

        [McpServerTool, Description("Select text in an element using TextPattern")]
        public async Task<object> SelectText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Start index of the text to select")] int startIndex, 
            [Description("Length of text to select")] int length, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _textService.SelectTextAsync(elementId, startIndex, length, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Find text in an element using TextPattern")]
        public async Task<object> FindText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Text to search for")] string searchText, 
            [Description("Search backward (default: false)")] bool backward = false, 
            [Description("Ignore case (default: true)")] bool ignoreCase = true, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _textService.GetTextAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get the current text selection from an element using TextPattern")]
        public async Task<object> GetTextSelection(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _textService.GetSelectedTextAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Set text content in an element using ValuePattern")]
        public async Task<object> SetText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Text to set")] string text, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _textService.SetTextAsync(elementId, text, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Traverse text using TextPattern with various navigation units")]
        public async Task<object> TraverseText(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Direction to traverse: character, word, line, paragraph, page, document (add '-back' suffix for backward movement)")] string direction, 
            [Description("Number of units to move (default: 1)")] int count = 1, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _textService.SelectTextAsync(elementId, 0, count, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get text formatting attributes using TextPattern")]
        public async Task<object> GetTextAttributes(
            [Description("Automation ID or name of the element")] string elementId, 
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null, 
            [Description("Process ID of the target window (optional)")] int? processId = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _textService.GetTextAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Grid Pattern Operations
        [McpServerTool, Description("Get a specific grid item at row and column coordinates")]
        public async Task<object> GetGridItem(
            [Description("Automation ID or name of the grid element")] string gridElementId,
            [Description("Row index (0-based)")] int row,
            [Description("Column index (0-based)")] int column,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _gridService.GetGridItemAsync(gridElementId, row, column, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get the row header for a specific row in a grid")]
        public async Task<object> GetRowHeader(
            [Description("Automation ID or name of the grid element")] string gridElementId,
            [Description("Row index (0-based)")] int row,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _gridService.GetRowHeaderAsync(gridElementId, row, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get the column header for a specific column in a grid")]
        public async Task<object> GetColumnHeader(
            [Description("Automation ID or name of the grid element")] string gridElementId,
            [Description("Column index (0-based)")] int column,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _gridService.GetColumnHeaderAsync(gridElementId, column, windowTitle, processId, timeoutSeconds));

        // Table Pattern Operations
        [McpServerTool, Description("Get all row headers for a table")]
        public async Task<object> GetRowHeaders(
            [Description("Automation ID or name of the table element")] string tableElementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _tableService.GetRowHeadersAsync(tableElementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get all column headers for a table")]
        public async Task<object> GetColumnHeaders(
            [Description("Automation ID or name of the table element")] string tableElementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _tableService.GetColumnHeadersAsync(tableElementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get row header items for a table")]
        public async Task<object> GetRowHeaderItems(
            [Description("Automation ID or name of the table element")] string tableElementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _tableService.GetRowHeaderItemsAsync(tableElementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get column header items for a table")]
        public async Task<object> GetColumnHeaderItems(
            [Description("Automation ID or name of the table element")] string tableElementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _tableService.GetColumnHeaderItemsAsync(tableElementId, windowTitle, processId, timeoutSeconds));




        // MultipleView Pattern Operations
        [McpServerTool, Description("Get all available views for an element")]
        public async Task<object> GetAvailableViews(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _multipleViewService.GetAvailableViewsAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get the current view of an element")]
        public async Task<object> GetCurrentView(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _multipleViewService.GetCurrentViewAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Get the name of a specific view by ID")]
        public async Task<object> GetViewName(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("View ID to get name for")] int viewId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _multipleViewService.GetViewNameAsync(elementId, viewId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Set the current view of an element")]
        public async Task<object> SetView(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("View ID to set")] int viewId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _multipleViewService.SetViewAsync(elementId, viewId, windowTitle, processId, timeoutSeconds));


        // Accessibility Information

        [McpServerTool, Description("Verify accessibility compliance for a window or element")]
        public async Task<object> VerifyAccessibility(
            [Description("Automation ID or name of the element (optional, checks entire window if not specified)")] string? elementId = null,
            [Description("Title of the window to verify")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _accessibilityService.VerifyAccessibilityAsync(elementId, windowTitle, processId, timeoutSeconds));


        // Custom Properties and Events
        [McpServerTool, Description("Get custom properties from an element")]
        public async Task<object> GetCustomProperties(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Comma-separated list of custom property IDs to retrieve")] string propertyIds,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _customPropertyService.GetCustomPropertiesAsync(elementId, propertyIds.Split(','), windowTitle, processId, timeoutSeconds));

        // Control Type Operations

        [McpServerTool, Description("Validate if element supports expected patterns for its control type (quality assurance and debugging tool)")]
        public async Task<object> ValidateControlTypePatterns(
            [Description("Automation ID or name of the element to validate")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _controlTypeService.ValidateControlTypePatternsAsync(elementId, windowTitle, processId, timeoutSeconds));

        // VirtualizedItem Pattern
        [McpServerTool, Description("Realize a virtualized item to make it fully available in the UI Automation tree")]
        public async Task<object> RealizeVirtualizedItem(
            [Description("Automation ID or name of the virtualized element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _virtualizedItemService.RealizeItemAsync(elementId, windowTitle, processId, timeoutSeconds));


        // ItemContainer Pattern
        [McpServerTool, Description("Find an item in a container by property value (useful for searching in lists, trees, and grids)")]
        public async Task<object> FindItemByProperty(
            [Description("Automation ID or name of the container element")] string containerId,
            [Description("Property name to search by (e.g., 'Name', 'AutomationId', 'ControlType'). Leave empty to find any item.")] string? propertyName = null,
            [Description("Property value to match. Leave empty to find any item.")] string? value = null,
            [Description("Start search after this element ID (for continued searches)")] string? startAfterId = null,
            [Description("Title of the window containing the container (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _itemContainerService.FindItemByPropertyAsync(containerId, propertyName, value, startAfterId, windowTitle, processId, timeoutSeconds));

        // SynchronizedInput Pattern
        [McpServerTool, Description("Start listening for synchronized input on an element (mouse or keyboard events)")]
        public async Task<object> StartSynchronizedInput(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Input type to synchronize: 'KeyUp', 'KeyDown', 'LeftMouseUp', 'LeftMouseDown', 'RightMouseUp', 'RightMouseDown'")] string inputType,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _synchronizedInputService.StartListeningAsync(elementId, inputType, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Cancel synchronized input listening on an element")]
        public async Task<object> CancelSynchronizedInput(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _synchronizedInputService.CancelAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Event Monitoring Operations
        [McpServerTool, Description("Monitor UI events for a specified duration")]
        public async Task<object> MonitorEvents(
            [Description("Type of event to monitor (e.g., 'Focus', 'Invoke', 'Selection', 'Text')")] string eventType,
            [Description("Duration to monitor in seconds")] int duration,
            [Description("Automation ID or name of the element to monitor (optional)")] string? elementId = null,
            [Description("Title of the window to monitor (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.MonitorEventsAsync(eventType, duration, elementId, windowTitle, processId));

        [McpServerTool, Description("Start continuous event monitoring")]
        public async Task<object> StartEventMonitoring(
            [Description("Type of event to monitor (e.g., 'Focus', 'Invoke', 'Selection', 'Text')")] string eventType,
            [Description("Automation ID or name of the element to monitor (optional)")] string? elementId = null,
            [Description("Title of the window to monitor (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.StartEventMonitoringAsync(eventType, elementId, windowTitle, processId));

        [McpServerTool, Description("Stop continuous event monitoring")]
        public async Task<object> StopEventMonitoring()
            => JsonSerializationHelper.Serialize(await _eventMonitorService.StopEventMonitoringAsync());

        [McpServerTool, Description("Get the current event log")]
        public async Task<object> GetEventLog()
            => JsonSerializationHelper.Serialize(await _eventMonitorService.GetEventLogAsync());

        // Window Capabilities and State
        [McpServerTool, Description("Get window capabilities and properties")]
        public async Task<object> GetWindowCapabilities(
            [Description("Title of the window")] string windowTitle,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _elementSearchService.GetWindowsAsync(timeoutSeconds));

        [McpServerTool, Description("Get window interaction state")]
        public async Task<object> GetWindowInteractionState(
            [Description("Title of the window")] string windowTitle,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _elementSearchService.GetWindowsAsync(timeoutSeconds));

        // Element Selection State
        [McpServerTool, Description("Check if an element is selected")]
        public async Task<object> IsElementSelected(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.IsSelectedAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Element Value Operations
        [McpServerTool, Description("Get the value of an element")]
        public async Task<object> GetElementValue(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _valueService.GetValueAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Check if an element is read-only")]
        public async Task<object> IsElementReadOnly(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _valueService.IsReadOnlyAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Grid Information
        [McpServerTool, Description("Get grid information (row count, column count, etc.)")]
        public async Task<object> GetGridInfo(
            [Description("Automation ID or name of the grid element")] string gridElementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _gridService.GetGridInfoAsync(gridElementId, windowTitle, processId, timeoutSeconds));

        // Transform Capabilities
        [McpServerTool, Description("Get transform capabilities (can move, resize, rotate)")]
        public async Task<object> GetTransformCapabilities(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _transformService.GetTransformCapabilitiesAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Selection Operations
        [McpServerTool, Description("Get selection information")]
        public async Task<object> GetSelection(
            [Description("Automation ID or name of the selection container element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.GetSelectionAsync(elementId, windowTitle, processId, timeoutSeconds));

        [McpServerTool, Description("Check if element can select multiple items")]
        public async Task<object> CanSelectMultiple(
            [Description("Automation ID or name of the selection container element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.CanSelectMultipleAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Scroll Information
        [McpServerTool, Description("Get scroll information")]
        public async Task<object> GetScrollInfo(
            [Description("Automation ID or name of the scrollable element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _layoutService.GetScrollInfoAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Accessibility Information
        [McpServerTool, Description("Get accessibility information")]
        public async Task<object> GetAccessibilityInfo(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _accessibilityService.GetAccessibilityInfoAsync(elementId, windowTitle, processId, timeoutSeconds));

    }
}
