using UIAutomationMCP.Shared.Exceptions;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.ErrorHandling
{
    /// <summary>
    /// Centralized error handler registry for consistent error processing across all layers
    /// </summary>
    public static class ErrorHandlerRegistry
    {
        /// <summary>
        /// Convert any exception to standardized ErrorResult
        /// </summary>
        public static ErrorResult HandleException(Exception exception, string operation, string? elementId = null, int? timeoutSeconds = null, Action<Exception, string, string?, string?>? logAction = null)
        {
            // Log the exception if log action is provided
            logAction?.Invoke(exception, operation, elementId, exception.GetType().Name);

            return exception switch
            {
                UIAutomationTimeoutException timeoutEx => CreateErrorResult(timeoutEx),
                UIAutomationInvalidOperationException invalidOpEx => CreateErrorResult(invalidOpEx),
                UIAutomationArgumentException argEx => CreateErrorResult(argEx),
                UIAutomationUnauthorizedException unauthorizedEx => CreateErrorResult(unauthorizedEx),
                UIAutomationElementNotFoundException notFoundEx => CreateErrorResult(notFoundEx),
                UIAutomationValidationException validationEx => CreateErrorResult(validationEx),
                
                // Map standard .NET exceptions to UI automation exceptions
                TimeoutException ex => CreateErrorResult(new UIAutomationTimeoutException(operation, elementId, timeoutSeconds ?? 60, ex.Message)),
                InvalidOperationException ex => CreateErrorResult(new UIAutomationInvalidOperationException(operation, elementId, ex.Message)),
                ArgumentException ex => CreateErrorResult(new UIAutomationArgumentException(operation, elementId, ex.Message)),
                UnauthorizedAccessException ex => CreateErrorResult(new UIAutomationUnauthorizedException(operation, elementId, ex.Message)),
                
                // Generic handling for unexpected exceptions
                _ => ErrorResult.CreateGenericError(operation, elementId ?? "", exception.GetType().Name, exception.Message)
            };
        }

        /// <summary>
        /// Create ErrorResult from UIAutomationException
        /// </summary>
        private static ErrorResult CreateErrorResult(UIAutomationException exception)
        {
            return new ErrorResult
            {
                Success = false,
                Error = exception.Message,
                AutomationId = exception.ElementId,
                Operation = exception.Operation,
                ErrorCategory = exception.ErrorCategory,
                ExceptionType = exception.GetType().Name,
                Details = exception.InnerException?.Message,
                Suggestions = exception.Suggestions,
                TimeoutSeconds = (exception as UIAutomationTimeoutException)?.TimeoutSeconds
            };
        }

        /// <summary>
        /// Validate element ID and return validation error if invalid
        /// </summary>
        public static ErrorResult? ValidateElementId(string? elementId, string operation, Action<string, string, string>? logAction = null)
        {
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationException = new UIAutomationValidationException(operation, "Element ID cannot be empty");
                logAction?.Invoke(operation, "validation", validationException.Message);
                return CreateErrorResult(validationException);
            }
            
            return null; // Validation passed
        }

        /// <summary>
        /// Create a success result with operation metadata
        /// </summary>
        public static T CreateSuccessResult<T>(T data, string operation, string? elementId = null) where T : class
        {
            // If the result type has metadata properties, we could set them here
            // For now, just return the data as-is
            return data;
        }

        /// <summary>
        /// Wrap exception handling for async operations
        /// </summary>
        public static async Task<OperationResult> HandleAsync(Func<Task<OperationResult>> operation, string operationName, string? elementId = null, Action<Exception, string, string?, string?>? logAction = null)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                var errorResult = HandleException(ex, operationName, elementId, logAction: logAction);
                return new OperationResult
                {
                    Success = false,
                    Error = errorResult.Error,
                    Data = errorResult
                };
            }
        }

        /// <summary>
        /// Wrap exception handling for sync operations
        /// </summary>
        public static OperationResult Handle(Func<OperationResult> operation, string operationName, string? elementId = null, Action<Exception, string, string?, string?>? logAction = null)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                var errorResult = HandleException(ex, operationName, elementId, logAction: logAction);
                return new OperationResult
                {
                    Success = false,
                    Error = errorResult.Error,
                    Data = errorResult
                };
            }
        }
    }
}