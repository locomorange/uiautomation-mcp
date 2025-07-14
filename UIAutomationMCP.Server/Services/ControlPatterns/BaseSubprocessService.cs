using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    /// <summary>
    /// Base class for subprocess-based UI automation services that provides common functionality
    /// for parameter management, logging, error handling, and subprocess execution.
    /// </summary>
    /// <typeparam name="TService">The concrete service type for proper logger typing</typeparam>
    public abstract class BaseSubprocessService<TService>
    {
        protected readonly ILogger<TService> _logger;
        protected readonly SubprocessExecutor _executor;

        protected BaseSubprocessService(ILogger<TService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        /// <summary>
        /// Creates a standard parameter dictionary with common UI automation parameters
        /// </summary>
        /// <param name="elementId">The element identifier</param>
        /// <param name="windowTitle">Optional window title</param>
        /// <param name="processId">Optional process ID</param>
        /// <param name="additionalParameters">Additional operation-specific parameters</param>
        /// <returns>Dictionary containing all parameters for subprocess execution</returns>
        protected Dictionary<string, object> CreateParameterDictionary(
            string elementId, 
            string? windowTitle = null, 
            int? processId = null,
            Dictionary<string, object>? additionalParameters = null)
        {
            var parameters = new Dictionary<string, object>
            {
                { "elementId", elementId },
                { "windowTitle", windowTitle ?? "" },
                { "processId", processId ?? 0 }
            };

            if (additionalParameters != null)
            {
                foreach (var param in additionalParameters)
                {
                    parameters[param.Key] = param.Value;
                }
            }

            return parameters;
        }

        /// <summary>
        /// Creates a parameter dictionary for window-based operations (without elementId)
        /// </summary>
        /// <param name="windowTitle">Optional window title</param>
        /// <param name="processId">Optional process ID</param>
        /// <param name="additionalParameters">Additional operation-specific parameters</param>
        /// <returns>Dictionary containing window parameters for subprocess execution</returns>
        protected Dictionary<string, object> CreateWindowParameterDictionary(
            string? windowTitle = null,
            int? processId = null,
            Dictionary<string, object>? additionalParameters = null)
        {
            var parameters = new Dictionary<string, object>
            {
                { "windowTitle", windowTitle ?? "" },
                { "processId", processId ?? 0 }
            };

            if (additionalParameters != null)
            {
                foreach (var param in additionalParameters)
                {
                    parameters[param.Key] = param.Value;
                }
            }

            return parameters;
        }

        /// <summary>
        /// Executes a subprocess operation with standardized error handling and logging
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation name</param>
        /// <param name="parameters">Operation parameters</param>
        /// <param name="timeoutSeconds">Timeout for the operation</param>
        /// <param name="contextInfo">Additional context information for logging</param>
        /// <returns>Result of the operation</returns>
        protected async Task<object> ExecuteOperationAsync<T>(
            string operation,
            Dictionary<string, object> parameters,
            int timeoutSeconds,
            string? contextInfo = null)
        {
            try
            {
                var elementId = parameters.GetValueOrDefault("elementId")?.ToString();
                var windowTitle = parameters.GetValueOrDefault("windowTitle")?.ToString();
                var processId = parameters.GetValueOrDefault("processId");

                _logger.LogInformation("Executing {Operation} for element: {ElementId} in window: {WindowTitle} (ProcessId: {ProcessId}){Context}", 
                    operation, elementId ?? "N/A", windowTitle ?? "any", processId ?? 0, 
                    contextInfo != null ? $" - {contextInfo}" : "");

                var result = await _executor.ExecuteAsync<T>(operation, parameters, timeoutSeconds);

                _logger.LogInformation("{Operation} executed successfully for element: {ElementId}", 
                    operation, elementId ?? "N/A");

                return CreateSuccessResult(operation, result, elementId, contextInfo);
            }
            catch (Exception ex)
            {
                var elementId = parameters.GetValueOrDefault("elementId")?.ToString();
                return SubprocessErrorHandler.HandleError(ex, operation, elementId ?? "unknown", timeoutSeconds, _logger);
            }
        }

        /// <summary>
        /// Executes a subprocess operation with simple error handling (basic try-catch)
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation name</param>
        /// <param name="parameters">Operation parameters</param>
        /// <param name="timeoutSeconds">Timeout for the operation</param>
        /// <param name="contextInfo">Additional context information for logging</param>
        /// <returns>Result of the operation</returns>
        protected async Task<object> ExecuteSimpleOperationAsync<T>(
            string operation,
            Dictionary<string, object> parameters,
            int timeoutSeconds,
            string? contextInfo = null)
        {
            try
            {
                var elementId = parameters.GetValueOrDefault("elementId")?.ToString();
                var windowTitle = parameters.GetValueOrDefault("windowTitle")?.ToString();
                var processId = parameters.GetValueOrDefault("processId");

                _logger.LogInformation("Executing {Operation} for element: {ElementId} in window: {WindowTitle} (ProcessId: {ProcessId}){Context}", 
                    operation, elementId ?? "N/A", windowTitle ?? "any", processId ?? 0, 
                    contextInfo != null ? $" - {contextInfo}" : "");

                var result = await _executor.ExecuteAsync<T>(operation, parameters, timeoutSeconds);

                _logger.LogInformation("{Operation} executed successfully for element: {ElementId}", 
                    operation, elementId ?? "N/A");

                return CreateSimpleSuccessResult(operation, result, elementId, contextInfo);
            }
            catch (Exception ex)
            {
                var elementId = parameters.GetValueOrDefault("elementId")?.ToString();
                _logger.LogError(ex, "Failed to execute {Operation} for element: {ElementId}", operation, elementId ?? "unknown");
                return new { Success = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Validates element ID and returns validation result if invalid
        /// </summary>
        /// <param name="elementId">The element ID to validate</param>
        /// <param name="operation">The operation name for error context</param>
        /// <returns>Validation error result if invalid, null if valid</returns>
        protected object? ValidateElementId(string elementId, string operation)
        {
            return SubprocessErrorHandler.ValidateElementId(elementId, operation, _logger);
        }

        /// <summary>
        /// Validates that a string parameter is not null or empty
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="parameterName">The parameter name for error messages</param>
        /// <param name="operation">The operation name for error context</param>
        /// <returns>Validation error result if invalid, null if valid</returns>
        protected object? ValidateRequiredParameter(string? value, string parameterName, string operation)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var validationError = $"{parameterName} is required and cannot be empty";
                _logger.LogWarning("{Operation} operation failed due to validation: {Error}", operation, validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }
            return null;
        }

        /// <summary>
        /// Creates a standardized success result with enhanced metadata
        /// </summary>
        /// <param name="operation">The operation name</param>
        /// <param name="data">The operation result data</param>
        /// <param name="elementId">The element ID (if applicable)</param>
        /// <param name="contextInfo">Additional context information</param>
        /// <returns>Standardized success result object</returns>
        protected virtual object CreateSuccessResult(string operation, object? data, string? elementId, string? contextInfo)
        {
            return new
            {
                Success = true,
                Data = data,
                Message = $"{operation} executed successfully" + (elementId != null ? $" for element '{elementId}'" : ""),
                ElementId = elementId,
                Operation = operation,
                Context = contextInfo
            };
        }

        /// <summary>
        /// Creates a simple success result with minimal metadata
        /// </summary>
        /// <param name="operation">The operation name</param>
        /// <param name="data">The operation result data</param>
        /// <param name="elementId">The element ID (if applicable)</param>
        /// <param name="contextInfo">Additional context information</param>
        /// <returns>Simple success result object</returns>
        protected virtual object CreateSimpleSuccessResult(string operation, object? data, string? elementId, string? contextInfo)
        {
            if (data != null)
            {
                return new { Success = true, Data = data };
            }
            return new { Success = true, Message = $"{operation} executed successfully" };
        }

        /// <summary>
        /// Creates a success result with custom message
        /// </summary>
        /// <param name="message">Custom success message</param>
        /// <param name="data">Optional result data</param>
        /// <param name="additionalProperties">Additional properties to include</param>
        /// <returns>Success result object</returns>
        protected object CreateCustomSuccessResult(string message, object? data = null, Dictionary<string, object>? additionalProperties = null)
        {
            var result = new Dictionary<string, object>
            {
                { "Success", true },
                { "Message", message }
            };

            if (data != null)
            {
                result["Data"] = data;
            }

            if (additionalProperties != null)
            {
                foreach (var prop in additionalProperties)
                {
                    result[prop.Key] = prop.Value;
                }
            }

            return result;
        }
    }
}