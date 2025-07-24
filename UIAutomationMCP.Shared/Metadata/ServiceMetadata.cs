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
        /// The specific action performed (e.g., "applicationLaunched", "applicationTerminated")
        /// </summary>
        [JsonPropertyName("actionPerformed")]
        public string ActionPerformed { get; set; } = "";

        /// <summary>
        /// Application executable path
        /// </summary>
        [JsonPropertyName("applicationPath")]
        public string? ApplicationPath { get; set; }

        /// <summary>
        /// Process ID of the launched/terminated application
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

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
}