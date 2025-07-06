using UIAutomationMCP.Models;

namespace UIAutomationMCP.Server.Services
{
    /// <summary>
    /// Common utilities for timeout and execution time handling
    /// </summary>
    public static class TimeoutHelper
    {
        /// <summary>
        /// Creates a timeout error message with suggestions for improvement
        /// </summary>
        /// <param name="operationName">Name of the operation that timed out</param>
        /// <param name="timeoutSeconds">Timeout duration in seconds</param>
        /// <param name="executionSeconds">Actual execution time in seconds</param>
        /// <returns>Formatted error message with suggestions</returns>
        public static string CreateTimeoutMessage(string operationName, int timeoutSeconds, double executionSeconds)
        {
            var suggestions = new List<string>();

            // Suggest increasing timeout if it's relatively short
            if (timeoutSeconds <= 30)
            {
                suggestions.Add("increase timeout to 100+ seconds");
            }
            else if (timeoutSeconds <= 100)
            {
                suggestions.Add("increase timeout to 200+ seconds");
            }
            else if (timeoutSeconds <= 200)
            {
                suggestions.Add("increase timeout to 300+ seconds");
            }
            else if (timeoutSeconds <= 300)
            {
                suggestions.Add("consider increasing timeout further based on operation complexity");
            }

            // Suggest more specific criteria for element operations
            if (operationName.Contains("element", StringComparison.OrdinalIgnoreCase) ||
                operationName.Contains("find", StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add("use more specific search criteria (AutomationId, ControlType, ProcessId)");
                suggestions.Add("ensure the target window is active and visible");
            }

            // Suggest checking application state
            if (operationName.Contains("window", StringComparison.OrdinalIgnoreCase) || 
                operationName.Contains("application", StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add("ensure the target application is running and responsive");
            }

            var suggestionText = suggestions.Count > 0 
                ? " Consider: " + string.Join(", ", suggestions) + "."
                : "";

            return $"{operationName} timeout after {timeoutSeconds} seconds (executed for {executionSeconds:F1}s).{suggestionText}";
        }

        /// <summary>
        /// Creates an OperationResult with timeout information
        /// </summary>
        /// <typeparam name="T">Type of result data</typeparam>
        /// <param name="operationName">Name of the operation that timed out</param>
        /// <param name="timeoutSeconds">Timeout duration in seconds</param>
        /// <param name="executionSeconds">Actual execution time in seconds</param>
        /// <returns>OperationResult with timeout error</returns>
        public static OperationResult<T> CreateTimeoutResult<T>(string operationName, int timeoutSeconds, double executionSeconds)
        {
            return new OperationResult<T>
            {
                Success = false,
                Error = CreateTimeoutMessage(operationName, timeoutSeconds, executionSeconds),
                ExecutionSeconds = Math.Round(executionSeconds, 1)
            };
        }

        /// <summary>
        /// Creates an OperationResult with timeout information (non-generic version)
        /// </summary>
        /// <param name="operationName">Name of the operation that timed out</param>
        /// <param name="timeoutSeconds">Timeout duration in seconds</param>
        /// <param name="executionSeconds">Actual execution time in seconds</param>
        /// <returns>OperationResult with timeout error</returns>
        public static OperationResult CreateTimeoutResult(string operationName, int timeoutSeconds, double executionSeconds)
        {
            return new OperationResult
            {
                Success = false,
                Error = CreateTimeoutMessage(operationName, timeoutSeconds, executionSeconds),
                ExecutionSeconds = Math.Round(executionSeconds, 1)
            };
        }

        /// <summary>
        /// Creates a successful OperationResult with execution time
        /// </summary>
        /// <typeparam name="T">Type of result data</typeparam>
        /// <param name="data">Result data</param>
        /// <param name="executionSeconds">Execution time in seconds</param>
        /// <returns>Successful OperationResult</returns>
        public static OperationResult<T> CreateSuccessResult<T>(T data, double executionSeconds)
        {
            return new OperationResult<T>
            {
                Success = true,
                Data = data,
                ExecutionSeconds = Math.Round(executionSeconds, 1)
            };
        }

        /// <summary>
        /// Creates a failed OperationResult with execution time
        /// </summary>
        /// <typeparam name="T">Type of result data</typeparam>
        /// <param name="error">Error message</param>
        /// <param name="executionSeconds">Execution time in seconds</param>
        /// <returns>Failed OperationResult</returns>
        public static OperationResult<T> CreateErrorResult<T>(string error, double executionSeconds)
        {
            return new OperationResult<T>
            {
                Success = false,
                Error = error,
                ExecutionSeconds = Math.Round(executionSeconds, 1)
            };
        }

        /// <summary>
        /// Suggests optimal timeout values based on operation type
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="currentTimeout">Current timeout value</param>
        /// <returns>Suggested timeout in seconds</returns>
        public static int SuggestOptimalTimeout(string operationName, int currentTimeout)
        {
            var lowerOperation = operationName.ToLowerInvariant();

            // Fast operations
            if (lowerOperation.Contains("click") || lowerOperation.Contains("invoke") || 
                lowerOperation.Contains("toggle") || lowerOperation.Contains("set"))
            {
                return Math.Max(currentTimeout, 30);
            }

            // Medium operations
            if (lowerOperation.Contains("find") || lowerOperation.Contains("get") || 
                lowerOperation.Contains("scroll") || lowerOperation.Contains("select"))
            {
                return Math.Max(currentTimeout, 60);
            }

            // Slow operations
            if (lowerOperation.Contains("tree") || lowerOperation.Contains("screenshot") || 
                lowerOperation.Contains("launch") || lowerOperation.Contains("elements"))
            {
                return Math.Max(currentTimeout, 120);
            }

            // Default suggestion
            return Math.Max(currentTimeout, 60);
        }

        /// <summary>
        /// Adjusts timeout based on execution time patterns
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="currentTimeout">Current timeout value</param>
        /// <param name="executionSeconds">Actual execution time in seconds</param>
        /// <param name="wasSuccessful">Whether the operation was successful</param>
        /// <returns>Adjusted timeout in seconds</returns>
        public static int AdjustTimeoutBasedOnPerformance(string operationName, int currentTimeout, double executionSeconds, bool wasSuccessful)
        {
            // If operation was successful and took less than 50% of timeout, timeout is probably OK
            if (wasSuccessful && executionSeconds < (currentTimeout * 0.5))
            {
                return currentTimeout;
            }

            // If operation timed out or took more than 80% of timeout, suggest increase
            if (!wasSuccessful || executionSeconds > (currentTimeout * 0.8))
            {
                return SuggestOptimalTimeout(operationName, (int)(currentTimeout * 1.5));
            }

            // If operation took 50-80% of timeout, suggest moderate increase
            if (executionSeconds > (currentTimeout * 0.5))
            {
                return SuggestOptimalTimeout(operationName, (int)(currentTimeout * 1.2));
            }

            return currentTimeout;
        }
    }
}