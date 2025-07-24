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
}