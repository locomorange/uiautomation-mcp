using ModelContextProtocol.Server;
using ModelContextProtocol;
using System.ComponentModel;
using UIAutomationMCP.Server.Services;
using UIAutomationMCP.Server.Services.ControlPatterns;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models.Logging;

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
        private readonly IFocusService _focusService;
        private readonly IMcpLogService _mcpLogService;

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
            IFocusService focusService,
            IMcpLogService mcpLogService)
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
            _focusService = focusService;
            _mcpLogService = mcpLogService;
        }

        // Window and Element Discovery

        [McpServerTool, Description("Search for UI elements with flexible filtering options. Returns basic element properties by default. When includeDetails=true, returns comprehensive data including: • All supported UI patterns (Toggle state, Range values, Window state, Selection info, Grid/Table structure, Scroll position, Text content, Transform capabilities, etc.) • Accessibility information (labeledBy, helpText, accessKey, acceleratorKey) • Advanced properties (frameworkId, runtimeId, isPassword) • Element hierarchy (parent and children relationships). For window detection, use scope='children' with requiredPattern='Window'.")]
        public async Task<object> SearchElements(
            [Description("Cross-property search text (searches Name, AutomationId, ClassName)")] string? searchText = null,
            [Description("Specific AutomationId to search for")] string? automationId = null, 
            [Description("Specific Name (display name) to search for")] string? name = null,
            [Description("Control type filter (Button, Slider, TextBox, etc.)")] string? controlType = null,
            [Description("Class name filter")] string? className = null,
            [Description("Window title filter")] string? windowTitle = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Search scope: children, descendants, subtree (default: descendants)")] string scope = "descendants",
            [Description("Required UI Automation pattern (only one supported for now)")] string? requiredPattern = null,
            [Description("Only return visible elements (default: true)")] bool visibleOnly = true,
            [Description("Enable fuzzy matching for text searches (default: false)")] bool fuzzyMatch = false,
            [Description("Only return enabled elements (default: false)")] bool enabledOnly = false,
            [Description("Maximum number of results to return (default: 50)")] int maxResults = 50,
            [Description("Sort results by: Name, ControlType, Position (optional)")] string? sortBy = null,
            [Description("Include comprehensive details: all UI patterns (Toggle, Range, Window, Selection, Grid, Scroll, Text, Transform, Value, ExpandCollapse, Dock, MultipleView, Table, etc.), accessibility info (labels, help text, keyboard shortcuts), and element hierarchy (default: false)")] bool includeDetails = false,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _elementSearchService.SearchElementsAsync(
                new UIAutomationMCP.Models.Requests.SearchElementsRequest
                {
                    SearchText = searchText,
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ClassName = className,
                    WindowTitle = windowTitle,
                    WindowHandle = windowHandle,
                    Scope = scope,
                    RequiredPatterns = requiredPattern,
                    AnyOfPatterns = null,
                    VisibleOnly = visibleOnly,
                    FuzzyMatch = fuzzyMatch,
                    EnabledOnly = enabledOnly,
                    MaxResults = maxResults,
                    SortBy = sortBy,
                    IncludeDetails = includeDetails
                }, timeoutSeconds));



        [McpServerTool, Description("Get the hierarchical element tree structure for navigation and overview. Returns basic ElementInfo without detailed pattern information. For detailed element analysis, use SearchElements with includeDetails=true.")]
        public async Task<object> GetElementTree(
            [Description("Maximum depth to traverse (default: 3)")] int maxDepth = 3, 
            [Description("Native window handle (HWND) for direct window targeting")] long? windowHandle = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _treeNavigationService.GetElementTreeAsync(windowHandle, maxDepth, timeoutSeconds));


        // Application Management
        [McpServerTool, Description("Take a screenshot of the desktop or specific window")]
        public async Task<object> TakeScreenshot(
            [Description("Title of the window to screenshot (optional, defaults to full screen)")] string? windowTitle = null, 
            [Description("Path to save the screenshot (optional)")] string? outputPath = null, 
            [Description("Maximum tokens for Base64 response (0 = no limit, auto-optimizes resolution and compression)")] int maxTokens = 0, 
            [Description("Native window handle (HWND) for direct window targeting")] long? windowHandle = null, 
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, windowHandle, timeoutSeconds));



        [McpServerTool, Description("Launch any application with automatic detection (Win32/UWP)")]
        public async Task<object> LaunchApplication(
            [Description("Application name, path, or identifier")] string application,
            [Description("Command line arguments (optional)")] string? arguments = null,
            [Description("Working directory (optional)")] string? workingDirectory = null,
            [Description("Timeout in seconds (default: 60)")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _applicationLauncher.LaunchApplicationAsync(application, arguments, workingDirectory, timeoutSeconds));

        // Core Interaction Patterns
        [McpServerTool, Description("Invoke an element (click button, activate menu item) using InvokePattern")]
        public async Task<object> InvokeElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Button, MenuItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _invokeService.InvokeElementAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle, 
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("Value to set")] string value,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (TextBox, Edit, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _valueService.SetValueAsync(
                value: value,
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle, 
                timeoutSeconds: timeoutSeconds));



        [McpServerTool, Description("Toggle a checkbox or toggle element using TogglePattern")]
        public async Task<object> ToggleElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (CheckBox, ToggleButton, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _toggleService.ToggleElementAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle, 
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Set focus to a UI element using UI Automation SetFocus method")]
        public async Task<object> SetFocus(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Edit, Button, etc.)")] string? controlType = null,
            [Description("Required pattern for element filtering (optional, e.g., 'value', 'invoke')")] string? requiredPattern = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _focusService.SetFocusAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                requiredPattern: requiredPattern,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Select an element in a list, tab, or tree using SelectionItemPattern")]
        public async Task<object> SelectElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TabItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _selectionService.SelectItemAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // IsElementSelected and GetSelectionContainer merged into FindElements Properties field



        [McpServerTool, Description("Add element to selection using SelectionItemPattern")]
        public async Task<object> AddToSelection(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _selectionService.AddToSelectionAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Remove element from selection using SelectionItemPattern")]
        public async Task<object> RemoveFromSelection(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _selectionService.RemoveFromSelectionAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Clear all selections in a container")]
        public async Task<object> ClearSelection(
            [Description("Automation ID or name of the container element")] string containerElementId, 
            [Description("Native window handle (HWND) for direct window targeting")] long? windowHandle = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _selectionService.ClearSelectionAsync(automationId: containerElementId, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds));

        // Layout and Navigation Patterns
        [McpServerTool, Description("Expand or collapse an element using ExpandCollapsePattern")]
        public async Task<object> ExpandCollapseElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Action to perform: expand, collapse, toggle")] string action = "toggle",
            [Description("ControlType to filter by (TreeItem, MenuItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.ExpandCollapseElementAsync(
                automationId: automationId,
                name: name,
                action: action,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Scroll an element using ScrollPattern")]
        public async Task<object> ScrollElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Direction to scroll: up, down, left, right, pageup, pagedown, pageleft, pageright")] string direction = "down",
            [Description("Amount to scroll (default: 1.0)")] double amount = 1.0,
            [Description("ControlType to filter by (ScrollViewer, ListBox, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.ScrollElementAsync(
                automationId: automationId,
                name: name,
                direction: direction,
                amount: amount,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Scroll an element into view using ScrollItemPattern")]
        public async Task<object> ScrollElementIntoView(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.ScrollElementIntoViewAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));


        [McpServerTool, Description("Set scroll position by percentage using ScrollPattern")]
        public async Task<object> SetScrollPercent(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Horizontal scroll percentage (0-100, -1 for no change)")] double horizontalPercent = -1,
            [Description("Vertical scroll percentage (0-100, -1 for no change)")] double verticalPercent = -1,
            [Description("ControlType to filter by (ScrollViewer, ListBox, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.SetScrollPercentAsync(
                automationId: automationId,
                name: name,
                horizontalPercent: horizontalPercent,
                verticalPercent: verticalPercent,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // Value and Range Patterns
        [McpServerTool, Description("Set the value of a range element (slider, progress bar) using RangeValuePattern")]
        public async Task<object> SetRangeValue(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Value to set within the element's range")] double value = 0,
            [Description("ControlType to filter by (Slider, ProgressBar, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _rangeService.SetRangeValueAsync(
                automationId: automationId,
                name: name,
                value: value,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));


        // Window Management Patterns
        [McpServerTool, Description("Perform window actions (minimize, maximize, close, etc.) using WindowPattern")]
        public async Task<object> WindowAction(
            [Description("Action to perform: minimize, maximize, normal, restore, close")] string action, 
            [Description("Title of the window (optional)")] string? windowTitle = null, 
            [Description("Native window handle (HWND) for direct window targeting")] long? windowHandle = null, 
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _windowService.WindowOperationAsync(action, windowTitle, windowHandle, timeoutSeconds));

        // GetWindowInteractionState and GetWindowCapabilities merged into FindElements Properties field

        [McpServerTool, Description("Wait for window to become idle using WindowPattern")]
        public async Task<object> WaitForWindowInputIdle(
            [Description("Maximum time to wait in milliseconds (default: 10000)")] int timeoutMilliseconds = 10000,
            [Description("Title of the window (optional)")] string? windowTitle = null,
            [Description("Native window handle (HWND) for direct window targeting")] long? windowHandle = null,
            [Description("Timeout in seconds for operation (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _windowService.WaitForInputIdleAsync(timeoutMilliseconds, windowTitle, windowHandle, timeoutSeconds));


        [McpServerTool, Description("Move an element to new coordinates using TransformPattern")]
        public async Task<object> MoveElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("X coordinate for move")] double x = 0,
            [Description("Y coordinate for move")] double y = 0,
            [Description("ControlType to filter by (Window, Pane, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _transformService.MoveElementAsync(
                automationId: automationId,
                name: name,
                x: x,
                y: y,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Resize an element using TransformPattern")]
        public async Task<object> ResizeElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("New width")] double width = 100,
            [Description("New height")] double height = 100,
            [Description("ControlType to filter by (Window, Pane, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _transformService.ResizeElementAsync(
                automationId: automationId,
                name: name,
                width: width,
                height: height,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Rotate an element using TransformPattern")]
        public async Task<object> RotateElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Rotation degrees")] double degrees = 0,
            [Description("ControlType to filter by (Window, Pane, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _transformService.RotateElementAsync(
                automationId: automationId,
                name: name,
                degrees: degrees,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Dock an element to a specific position using DockPattern")]
        public async Task<object> DockElement(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Dock position: top, bottom, left, right, fill, none")] string dockPosition = "none",
            [Description("ControlType to filter by (Pane, ToolBar, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _layoutService.DockElementAsync(
                automationId: automationId,
                name: name,
                dockPosition: dockPosition,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // Text Pattern Operations

        [McpServerTool, Description("Select text in an element using TextPattern")]
        public async Task<object> SelectText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Start index of the text to select")] int startIndex = 0,
            [Description("Length of text to select")] int length = 1,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.SelectTextAsync(
                automationId: automationId,
                name: name,
                startIndex: startIndex,
                length: length,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Find text in an element using TextPattern")]
        public async Task<object> FindText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Text to search for")] string searchText = "",
            [Description("Search backward (default: false)")] bool backward = false,
            [Description("Ignore case (default: true)")] bool ignoreCase = true,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
        {
            var response = await _textService.FindTextAsync(automationId, name, searchText, backward, ignoreCase, controlType, windowHandle, timeoutSeconds);
            return JsonSerializationHelper.Serialize(response);
        }

        [McpServerTool, Description("Get text formatting attributes (font, color, size, style) from an element using TextPattern")]
        public async Task<object> GetTextAttributes(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Start index for attribute range (default: 0)")] int startIndex = 0,
            [Description("Length of attribute range (default: entire text)")] int length = -1,
            [Description("Specific attribute to get (FontName, FontSize, ForegroundColor, etc.)")] string? attributeName = null,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
        {
            var response = await _textService.GetTextAttributesAsync(automationId, name, startIndex, length, attributeName, controlType, windowHandle, timeoutSeconds);
            return JsonSerializationHelper.Serialize(response);
        }


        [McpServerTool, Description("Set text content in an element using ValuePattern")]
        public async Task<object> SetText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Text to set")] string text = "",
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.SetTextAsync(
                automationId: automationId,
                name: name,
                text: text,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Traverse text using TextPattern with various navigation units")]
        public async Task<object> TraverseText(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Direction to traverse: character, word, line, paragraph, page, document (add '-back' suffix for backward movement)")] string direction = "character",
            [Description("Number of units to move (default: 1)")] int count = 1,
            [Description("ControlType to filter by (Edit, Document, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _textService.SelectTextAsync(
                automationId: automationId,
                name: name,
                startIndex: 0,
                length: count,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));


        // Grid Pattern Operations
        [McpServerTool, Description("Get a specific grid item at row and column coordinates")]
        public async Task<object> GetGridItem(
            [Description("Row index (0-based)")] int row,
            [Description("Column index (0-based)")] int column,
            [Description("AutomationId of the grid element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the grid element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (DataGrid, List, Table, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? gridElementId = null)
            => JsonSerializationHelper.Serialize(await _gridService.GetGridItemAsync(
                automationId: automationId,
                name: name,
                row: row,
                column: column,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Get the row header for a specific row in a grid")]
        public async Task<object> GetRowHeader(
            [Description("Row index (0-based)")] int row,
            [Description("AutomationId of the grid element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the grid element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (DataGrid, List, Table, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? gridElementId = null)
            => JsonSerializationHelper.Serialize(await _gridService.GetRowHeaderAsync(
                automationId: automationId,
                name: name,
                row: row,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Get the column header for a specific column in a grid")]
        public async Task<object> GetColumnHeader(
            [Description("Column index (0-based)")] int column,
            [Description("AutomationId of the grid element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the grid element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (DataGrid, List, Table, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? gridElementId = null)
            => JsonSerializationHelper.Serialize(await _gridService.GetColumnHeaderAsync(
                automationId: automationId,
                name: name,
                column: column,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // Table Pattern Operations




        // MultipleView Pattern Operations

        [McpServerTool, Description("Set the current view of an element")]
        public async Task<object> SetView(
            [Description("View ID to set")] int viewId,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Pane, Custom, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _multipleViewService.SetViewAsync(
                automationId: automationId,
                name: name,
                viewId: viewId,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));


        // Custom Properties and Events

        // VirtualizedItem Pattern
        [McpServerTool, Description("Realize a virtualized item to make it fully available in the UI Automation tree")]
        public async Task<object> RealizeVirtualizedItem(
            [Description("AutomationId of the virtualized element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the virtualized element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, DataItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _virtualizedItemService.RealizeItemAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));


        // ItemContainer Pattern
        [McpServerTool, Description("Find an item in a container by property value (useful for searching in lists, trees, and grids)")]
        public async Task<object> FindItemByProperty(
            [Description("Property name to search by (e.g., 'Name', 'AutomationId', 'ControlType'). Leave empty to find any item.")] string? propertyName = null,
            [Description("Property value to match. Leave empty to find any item.")] string? value = null,
            [Description("Start search after this element ID (for continued searches)")] string? startAfterId = null,
            [Description("AutomationId of the container element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the container element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (List, Tree, DataGrid, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? containerId = null)
            => JsonSerializationHelper.Serialize(await _itemContainerService.FindItemByPropertyAsync(
                automationId: automationId,
                name: name,
                propertyName: propertyName,
                value: value,
                startAfterId: startAfterId,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // SynchronizedInput Pattern
        [McpServerTool, Description("Start listening for synchronized input on an element (mouse or keyboard events)")]
        public async Task<object> StartSynchronizedInput(
            [Description("Input type to synchronize: 'KeyUp', 'KeyDown', 'LeftMouseUp', 'LeftMouseDown', 'RightMouseUp', 'RightMouseDown'")] string inputType,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Any, Button, Edit, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _synchronizedInputService.StartListeningAsync(
                automationId: automationId,
                name: name,
                inputType: inputType,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Cancel synchronized input listening on an element")]
        public async Task<object> CancelSynchronizedInput(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Any, Button, Edit, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _synchronizedInputService.CancelAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // Event Monitoring Operations
        [McpServerTool, Description("Start continuous event monitoring")]
        public async Task<object> StartEventMonitoring(
            [Description("Type of event to monitor (e.g., 'Focus', 'Invoke', 'Selection', 'Text')")] string eventType,
            [Description("AutomationId of the element to monitor (optional, preferred identifier)")] string? automationId = null,
            [Description("Name of the element to monitor (optional, fallback identifier)")] string? name = null,
            [Description("ControlType to filter by (optional, Any, Button, Edit, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds")] int timeoutSeconds = 60,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.StartEventMonitoringAsync(
                eventType, automationId: elementId ?? automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Stop continuous event monitoring")]
        public async Task<object> StopEventMonitoring(
            [Description("Session ID returned from StartEventMonitoring")] string? sessionId = null,
            [Description("Timeout in seconds")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.StopEventMonitoringAsync(sessionId, timeoutSeconds));

        [McpServerTool, Description("Get the current event log")]
        public async Task<object> GetEventLog(
            [Description("Session ID returned from StartEventMonitoring")] string? sessionId = null,
            [Description("Maximum number of events to retrieve")] int maxCount = 100,
            [Description("Timeout in seconds")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.GetEventLogAsync(sessionId, maxCount, timeoutSeconds));

        // Window Capabilities and State


        // Grid Information

        // Transform Capabilities

        // Selection Operations


        // Scroll Information

        // Accessibility Information

        // Range Value Operations

        // Text Operations

        // Table Operations

        // Selection Operations

        // Transform Operations

    }
}
