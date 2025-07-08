using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Helpers
{
    public static class SubprocessErrorHandler
    {
        public static object HandleError(Exception ex, string operation, string elementId, int timeoutSeconds, ILogger logger)
        {
            return ex switch
            {
                TimeoutException timeoutEx => HandleTimeoutError(timeoutEx, operation, elementId, timeoutSeconds, logger),
                InvalidOperationException invalidOpEx => HandleInvalidOperationError(invalidOpEx, operation, elementId, logger),
                ArgumentException argEx => HandleArgumentError(argEx, operation, elementId, logger),
                UnauthorizedAccessException unauthorizedEx => HandleUnauthorizedError(unauthorizedEx, operation, elementId, logger),
                _ => HandleGenericError(ex, operation, elementId, logger)
            };
        }

        private static object HandleTimeoutError(TimeoutException ex, string operation, string elementId, int timeoutSeconds, ILogger logger)
        {
            var userError = $"{GetOperationDisplayName(operation)} timed out after {timeoutSeconds} seconds. The element '{elementId}' may not be responding or accessible.";
            logger.LogError(ex, "{Operation} operation timed out for element: {ElementId}", operation, elementId);
            
            return new
            {
                Success = false,
                Error = userError,
                ElementId = elementId,
                Operation = operation,
                TimeoutSeconds = timeoutSeconds,
                ErrorCategory = "Timeout",
                Suggestions = new[]
                {
                    "Verify the element exists and is accessible",
                    "Check if the target application is responding",
                    "Consider increasing the timeout value",
                    "Ensure the element ID is correct"
                }
            };
        }

        private static object HandleInvalidOperationError(InvalidOperationException ex, string operation, string elementId, ILogger logger)
        {
            var userError = $"Cannot perform {GetOperationDisplayName(operation).ToLower()} on element '{elementId}'. The element may not support this operation or may not be in the correct state.";
            logger.LogError(ex, "Invalid {Operation} operation for element: {ElementId}", operation, elementId);
            
            return new
            {
                Success = false,
                Error = userError,
                ElementId = elementId,
                Operation = operation,
                ErrorCategory = "InvalidOperation",
                Details = ex.Message,
                Suggestions = new[]
                {
                    "Verify the element supports the requested operation",
                    "Check if the element is in the correct state",
                    "Ensure the element is enabled and visible",
                    "Try refreshing the element reference"
                }
            };
        }

        private static object HandleArgumentError(ArgumentException ex, string operation, string elementId, ILogger logger)
        {
            var userError = $"Invalid element identifier '{elementId}' for {GetOperationDisplayName(operation).ToLower()}. Please check that the element ID is correct and the target window is accessible.";
            logger.LogError(ex, "Invalid arguments for {Operation} operation: {ElementId}", operation, elementId);
            
            return new
            {
                Success = false,
                Error = userError,
                ElementId = elementId,
                Operation = operation,
                ErrorCategory = "InvalidArgument",
                Details = ex.Message,
                Suggestions = new[]
                {
                    "Verify the element ID is spelled correctly",
                    "Check if the target window is open and accessible",
                    "Ensure the application is in the foreground",
                    "Try using a different element locator strategy"
                }
            };
        }

        private static object HandleUnauthorizedError(UnauthorizedAccessException ex, string operation, string elementId, ILogger logger)
        {
            var userError = $"Access denied when attempting {GetOperationDisplayName(operation).ToLower()} on element '{elementId}'. The application may require elevated privileges.";
            logger.LogError(ex, "Unauthorized access during {Operation} operation for element: {ElementId}", operation, elementId);
            
            return new
            {
                Success = false,
                Error = userError,
                ElementId = elementId,
                Operation = operation,
                ErrorCategory = "Unauthorized",
                Details = ex.Message,
                Suggestions = new[]
                {
                    "Run the application with administrator privileges",
                    "Check if the target application allows UI automation",
                    "Verify Windows UAC settings",
                    "Ensure the target process is accessible"
                }
            };
        }

        private static object HandleGenericError(Exception ex, string operation, string elementId, ILogger logger)
        {
            var userError = $"Failed to perform {GetOperationDisplayName(operation).ToLower()} on element '{elementId}'. This could be due to the element not being found, the application not responding, or system issues.";
            logger.LogError(ex, "Unexpected error during {Operation} operation for element: {ElementId}. Exception: {ExceptionType}", 
                operation, elementId, ex.GetType().Name);
            
            return new
            {
                Success = false,
                Error = userError,
                ElementId = elementId,
                Operation = operation,
                ErrorCategory = "Unexpected",
                ExceptionType = ex.GetType().Name,
                Details = ex.Message,
                Suggestions = new[]
                {
                    "Verify the element exists and is accessible",
                    "Check if the target application is running",
                    "Ensure system resources are available",
                    "Try the operation again after a brief delay"
                }
            };
        }

        public static object? ValidateElementId(string elementId, string operation, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = $"Element ID is required for {GetOperationDisplayName(operation).ToLower()} operation and cannot be empty";
                logger.LogWarning("{Operation} operation failed due to validation: {Error}", operation, validationError);
                
                return new
                {
                    Success = false,
                    Error = validationError,
                    Operation = operation,
                    ErrorCategory = "Validation",
                    Suggestions = new[]
                    {
                        "Provide a valid element ID",
                        "Check the UI element's AutomationId property",
                        "Use element inspection tools to find the correct ID"
                    }
                };
            }
            
            return null; // Validation passed
        }

        private static string GetOperationDisplayName(string operation)
        {
            return operation switch
            {
                "ToggleElement" => "Toggle",
                "InvokeElement" => "Invoke",
                "SetElementValue" => "Set Value",
                "GetElementValue" => "Get Value",
                "GetText" => "Get Text",
                "SetText" => "Set Text",
                "SelectText" => "Select Text",
                "FindText" => "Find Text",
                "GetTextSelection" => "Get Text Selection",
                "TraverseText" => "Traverse Text",
                "GetTextAttributes" => "Get Text Attributes",
                "WindowAction" => "Window Action",
                "TransformElement" => "Transform Element",
                "SelectElement" => "Select Element",
                "GetSelection" => "Get Selection",
                "ExpandCollapseElement" => "Expand/Collapse",
                "ScrollElement" => "Scroll",
                "ScrollElementIntoView" => "Scroll Into View",
                "DockElement" => "Dock",
                "SetRangeValue" => "Set Range Value",
                "GetRangeValue" => "Get Range Value",
                "FindElements" => "Find Elements",
                "GetDesktopWindows" => "Get Desktop Windows",
                _ => operation
            };
        }
    }
}