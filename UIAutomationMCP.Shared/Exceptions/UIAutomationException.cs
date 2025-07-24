using System;

namespace UIAutomationMCP.Shared.Exceptions
{
    /// <summary>
    /// Base exception for all UI automation operations
    /// </summary>
    public abstract class UIAutomationException : Exception
    {
        /// <summary>
        /// Operation that failed
        /// </summary>
        public string Operation { get; }
        
        /// <summary>
        /// Element identifier involved in the operation
        /// </summary>
        public string? ElementId { get; }
        
        /// <summary>
        /// Error category for classification
        /// </summary>
        public abstract string ErrorCategory { get; }
        
        /// <summary>
        /// User-friendly suggestions for resolving the error
        /// </summary>
        public virtual string[] Suggestions { get; protected set; } = Array.Empty<string>();

        protected UIAutomationException(string operation, string? elementId, string message) 
            : base(message)
        {
            Operation = operation;
            ElementId = elementId;
        }

        protected UIAutomationException(string operation, string? elementId, string message, Exception innerException) 
            : base(message, innerException)
        {
            Operation = operation;
            ElementId = elementId;
        }
    }

    /// <summary>
    /// Exception thrown when an operation times out
    /// </summary>
    public class UIAutomationTimeoutException : UIAutomationException
    {
        public override string ErrorCategory => "Timeout";
        
        public int TimeoutSeconds { get; }

        public UIAutomationTimeoutException(string operation, string? elementId, int timeoutSeconds, string? details = null)
            : base(operation, elementId, $"{operation} timed out after {timeoutSeconds} seconds. The element '{elementId}' may not be responding or accessible. {details}".Trim())
        {
            TimeoutSeconds = timeoutSeconds;
            Suggestions = new[]
            {
                "Verify the element exists and is accessible",
                "Check if the target application is responding",
                "Consider increasing the timeout value",
                "Ensure the element ID is correct"
            };
        }
    }

    /// <summary>
    /// Exception thrown when an element doesn't support the requested operation
    /// </summary>
    public class UIAutomationInvalidOperationException : UIAutomationException
    {
        public override string ErrorCategory => "InvalidOperation";

        public UIAutomationInvalidOperationException(string operation, string? elementId, string? details = null)
            : base(operation, elementId, $"Cannot perform {operation} on element '{elementId}'. The element may not support this operation or may not be in the correct state. {details}".Trim())
        {
            Suggestions = new[]
            {
                "Verify the element supports the requested operation",
                "Check if the element is in the correct state",
                "Ensure the element is enabled and visible",
                "Try refreshing the element reference"
            };
        }
    }

    /// <summary>
    /// Exception thrown when element ID or parameters are invalid
    /// </summary>
    public class UIAutomationArgumentException : UIAutomationException
    {
        public override string ErrorCategory => "InvalidArgument";

        public UIAutomationArgumentException(string operation, string? elementId, string? details = null)
            : base(operation, elementId, $"Invalid element identifier '{elementId}' for {operation}. Please check that the automation ID is correct and the target window is accessible. {details}".Trim())
        {
            Suggestions = new[]
            {
                "Verify the element ID is spelled correctly",
                "Check if the target window is open and accessible",
                "Ensure the application is in the foreground",
                "Try using a different element locator strategy"
            };
        }
    }

    /// <summary>
    /// Exception thrown when access is denied
    /// </summary>
    public class UIAutomationUnauthorizedException : UIAutomationException
    {
        public override string ErrorCategory => "Unauthorized";

        public UIAutomationUnauthorizedException(string operation, string? elementId, string? details = null)
            : base(operation, elementId, $"Access denied when attempting {operation} on element '{elementId}'. The application may require elevated privileges. {details}".Trim())
        {
            Suggestions = new[]
            {
                "Run the application with administrator privileges",
                "Check if the target application allows UI automation",
                "Verify Windows UAC settings",
                "Ensure the target process is accessible"
            };
        }
    }

    /// <summary>
    /// Exception thrown when an element is not found
    /// </summary>
    public class UIAutomationElementNotFoundException : UIAutomationException
    {
        public override string ErrorCategory => "ElementNotFound";

        public UIAutomationElementNotFoundException(string operation, string? elementId, string? details = null)
            : base(operation, elementId, $"Element '{elementId}' not found during {operation}. {details}".Trim())
        {
            Suggestions = new[]
            {
                "Verify the element ID is correct",
                "Check if the target window is open",
                "Ensure the element is not hidden or disabled",
                "Wait for the element to appear before interacting"
            };
        }
    }

    /// <summary>
    /// Exception thrown for validation errors
    /// </summary>
    public class UIAutomationValidationException : UIAutomationException
    {
        public override string ErrorCategory => "Validation";

        public UIAutomationValidationException(string operation, string issue)
            : base(operation, null, $"Element ID is required for {operation} operation and cannot be empty")
        {
            Suggestions = new[]
            {
                "Provide a valid element ID",
                "Check the UI element's AutomationId property",
                "Use element inspection tools to find the correct ID"
            };
        }
    }
}