using ModelContextProtocol.Server;
using ModelContextProtocol;
using Microsoft.Extensions.Logging;
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
        private readonly IInteractionService _interactionService;
        private readonly ISelectionService _selectionService;
        private readonly ITextService _textService;
        private readonly ILayoutService _layoutService;
        private readonly IGridTableService _gridTableService;
        private readonly IAdvancedPatternService _advancedPatternService;
        private readonly IWindowService _windowService;
        private readonly ITransformService _transformService;
        private readonly IEventMonitorService _eventMonitorService;
        private readonly IFocusService _focusService;
        private readonly IItemContainerService _itemContainerService;
        private readonly IMcpLogService _mcpLogService;

        public UIAutomationTools(
            IApplicationLauncher applicationLauncher,
            IScreenshotService screenshotService,
            IElementSearchService elementSearchService,
            ITreeNavigationService treeNavigationService,
            IInteractionService interactionService,
            ISelectionService selectionService,
            ITextService textService,
            ILayoutService layoutService,
            IGridTableService gridTableService,
            IAdvancedPatternService advancedPatternService,
            IWindowService windowService,
            ITransformService transformService,
            IEventMonitorService eventMonitorService,
            IFocusService focusService,
            IItemContainerService itemContainerService,
            IMcpLogService mcpLogService)
        {
            _applicationLauncher = applicationLauncher;
            _screenshotService = screenshotService;
            _elementSearchService = elementSearchService;
            _treeNavigationService = treeNavigationService;
            _interactionService = interactionService;
            _selectionService = selectionService;
            _textService = textService;
            _layoutService = layoutService;
            _gridTableService = gridTableService;
            _advancedPatternService = advancedPatternService;
            _windowService = windowService;
            _transformService = transformService;
            _eventMonitorService = eventMonitorService;
            _focusService = focusService;
            _itemContainerService = itemContainerService;
            _mcpLogService = mcpLogService;
        }

        // Window and Element Discovery

        [McpServerTool, Description("Search for UI elements with flexible filtering options. Returns basic element properties by default. When includeDetails=true, returns comprehensive data including: • All supported UI patterns (Toggle state, Range values, Window state, Selection info, Grid/Table structure, Scroll position, Text content, Transform capabilities, etc.) • Accessibility information (labeledBy, helpText, accessKey, acceleratorKey) • Advanced properties (frameworkId, runtimeId, isPassword) • Element hierarchy (parent and children relationships). For window detection, use scope='children' with requiredPattern='Window' (finds all elements with WindowPattern including Panes). Avoid controlType='Window' as it excludes WindowPattern-supporting Panes and other window-like elements.")]
        public async Task<object> SearchElements(
            [Description("Cross-property search text (searches Name, AutomationId, ClassName)")] string? searchText = null,
            [Description("Specific AutomationId to search for")] string? automationId = null,
            [Description("Specific Name (display name) to search for")] string? name = null,
            [Description("Control type filter (Button, Slider, TextBox, etc.). For windows, use requiredPattern='Window' instead")] string? controlType = null,
            [Description("Class name filter")] string? className = null,
            [Description("Window title filter")] string? windowTitle = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Search scope: children, descendants, subtree (default: descendants)")] string scope = "descendants",
            [Description("Required UI Automation pattern. Use 'Window' to find all window-like elements including Panes")] string? requiredPattern = null,
            [Description("Only return visible elements (default: true)")] bool visibleOnly = true,
            [Description("Enable fuzzy matching for text searches (default: false)")] bool fuzzyMatch = false,
            [Description("Only return enabled elements (default: false)")] bool enabledOnly = false,
            [Description("Maximum number of results to return (default: 50)")] int maxResults = 50,
            [Description("Sort results by: Name, ControlType, Position (optional)")] string? sortBy = null,
            [Description("Include comprehensive details: all UI patterns (Toggle, Range, Window, Selection, Grid, Scroll, Text, Transform, Value, ExpandCollapse, Dock, MultipleView, Table, etc.), accessibility info (labels, help text, keyboard shortcuts), and element hierarchy (default: false)")] bool includeDetails = false,
            [Description("Use WindowHandle as filter instead of search root (default: false). true=window-level search, false=search within window")] bool useWindowHandleAsFilter = false,
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
                    IncludeDetails = includeDetails,
                    UseWindowHandleAsFilter = useWindowHandleAsFilter
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
        {
            // Log using the existing _mcpLogService
            await _mcpLogService.LogInformationAsync("TakeScreenshot",
                $"TakeScreenshot called: windowTitle={windowTitle}, maxTokens={maxTokens}, timeoutSeconds={timeoutSeconds}",
                "tool");

            // MCP notification would need to be sent here during tool execution
            // Currently not possible without access to IMcpServer in tool methods

            var result = await _screenshotService.TakeScreenshotAsync(windowTitle, outputPath, maxTokens, windowHandle, timeoutSeconds);

            await _mcpLogService.LogInformationAsync("TakeScreenshot",
                "Screenshot captured successfully",
                "tool");

            return JsonSerializationHelper.Serialize(result);
        }



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
            => JsonSerializationHelper.Serialize(await _interactionService.InvokeElementAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Set the value of an element (text input, etc.) using ValuePattern")]
        public async Task<object> SetElementValue(
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("Value to set")] string value = "",
            [Description("ControlType to filter by (TextBox, Edit, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30,
            [Description("DEPRECATED: Use automationId or name instead")] string? elementId = null)
            => JsonSerializationHelper.Serialize(await _interactionService.SetValueAsync(
                automationId: automationId,
                name: name,
                value: value,
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
            => JsonSerializationHelper.Serialize(await _interactionService.ToggleElementAsync(
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

        [McpServerTool, Description("Perform selection actions on an element (select, add, remove, clear) using SelectionItemPattern or SelectionPattern")]
        public async Task<object> SelectionAction(
            [Description("Action to perform: select, add, remove, clear")] string action,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TabItem, TreeItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
        {
            return action.ToLower() switch
            {
                "select" => JsonSerializationHelper.Serialize(await _selectionService.SelectItemAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "add" => JsonSerializationHelper.Serialize(await _selectionService.AddToSelectionAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "remove" => JsonSerializationHelper.Serialize(await _selectionService.RemoveFromSelectionAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "clear" => JsonSerializationHelper.Serialize(await _selectionService.ClearSelectionAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                _ => throw new ArgumentException($"Invalid action: {action}")
            };
        }


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
            => JsonSerializationHelper.Serialize(await _interactionService.SetRangeValueAsync(
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
            => JsonSerializationHelper.Serialize(await _windowService.WindowOperationAsync(operation: action, windowTitle: windowTitle, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds));

        // GetWindowInteractionState and GetWindowCapabilities merged into FindElements Properties field

        [McpServerTool, Description("Wait for window to become idle using WindowPattern")]
        public async Task<object> WaitForWindowInputIdle(
            [Description("Maximum time to wait in milliseconds (default: 10000)")] int timeoutMilliseconds = 10000,
            [Description("Title of the window (optional)")] string? windowTitle = null,
            [Description("Native window handle (HWND) for direct window targeting")] long? windowHandle = null,
            [Description("Timeout in seconds for operation (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _windowService.WaitForInputIdleAsync(timeoutMilliseconds: timeoutMilliseconds, windowTitle: windowTitle, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds));


        [McpServerTool, Description("Transform an element (move, resize, rotate) using TransformPattern")]
        public async Task<object> TransformElement(
            [Description("Action to perform: move, resize, rotate")] string action,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("X coordinate or Width or Degrees depending on action")] double value1 = 0,
            [Description("Y coordinate or Height depending on action")] double value2 = 0,
            [Description("ControlType to filter by")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
        {
            return action.ToLower() switch
            {
                "move" => JsonSerializationHelper.Serialize(await _transformService.MoveElementAsync(automationId: automationId, name: name, x: value1, y: value2, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "resize" => JsonSerializationHelper.Serialize(await _transformService.ResizeElementAsync(automationId: automationId, name: name, width: value1, height: value2, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "rotate" => JsonSerializationHelper.Serialize(await _transformService.RotateElementAsync(automationId: automationId, name: name, degrees: value1, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                _ => throw new ArgumentException($"Invalid action: {action}")
            };
        }

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
            var response = await _textService.FindTextAsync(
                automationId: automationId,
                name: name,
                searchText: searchText,
                backward: backward,
                ignoreCase: ignoreCase,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds);
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
            var response = await _textService.GetTextAttributesAsync(
                automationId: automationId,
                name: name,
                startIndex: startIndex,
                length: length,
                attributeName: attributeName,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds);
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

        // Grid Pattern Operations
        [McpServerTool, Description("Get grid-related information (item at coordinates, row header, column header) using GridPattern or TablePattern")]
        public async Task<object> GetGridInfo(
            [Description("Action to perform: getItem, getRowHeader, getColumnHeader")] string action,
            [Description("Row index (0-based)")] int row = 0,
            [Description("Column index (0-based)")] int column = 0,
            [Description("AutomationId of the grid element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the grid element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (DataGrid, List, Table, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
        {
            // In unit tests, many grid/multipleview mocks expect automationId and name to be passed exactly.
            // Some tests pass automationId, others pass name. We must ensure we don't pass one for the other if not intended.
            
            return action.ToLower() switch
            {
                "get-item" => JsonSerializationHelper.Serialize(await _gridTableService.GetGridItemAsync(automationId: automationId, name: name, row: row, column: column, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "get-row-header" => JsonSerializationHelper.Serialize(await _gridTableService.GetRowHeaderAsync(automationId: automationId, name: name, row: row, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "get-column-header" => JsonSerializationHelper.Serialize(await _gridTableService.GetColumnHeaderAsync(automationId: automationId, name: name, column: column, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "get-table-info" => JsonSerializationHelper.Serialize(await _gridTableService.GetTableInfoAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "getitem" => JsonSerializationHelper.Serialize(await _gridTableService.GetGridItemAsync(automationId: automationId, name: name, row: row, column: column, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "getrowheader" => JsonSerializationHelper.Serialize(await _gridTableService.GetRowHeaderAsync(automationId: automationId, name: name, row: row, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "getcolumnheader" => JsonSerializationHelper.Serialize(await _gridTableService.GetColumnHeaderAsync(automationId: automationId, name: name, column: column, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "gettableinfo" => JsonSerializationHelper.Serialize(await _gridTableService.GetTableInfoAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                _ => throw new ArgumentException($"Invalid action: {action}")
            };
        }





        [McpServerTool, Description("Set the current view of an element")]
        public async Task<object> SetView(
            [Description("View ID to set")] int viewId,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (Pane, Custom, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
        {
            return JsonSerializationHelper.Serialize(await _advancedPatternService.SetViewAsync(
                automationId: automationId,
                name: name,
                viewId: viewId,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));
        }




        // Custom Properties and Events

        // VirtualizedItem Pattern
        [McpServerTool, Description("Realize a virtualized item to make it fully available in the UI Automation tree")]
        public async Task<object> RealizeVirtualizedItem(
            [Description("AutomationId of the virtualized element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the virtualized element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by (ListItem, TreeItem, DataItem, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _advancedPatternService.RealizeItemAsync(
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        // ItemContainer Pattern
        [McpServerTool, Description("Find an item within a virtualized container (DataGrid, ListView, etc.) using ItemContainerPattern.FindItemByProperty. Efficiently searches without loading all items. Auto-realizes the found item if virtualized.")]
        public async Task<object> FindItemByProperty(
            [Description("AutomationId of the container element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the container element (fallback, display name)")] string? name = null,
            [Description("Property to search by: Name, AutomationId, ClassName, ControlType, etc. (empty = search all properties)")] string? propertyName = null,
            [Description("Value to search for")] string? value = null,
            [Description("AutomationId of the element to start searching after (for pagination)")] string? startAfterId = null,
            [Description("ControlType to filter by (DataGrid, List, Tree, etc.)")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
            => JsonSerializationHelper.Serialize(await _itemContainerService.FindItemByPropertyAsync(
                automationId: automationId,
                name: name,
                propertyName: propertyName,
                value: value,
                startAfterId: startAfterId,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));


        [McpServerTool, Description("Manage synchronized input (start, cancel) on an element using SynchronizedInputPattern")]
        public async Task<object> SynchronizedInput(
            [Description("Action to perform: start, cancel")] string action,
            [Description("Input type to synchronize (for 'start'): KeyUp, KeyDown, LeftMouseUp, LeftMouseDown, RightMouseUp, RightMouseDown")] string? inputType = null,
            [Description("AutomationId of the element (preferred, stable identifier)")] string? automationId = null,
            [Description("Name of the element (fallback, display name)")] string? name = null,
            [Description("ControlType to filter by")] string? controlType = null,
            [Description("Native window handle (HWND) for direct element targeting")] long? windowHandle = null,
            [Description("Timeout in seconds (default: 30)")] int timeoutSeconds = 30)
        {
            return action.ToLower() switch
            {
                "start" => JsonSerializationHelper.Serialize(await _advancedPatternService.StartListeningAsync(automationId: automationId, name: name, inputType: inputType ?? "", controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                "cancel" => JsonSerializationHelper.Serialize(await _advancedPatternService.CancelAsync(automationId: automationId, name: name, controlType: controlType, windowHandle: windowHandle, timeoutSeconds: timeoutSeconds)),
                _ => throw new ArgumentException($"Invalid action: {action}")
            };
        }

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
                eventType: eventType,
                automationId: automationId,
                name: name,
                controlType: controlType,
                windowHandle: windowHandle,
                timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Stop continuous event monitoring")]
        public async Task<object> StopEventMonitoring(
            [Description("Session ID returned from StartEventMonitoring")] string? sessionId = null,
            [Description("Timeout in seconds")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.StopEventMonitoringAsync(sessionId: sessionId, timeoutSeconds: timeoutSeconds));

        [McpServerTool, Description("Get the current event log")]
        public async Task<object> GetEventLog(
            [Description("Session ID returned from StartEventMonitoring")] string? sessionId = null,
            [Description("Maximum number of events to retrieve")] int maxCount = 100,
            [Description("Timeout in seconds")] int timeoutSeconds = 60)
            => JsonSerializationHelper.Serialize(await _eventMonitorService.GetEventLogAsync(sessionId: sessionId, maxCount: maxCount, timeoutSeconds: timeoutSeconds));
    }
}
