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

        [McpServerTool, Description("Search for UI elements with optional detailed information. Returns basic ElementInfo by default, or comprehensive details when includeDetails=true. Replaces the need for separate GetElementDetails calls.")]
        public async Task<object> SearchElements(
            [Description("Cross-property search text (searches Name, AutomationId, ClassName)")] string? searchText = null,
            [Description("Specific AutomationId to search for")] string? automationId = null, 
            [Description("Specific Name (display name) to search for")] string? name = null,
            [Description("Control type filter (Button, Slider, TextBox, etc.)")] string? controlType = null,
            [Description("Class name filter")] string? className = null,
            [Description("Window title filter")] string? windowTitle = null,
            [Description("Process ID filter")] int? processId = null,
            [Description("Search scope: children, descendants, subtree (default: descendants)")] string scope = "descendants",
            [Description("Required UI Automation pattern (only one supported for now)")] string? requiredPattern = null,
            [Description("Only return visible elements (default: true)")] bool visibleOnly = true,
            [Description("Enable fuzzy matching for text searches (default: false)")] bool fuzzyMatch = false,
            [Description("Only return enabled elements (default: false)")] bool enabledOnly = false,
            [Description("Maximum number of results to return (default: 50)")] int maxResults = 50,
            [Description("Sort results by: Name, ControlType, Position (optional)")] string? sortBy = null,
            [Description("Include detailed pattern information, accessibility data, and hierarchy (default: false)")] bool includeDetails = false,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
        {
            var request = new UIAutomationMCP.Shared.Requests.SearchElementsRequest
            {
                SearchText = searchText,
                AutomationId = automationId,
                Name = name,
                ControlType = controlType,
                ClassName = className,
                WindowTitle = windowTitle,
                ProcessId = processId,
                Scope = scope,
                RequiredPatterns = requiredPattern,
                AnyOfPatterns = null,
                VisibleOnly = visibleOnly,
                FuzzyMatch = fuzzyMatch,
                EnabledOnly = enabledOnly,
                MaxResults = maxResults,
                SortBy = sortBy,
                IncludeDetails = includeDetails,
                TimeoutSeconds = timeoutSeconds
            };
            
            return JsonSerializationHelper.Serialize(await _elementSearchService.SearchElementsAsync(request));
        }


        [McpServerTool, Description("Get detailed information about UI elements including pattern states (Toggle, Selection, Value), properties, and accessibility info. This is the primary tool for both element discovery and state inspection.")]
        public async Task<object> GetElementInfo(
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
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Button, MenuItem, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _invokeService.InvokeElementAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("Value to set")] string value,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (TextBox, Edit, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _valueService.SetValueAsync(
                elementId ?? automationId ?? name ?? "", 
                value,
                null, // windowTitle removed
                processId, 
                timeoutSeconds));



        [McpServerTool, Description("Toggle a checkbox or toggle element using TogglePattern")]
        public async Task<object> ToggleElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (CheckBox, ToggleButton, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _toggleService.ToggleElementAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Select an element in a list, tab, or tree using SelectionItemPattern")]
        public async Task<object> SelectElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TabItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _selectionService.SelectItemAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        // IsElementSelected and GetSelectionContainer merged into FindElements Properties field



        [McpServerTool, Description("Add element to selection using SelectionItemPattern")]
        public async Task<object> AddToSelection(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _selectionService.AddToSelectionAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Remove element from selection using SelectionItemPattern")]
        public async Task<object> RemoveFromSelection(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _selectionService.RemoveFromSelectionAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

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
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Action to perform: expand, collapse, toggle")] string action = "toggle",
            [Description("ControlType to filter by (TreeItem, MenuItem, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.ExpandCollapseElementAsync(
                elementId ?? automationId ?? name ?? "", 
                action, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Scroll an element using ScrollPattern")]
        public async Task<object> ScrollElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Direction to scroll: up, down, left, right, pageup, pagedown, pageleft, pageright")] string direction = "down",
            [Description("Amount to scroll (default: 1.0)")] double amount = 1.0,
            [Description("ControlType to filter by (ScrollViewer, ListBox, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.ScrollElementAsync(
                elementId ?? automationId ?? name ?? "", 
                direction, 
                amount, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Scroll an element into view using ScrollItemPattern")]
        public async Task<object> ScrollElementIntoView(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.ScrollElementIntoViewAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));


        [McpServerTool, Description("Set scroll position by percentage using ScrollPattern")]
        public async Task<object> SetScrollPercent(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Horizontal scroll percentage (0-100, -1 for no change)")] double horizontalPercent = -1,
            [Description("Vertical scroll percentage (0-100, -1 for no change)")] double verticalPercent = -1,
            [Description("ControlType to filter by (ScrollViewer, ListBox, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.SetScrollPercentAsync(
                elementId ?? automationId ?? name ?? "", 
                horizontalPercent, 
                verticalPercent, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        // Value and Range Patterns
        [McpServerTool, Description("Set the value of a range element (slider, progress bar) using RangeValuePattern")]
        public async Task<object> SetRangeValue(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Value to set within the element's range")] double value = 0,
            [Description("ControlType to filter by (Slider, ProgressBar, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _rangeService.SetRangeValueAsync(
                elementId ?? automationId ?? name ?? "", 
                value, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));


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
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("X coordinate for move")] double x = 0,
            [Description("Y coordinate for move")] double y = 0,
            [Description("ControlType to filter by (Window, Pane, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _transformService.MoveElementAsync(
                elementId ?? automationId ?? name ?? "", 
                x, 
                y, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Resize an element using TransformPattern")]
        public async Task<object> ResizeElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("New width")] double width = 100,
            [Description("New height")] double height = 100,
            [Description("ControlType to filter by (Window, Pane, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _transformService.ResizeElementAsync(
                elementId ?? automationId ?? name ?? "", 
                width, 
                height, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Rotate an element using TransformPattern")]
        public async Task<object> RotateElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Rotation degrees")] double degrees = 0,
            [Description("ControlType to filter by (Window, Pane, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _transformService.RotateElementAsync(
                elementId ?? automationId ?? name ?? "", 
                degrees, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Dock an element to a specific position using DockPattern")]
        public async Task<object> DockElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Dock position: top, bottom, left, right, fill, none")] string dockPosition = "none",
            [Description("ControlType to filter by (Pane, ToolBar, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.DockElementAsync(
                elementId ?? automationId ?? name ?? "", 
                dockPosition, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        // Text Pattern Operations

        [McpServerTool, Description("Select text in an element using TextPattern")]
        public async Task<object> SelectText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Start index of the text to select")] int startIndex = 0,
            [Description("Length of text to select")] int length = 1,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.SelectTextAsync(
                elementId ?? automationId ?? name ?? "", 
                startIndex, 
                length, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Find text in an element using TextPattern")]
        public async Task<object> FindText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Text to search for")] string searchText = "",
            [Description("Search backward (default: false)")] bool backward = false,
            [Description("Ignore case (default: true)")] bool ignoreCase = true,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.GetTextAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Get the current text selection from an element using TextPattern")]
        public async Task<object> GetTextSelection(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.GetSelectedTextAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Set text content in an element using ValuePattern")]
        public async Task<object> SetText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Text to set")] string text = "",
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.SetTextAsync(
                elementId ?? automationId ?? name ?? "", 
                text, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Traverse text using TextPattern with various navigation units")]
        public async Task<object> TraverseText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Direction to traverse: character, word, line, paragraph, page, document (add '-back' suffix for backward movement)")] string direction = "character",
            [Description("Number of units to move (default: 1)")] int count = 1,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.SelectTextAsync(
                elementId ?? automationId ?? name ?? "", 
                0, 
                count, 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        [McpServerTool, Description("Get text formatting attributes using TextPattern")]
        public async Task<object> GetTextAttributes(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.GetTextAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

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

        [McpServerTool, Description("Check if selection is required")]
        public async Task<object> IsSelectionRequired(
            [Description("Automation ID or name of the selection container element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.IsSelectionRequiredAsync(elementId, windowTitle, processId, timeoutSeconds));

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

        [McpServerTool, Description("Get labeled by relationship information")]
        public async Task<object> GetLabeledBy(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _accessibilityService.GetLabeledByAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Range Value Operations
        [McpServerTool, Description("Get range value information")]
        public async Task<object> GetRangeValue(
            [Description("Automation ID or name of the range element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _rangeService.GetRangeValueAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Text Operations
        [McpServerTool, Description("Get text content from an element")]
        public async Task<object> GetText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Edit, Document, Text, etc.)")] string? controlType = null,
            [Description("Process ID to limit search scope")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.GetTextAsync(
                elementId ?? automationId ?? name ?? "", 
                null, // windowTitle removed
                processId, 
                timeoutSeconds));

        // Table Operations
        [McpServerTool, Description("Get table information from an element")]
        public async Task<object> GetTableInfo(
            [Description("Automation ID or name of the table element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _tableService.GetTableInfoAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Selection Operations
        [McpServerTool, Description("Get selection container information")]
        public async Task<object> GetSelectionContainer(
            [Description("Element ID or container ID")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.GetSelectionContainerAsync(elementId, windowTitle, processId, timeoutSeconds));

        // Transform Operations
        [McpServerTool, Description("Get transform pattern capabilities")]
        public async Task<object> GetTransformPattern(
            [Description("Automation ID or name of the element")] string elementId,
            [Description("Title of the window containing the element (optional)")] string? windowTitle = null,
            [Description("Process ID of the target window (optional)")] int? processId = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _transformService.GetTransformCapabilitiesAsync(elementId, windowTitle, processId, timeoutSeconds));

    }
}
