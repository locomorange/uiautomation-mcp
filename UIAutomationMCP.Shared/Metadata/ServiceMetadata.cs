using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Metadata
{
    /// <summary>
    /// Base class for all service metadata providing common execution information
    /// </summary>
    public abstract class ServiceMetadata
    {
        /// <summary>
        /// The type of operation performed (e.g., "toggle", "text", "window")
        /// </summary>
        [JsonPropertyName("operationType")]
        public string OperationType { get; set; } = "";

        /// <summary>
        /// Whether the operation completed successfully
        /// </summary>
        [JsonPropertyName("operationCompleted")]
        public bool OperationCompleted { get; set; } = true;

        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        [JsonPropertyName("executionTimeMs")]
        public double ExecutionTimeMs { get; set; }

        /// <summary>
        /// The method name that was called
        /// </summary>
        [JsonPropertyName("methodName")]
        public string MethodName { get; set; } = "";
    }

    /// <summary>
    /// Metadata for toggle-related operations
    /// </summary>
    public class ToggleServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "elementToggled", "stateSet")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Previous toggle state before the operation
        /// </summary>
        [JsonPropertyName("previousState")]
        public string? PreviousState { get; set; }

        /// <summary>
        /// Current toggle state after the operation
        /// </summary>
        [JsonPropertyName("currentState")]
        public string? CurrentState { get; set; }
    }

    /// <summary>
    /// Metadata for text-related operations
    /// </summary>
    public class TextServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "setText", "appendText", "textFound")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Length of text that was processed
        /// </summary>
        [JsonPropertyName("textLength")]
        public int? TextLength { get; set; }

        /// <summary>
        /// Whether text was found (for search operations)
        /// </summary>
        [JsonPropertyName("textFound")]
        public bool? TextFound { get; set; }

        /// <summary>
        /// Starting index of found text
        /// </summary>
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// Whether the element has attributes (for attribute operations)
        /// </summary>
        [JsonPropertyName("hasAttributes")]
        public bool? HasAttributes { get; set; }
    }

    /// <summary>
    /// Metadata for window-related operations
    /// </summary>
    public class WindowServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "windowMoved", "windowResized", "waitForInputIdle")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether input idle was achieved (for WaitForInputIdle operations)
        /// </summary>
        [JsonPropertyName("inputIdleAchieved")]
        public bool? InputIdleAchieved { get; set; }

        /// <summary>
        /// Window width (for resize operations)
        /// </summary>
        [JsonPropertyName("windowWidth")]
        public int? WindowWidth { get; set; }

        /// <summary>
        /// Window height (for resize operations)
        /// </summary>
        [JsonPropertyName("windowHeight")]
        public int? WindowHeight { get; set; }

        /// <summary>
        /// Window X position (for move operations)
        /// </summary>
        [JsonPropertyName("positionX")]
        public int? PositionX { get; set; }

        /// <summary>
        /// Window Y position (for move operations)
        /// </summary>
        [JsonPropertyName("positionY")]
        public int? PositionY { get; set; }
    }

    /// <summary>
    /// Metadata for value-related operations
    /// </summary>
    public class ValueServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "valueSet", "valueRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Length of the value that was processed
        /// </summary>
        [JsonPropertyName("valueLength")]
        public int? ValueLength { get; set; }

        /// <summary>
        /// Whether the element has a value
        /// </summary>
        [JsonPropertyName("hasValue")]
        public bool? HasValue { get; set; }
    }

    /// <summary>
    /// Metadata for search-related operations
    /// </summary>
    public class SearchServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "elementsSearched")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Number of elements found
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int ElementsFound { get; set; }

        /// <summary>
        /// Total number of results available
        /// </summary>
        [JsonPropertyName("totalResults")]
        public int? TotalResults { get; set; }
    }

    /// <summary>
    /// Metadata for invoke operations
    /// </summary>
    public class InvokeServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (always "elementInvoked" for invoke operations)
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "elementInvoked";
    }

    /// <summary>
    /// Metadata for tree navigation operations
    /// </summary>
    public class TreeNavigationServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "childrenRetrieved", "parentRetrieved", "siblingsRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Number of children/siblings found
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int? ElementsFound { get; set; }

        /// <summary>
        /// Element ID that was navigated from
        /// </summary>
        [JsonPropertyName("sourceElementId")]
        public string? SourceElementId { get; set; }

        /// <summary>
        /// Whether navigation was successful
        /// </summary>
        [JsonPropertyName("navigationSuccessful")]
        public bool NavigationSuccessful { get; set; } = true;
    }

    /// <summary>
    /// Metadata for accessibility verification operations
    /// </summary>
    public class AccessibilityServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "accessibilityVerified", "accessibilityPropertiesRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Number of accessibility elements found
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int? ElementsFound { get; set; }

        /// <summary>
        /// Whether accessibility verification was successful
        /// </summary>
        [JsonPropertyName("verificationSuccessful")]
        public bool VerificationSuccessful { get; set; } = true;

        /// <summary>
        /// Accessibility properties count (for property operations)
        /// </summary>
        [JsonPropertyName("propertiesCount")]
        public int? PropertiesCount { get; set; }
    }

    /// <summary>
    /// Metadata for application launcher operations
    /// </summary>
    public class ApplicationLauncherMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "applicationLaunched", "uwpApplicationLaunched", "applicationLaunchedByName")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Application executable path or name
        /// </summary>
        [JsonPropertyName("applicationPath")]
        public string? ApplicationPath { get; set; }

        /// <summary>
        /// Process ID of the launched application
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Process name of the launched application
        /// </summary>
        [JsonPropertyName("processName")]
        public string? ProcessName { get; set; }

        /// <summary>
        /// Whether the process has exited
        /// </summary>
        [JsonPropertyName("hasExited")]
        public bool? HasExited { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Command line arguments used (for launch operations)
        /// </summary>
        [JsonPropertyName("commandLineArguments")]
        public string? CommandLineArguments { get; set; }

        /// <summary>
        /// Working directory used (for Win32 launch operations)
        /// </summary>
        [JsonPropertyName("workingDirectory")]
        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// Window title of the launched application (for name-based launch)
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// Application ID for UWP applications
        /// </summary>
        [JsonPropertyName("appId")]
        public string? AppId { get; set; }
    }

    /// <summary>
    /// Metadata for control type validation operations
    /// </summary>
    public class ControlTypeServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "controlTypePatternsValidated")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "controlTypePatternsValidated";

        /// <summary>
        /// Number of elements found during validation
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int? ElementsFound { get; set; }

        /// <summary>
        /// The control type that was validated
        /// </summary>
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }

        /// <summary>
        /// Whether validation was successful
        /// </summary>
        [JsonPropertyName("validationSuccessful")]
        public bool ValidationSuccessful { get; set; } = true;

        /// <summary>
        /// Number of supported patterns found
        /// </summary>
        [JsonPropertyName("supportedPatternsCount")]
        public int? SupportedPatternsCount { get; set; }
    }

    /// <summary>
    /// Metadata for focus management operations
    /// </summary>
    public class FocusServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (always "focusSet" for focus operations)
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "focusSet";

        /// <summary>
        /// The automation ID of the element that received focus
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// The name of the element that received focus
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// The control type of the focused element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// The required pattern that was validated
        /// </summary>
        [JsonPropertyName("requiredPattern")]
        public string? RequiredPattern { get; set; }

        /// <summary>
        /// Whether focus was successfully set
        /// </summary>
        [JsonPropertyName("focusSuccessful")]
        public bool FocusSuccessful { get; set; } = true;
    }

    /// <summary>
    /// Metadata for custom property operations
    /// </summary>
    public class CustomPropertyServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "customPropertiesRetrieved", "customPropertySet")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Number of elements found
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int? ElementsFound { get; set; }

        /// <summary>
        /// Number of custom properties retrieved or set
        /// </summary>
        [JsonPropertyName("propertiesCount")]
        public int? PropertiesCount { get; set; }

        /// <summary>
        /// The property ID that was accessed (for set operations)
        /// </summary>
        [JsonPropertyName("propertyId")]
        public string? PropertyId { get; set; }

        /// <summary>
        /// The value that was set (for set operations)
        /// </summary>
        [JsonPropertyName("propertyValue")]
        public string? PropertyValue { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;
    }

    /// <summary>
    /// Metadata for range value operations
    /// </summary>
    public class RangeServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "rangeValueSet", "rangeValueRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// The value that was set or retrieved
        /// </summary>
        [JsonPropertyName("rangeValue")]
        public double? RangeValue { get; set; }

        /// <summary>
        /// The minimum value of the range
        /// </summary>
        [JsonPropertyName("minimumValue")]
        public double? MinimumValue { get; set; }

        /// <summary>
        /// The maximum value of the range
        /// </summary>
        [JsonPropertyName("maximumValue")]
        public double? MaximumValue { get; set; }

        /// <summary>
        /// The small change value for the range
        /// </summary>
        [JsonPropertyName("smallChange")]
        public double? SmallChange { get; set; }

        /// <summary>
        /// The large change value for the range
        /// </summary>
        [JsonPropertyName("largeChange")]
        public double? LargeChange { get; set; }

        /// <summary>
        /// Whether the range is read-only
        /// </summary>
        [JsonPropertyName("isReadOnly")]
        public bool? IsReadOnly { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;
    }

    /// <summary>
    /// Metadata for grid operations
    /// </summary>
    public class GridServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "gridItemRetrieved", "rowHeaderRetrieved", "columnHeaderRetrieved", "gridInfoRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Number of elements found
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int? ElementsFound { get; set; }

        /// <summary>
        /// Row index for grid item or row header operations
        /// </summary>
        [JsonPropertyName("rowIndex")]
        public int? RowIndex { get; set; }

        /// <summary>
        /// Column index for grid item or column header operations
        /// </summary>
        [JsonPropertyName("columnIndex")]
        public int? ColumnIndex { get; set; }

        /// <summary>
        /// Total row count (for grid info operations)
        /// </summary>
        [JsonPropertyName("totalRows")]
        public int? TotalRows { get; set; }

        /// <summary>
        /// Total column count (for grid info operations)
        /// </summary>
        [JsonPropertyName("totalColumns")]
        public int? TotalColumns { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Whether the grid supports row headers
        /// </summary>
        [JsonPropertyName("supportsRowHeaders")]
        public bool? SupportsRowHeaders { get; set; }

        /// <summary>
        /// Whether the grid supports column headers
        /// </summary>
        [JsonPropertyName("supportsColumnHeaders")]
        public bool? SupportsColumnHeaders { get; set; }
    }

    /// <summary>
    /// Metadata for event monitoring operations
    /// </summary>
    public class EventMonitorServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "eventsMonitored", "monitoringStarted", "monitoringStopped", "eventLogRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Event type being monitored (e.g., "Focus", "Selection", "Click")
        /// </summary>
        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }

        /// <summary>
        /// Session ID for the monitoring session
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Duration of monitoring in seconds (for MonitorEvents)
        /// </summary>
        [JsonPropertyName("monitoringDuration")]
        public int? MonitoringDuration { get; set; }

        /// <summary>
        /// Number of events captured
        /// </summary>
        [JsonPropertyName("eventsCount")]
        public int? EventsCount { get; set; }

        /// <summary>
        /// Maximum number of events requested (for GetEventLog)
        /// </summary>
        [JsonPropertyName("maxEventCount")]
        public int? MaxEventCount { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Whether monitoring is currently active
        /// </summary>
        [JsonPropertyName("monitoringActive")]
        public bool? MonitoringActive { get; set; }

        /// <summary>
        /// Element that was targeted for monitoring (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was targeted for monitoring (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }
    }

    /// <summary>
    /// Metadata for selection operations
    /// </summary>
    public class SelectionServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "itemSelected", "itemAddedToSelection", "itemRemovedFromSelection", "selectionCleared", "selectionContainerRetrieved", "selectionRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was targeted for selection (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was targeted for selection (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the target element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// Number of selected items (for GetSelection operations)
        /// </summary>
        [JsonPropertyName("selectedItemsCount")]
        public int? SelectedItemsCount { get; set; }

        /// <summary>
        /// Whether multiple selection is supported
        /// </summary>
        [JsonPropertyName("supportsMultipleSelection")]
        public bool? SupportsMultipleSelection { get; set; }

        /// <summary>
        /// Selection container automation ID (for GetSelectionContainer operations)
        /// </summary>
        [JsonPropertyName("selectionContainerAutomationId")]
        public string? SelectionContainerAutomationId { get; set; }

        /// <summary>
        /// Whether the selection is required (for GetSelection operations)
        /// </summary>
        [JsonPropertyName("isSelectionRequired")]
        public bool? IsSelectionRequired { get; set; }
    }

    /// <summary>
    /// Metadata for screenshot operations
    /// </summary>
    public class ScreenshotServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (always "screenshotTaken" for screenshot operations)
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "screenshotTaken";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Output path where the screenshot was saved
        /// </summary>
        [JsonPropertyName("outputPath")]
        public string? OutputPath { get; set; }

        /// <summary>
        /// Width of the captured screenshot
        /// </summary>
        [JsonPropertyName("screenshotWidth")]
        public int? ScreenshotWidth { get; set; }

        /// <summary>
        /// Height of the captured screenshot
        /// </summary>
        [JsonPropertyName("screenshotHeight")]
        public int? ScreenshotHeight { get; set; }

        /// <summary>
        /// File size of the screenshot in bytes
        /// </summary>
        [JsonPropertyName("fileSize")]
        public long? FileSize { get; set; }

        /// <summary>
        /// Target window title (if capturing a specific window)
        /// </summary>
        [JsonPropertyName("targetWindowTitle")]
        public string? TargetWindowTitle { get; set; }

        /// <summary>
        /// Target process ID (if capturing a specific window)
        /// </summary>
        [JsonPropertyName("targetProcessId")]
        public int? TargetProcessId { get; set; }

        /// <summary>
        /// Whether base64 image data was generated
        /// </summary>
        [JsonPropertyName("hasBase64Data")]
        public bool? HasBase64Data { get; set; }

        /// <summary>
        /// Maximum tokens requested for optimization
        /// </summary>
        [JsonPropertyName("maxTokensRequested")]
        public int? MaxTokensRequested { get; set; }

        /// <summary>
        /// Timestamp when the screenshot was taken
        /// </summary>
        [JsonPropertyName("screenshotTimestamp")]
        public string? ScreenshotTimestamp { get; set; }
    }

    /// <summary>
    /// Metadata for multiple view operations
    /// </summary>
    public class MultipleViewServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "availableViewsRetrieved", "viewSet", "currentViewRetrieved", "viewNameRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was targeted for view operations (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was targeted for view operations (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the target element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// View ID that was set or retrieved (for SetView and GetViewName operations)
        /// </summary>
        [JsonPropertyName("viewId")]
        public int? ViewId { get; set; }

        /// <summary>
        /// Number of available views found (for GetAvailableViews operations)
        /// </summary>
        [JsonPropertyName("viewsCount")]
        public int? ViewsCount { get; set; }

        /// <summary>
        /// Current view ID (for GetCurrentView operations)
        /// </summary>
        [JsonPropertyName("currentViewId")]
        public int? CurrentViewId { get; set; }

        /// <summary>
        /// View name retrieved (for GetViewName operations)
        /// </summary>
        [JsonPropertyName("viewName")]
        public string? ViewName { get; set; }
    }

    /// <summary>
    /// Metadata for item container operations
    /// </summary>
    public class ItemContainerServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (always "itemSearchedByProperty" for ItemContainer operations)
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "itemSearchedByProperty";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Container element that was searched (automation ID)
        /// </summary>
        [JsonPropertyName("containerAutomationId")]
        public string? ContainerAutomationId { get; set; }

        /// <summary>
        /// Container element that was searched (name)
        /// </summary>
        [JsonPropertyName("containerElementName")]
        public string? ContainerElementName { get; set; }

        /// <summary>
        /// Control type of the container element
        /// </summary>
        [JsonPropertyName("containerControlType")]
        public string? ContainerControlType { get; set; }

        /// <summary>
        /// Property name that was searched for
        /// </summary>
        [JsonPropertyName("searchPropertyName")]
        public string? SearchPropertyName { get; set; }

        /// <summary>
        /// Property value that was searched for
        /// </summary>
        [JsonPropertyName("searchPropertyValue")]
        public string? SearchPropertyValue { get; set; }

        /// <summary>
        /// Starting element ID for search (if specified)
        /// </summary>
        [JsonPropertyName("startAfterId")]
        public string? StartAfterId { get; set; }

        /// <summary>
        /// Number of items found
        /// </summary>
        [JsonPropertyName("itemsFound")]
        public int? ItemsFound { get; set; }
    }

    /// <summary>
    /// Metadata for virtualized item operations
    /// </summary>
    public class VirtualizedItemServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (always "itemRealized" for VirtualizedItem operations)
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "itemRealized";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was realized (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was realized (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the realized element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// Whether the item was successfully realized
        /// </summary>
        [JsonPropertyName("itemRealized")]
        public bool ItemRealized { get; set; } = true;
    }

    /// <summary>
    /// Metadata for layout operations
    /// </summary>
    public class LayoutServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "layoutInvalidated", "layoutUpdated")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was targeted for layout operations (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was targeted for layout operations (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the target element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }
    }

    /// <summary>
    /// Metadata for synchronized input operations
    /// </summary>
    public class SynchronizedInputServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "inputSynchronized", "inputCanceled")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was targeted for input operations (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was targeted for input operations (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the target element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// Type of input that was synchronized
        /// </summary>
        [JsonPropertyName("inputType")]
        public string? InputType { get; set; }
    }

    /// <summary>
    /// Metadata for transform operations
    /// </summary>
    public class TransformServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "elementMoved", "elementResized", "elementRotated")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was transformed (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was transformed (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the target element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// X position (for move operations)
        /// </summary>
        [JsonPropertyName("positionX")]
        public double? PositionX { get; set; }

        /// <summary>
        /// Y position (for move operations)
        /// </summary>
        [JsonPropertyName("positionY")]
        public double? PositionY { get; set; }

        /// <summary>
        /// Width (for resize operations)
        /// </summary>
        [JsonPropertyName("width")]
        public double? Width { get; set; }

        /// <summary>
        /// Height (for resize operations)
        /// </summary>
        [JsonPropertyName("height")]
        public double? Height { get; set; }

        /// <summary>
        /// Rotation degrees (for rotate operations)
        /// </summary>
        [JsonPropertyName("rotationDegrees")]
        public double? RotationDegrees { get; set; }
    }

    /// <summary>
    /// Metadata for table operations
    /// </summary>
    public class TableServiceMetadata : ServiceMetadata
    {
        /// <summary>
        /// The specific action performed (e.g., "tableItemRetrieved", "rowHeaderRetrieved", "columnHeaderRetrieved")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        [JsonPropertyName("operationSuccessful")]
        public bool OperationSuccessful { get; set; } = true;

        /// <summary>
        /// Element that was targeted for table operations (automation ID)
        /// </summary>
        [JsonPropertyName("targetAutomationId")]
        public string? TargetAutomationId { get; set; }

        /// <summary>
        /// Element that was targeted for table operations (name)
        /// </summary>
        [JsonPropertyName("targetElementName")]
        public string? TargetElementName { get; set; }

        /// <summary>
        /// Control type of the target element
        /// </summary>
        [JsonPropertyName("targetControlType")]
        public string? TargetControlType { get; set; }

        /// <summary>
        /// Row index for table operations
        /// </summary>
        [JsonPropertyName("rowIndex")]
        public int? RowIndex { get; set; }

        /// <summary>
        /// Column index for table operations
        /// </summary>
        [JsonPropertyName("columnIndex")]
        public int? ColumnIndex { get; set; }

        /// <summary>
        /// Number of elements found
        /// </summary>
        [JsonPropertyName("elementsFound")]
        public int? ElementsFound { get; set; }
    }
}