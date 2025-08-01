using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Standard error response for UI Automation operations
    /// </summary>
    [MessagePackObject]
    public class ErrorResult
    {
        [Key(0)]
        [JsonPropertyName("success")]
        public bool Success { get; set; } = false;

        [Key(1)]
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [Key(2)]
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }

        [Key(3)]
        [JsonPropertyName("operation")]
        public string? Operation { get; set; }

        [Key(4)]
        [JsonPropertyName("timeoutSeconds")]
        public int? TimeoutSeconds { get; set; }

        [Key(5)]
        [JsonPropertyName("errorCategory")]
        public string? ErrorCategory { get; set; }

        [Key(6)]
        [JsonPropertyName("exceptionType")]
        public string? ExceptionType { get; set; }

        [Key(7)]
        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [Key(8)]
        [JsonPropertyName("suggestions")]
        public string[]? Suggestions { get; set; }

        /// <summary>
        /// Create a timeout error result
        /// </summary>
        public static ErrorResult CreateTimeoutError(string operation, string automationId, int timeoutSeconds, string? details = null)
        {
            return new ErrorResult
            {
                Error = $"{operation} could not complete within {timeoutSeconds} seconds for element '{automationId}'. Consider increasing the timeout duration to allow more time for the operation to complete.",
                AutomationId = automationId,
                Operation = operation,
                TimeoutSeconds = timeoutSeconds,
                ErrorCategory = "Timeout",
                Details = details,
                Suggestions = new[]
                {
                    "Increase the timeout value to allow more time for completion",
                    "Verify the element exists and is accessible",
                    "Check if the target application is responding",
                    "Ensure the element ID is correct"
                }
            };
        }

        /// <summary>
        /// Create an invalid operation error result
        /// </summary>
        public static ErrorResult CreateInvalidOperationError(string operation, string automationId, string? details = null)
        {
            return new ErrorResult
            {
                Error = $"Cannot perform {operation} on element '{automationId}'. The element may not support this operation or may not be in the correct state.",
                AutomationId = automationId,
                Operation = operation,
                ErrorCategory = "InvalidOperation",
                Details = details,
                Suggestions = new[]
                {
                    "Verify the element supports the requested operation",
                    "Check if the element is in the correct state",
                    "Ensure the element is enabled and visible",
                    "Try refreshing the element reference"
                }
            };
        }

        /// <summary>
        /// Create an argument error result
        /// </summary>
        public static ErrorResult CreateArgumentError(string operation, string automationId, string? details = null)
        {
            return new ErrorResult
            {
                Error = $"Invalid element identifier '{automationId}' for {operation}. Please check that the automation ID is correct and the target window is accessible.",
                AutomationId = automationId,
                Operation = operation,
                ErrorCategory = "InvalidArgument",
                Details = details,
                Suggestions = new[]
                {
                    "Verify the element ID is spelled correctly",
                    "Check if the target window is open and accessible",
                    "Ensure the application is in the foreground",
                    "Try using a different element locator strategy"
                }
            };
        }

        /// <summary>
        /// Create an unauthorized error result
        /// </summary>
        public static ErrorResult CreateUnauthorizedError(string operation, string automationId, string? details = null)
        {
            return new ErrorResult
            {
                Error = $"Access denied when attempting {operation} on element '{automationId}'. The application may require elevated privileges.",
                AutomationId = automationId,
                Operation = operation,
                ErrorCategory = "Unauthorized",
                Details = details,
                Suggestions = new[]
                {
                    "Run the application with administrator privileges",
                    "Check if the target application allows UI automation",
                    "Verify Windows UAC settings",
                    "Ensure the target process is accessible"
                }
            };
        }

        /// <summary>
        /// Create a generic error result
        /// </summary>
        public static ErrorResult CreateGenericError(string operation, string automationId, string exceptionType, string? details = null)
        {
            return new ErrorResult
            {
                Error = $"Failed to perform {operation} on element '{automationId}'. This could be due to the element not being found, the application not responding, or system issues.",
                AutomationId = automationId,
                Operation = operation,
                ErrorCategory = "Unexpected",
                ExceptionType = exceptionType,
                Details = details,
                Suggestions = new[]
                {
                    "Verify the element exists and is accessible",
                    "Check if the target application is running",
                    "Ensure system resources are available",
                    "Try the operation again after a brief delay"
                }
            };
        }

        /// <summary>
        /// Create a validation error result
        /// </summary>
        public static ErrorResult CreateValidationError(string operation, string issue)
        {
            return new ErrorResult
            {
                Error = $"Element ID is required for {operation} operation and cannot be empty",
                Operation = operation,
                ErrorCategory = "Validation",
                Details = issue,
                Suggestions = new[]
                {
                    "Provide a valid element ID",
                    "Check the UI element's AutomationId property",
                    "Use element inspection tools to find the correct ID"
                }
            };
        }
    }
}