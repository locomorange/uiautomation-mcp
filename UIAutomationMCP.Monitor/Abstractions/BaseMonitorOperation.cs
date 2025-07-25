using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Monitor.Abstractions
{
    /// <summary>
    /// Base class for Monitor operations providing common functionality
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResult">Result type</typeparam>
    public abstract class BaseMonitorOperation<TRequest, TResult> : IMonitorOperation<TRequest, TResult>, IMonitorOperation
        where TRequest : class
        where TResult : class
    {
        protected readonly SessionManager _sessionManager;
        protected readonly ElementFinderService _elementFinderService;
        protected readonly ILogger _logger;

        protected BaseMonitorOperation(
            SessionManager sessionManager,
            ElementFinderService elementFinderService,
            ILogger logger)
        {
            _sessionManager = sessionManager;
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        /// <summary>
        /// Execute operation with typed request and result
        /// </summary>
        public async Task<OperationResult<TResult>> ExecuteAsync(TRequest request)
        {
            try
            {
                _logger.LogDebug("Starting {OperationType} with request type {RequestType}", GetType().Name, typeof(TRequest).Name);
                
                // Validate request
                var validationResult = ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    var error = string.Join(", ", validationResult.Errors);
                    _logger.LogWarning("Request validation failed: {Error}", error);
                    return OperationResult<TResult>.FromError(error);
                }

                // Execute the operation
                var result = await ExecuteOperationAsync(request);
                
                _logger.LogDebug("Completed {OperationType} successfully", GetType().Name);
                return OperationResult<TResult>.FromSuccess(result);
            }
            catch (UIAutomationException ex)
            {
                _logger.LogWarning(ex, "UI Automation error in {OperationType}: {Error}", GetType().Name, ex.Message);
                return OperationResult<TResult>.FromError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in {OperationType}", GetType().Name);
                return OperationResult<TResult>.FromError($"Operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Legacy string-based interface implementation
        /// </summary>
        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var request = JsonSerializationHelper.Deserialize<TRequest>(parametersJson);
                if (request == null)
                {
                    return OperationResult.FromError("Failed to deserialize request");
                }

                var typedResult = await ExecuteAsync(request);
                
                return new OperationResult
                {
                    Success = typedResult.Success,
                    Error = typedResult.Error,
                    Data = typedResult.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing request for {OperationType}", GetType().Name);
                return OperationResult.FromError($"Request processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate the request parameters
        /// Override in derived classes for specific validation logic
        /// </summary>
        /// <param name="request">Request to validate</param>
        /// <returns>Validation result</returns>
        protected virtual Core.Validation.ValidationResult ValidateRequest(TRequest request)
        {
            if (request == null)
            {
                return Core.Validation.ValidationResult.Failure("Request cannot be null");
            }

            return Core.Validation.ValidationResult.Success;
        }

        /// <summary>
        /// Execute the specific operation logic
        /// Implement in derived classes
        /// </summary>
        /// <param name="request">Validated request</param>
        /// <returns>Operation result</returns>
        protected abstract Task<TResult> ExecuteOperationAsync(TRequest request);
    }
}