using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Services;

namespace UIAutomationMCP.Subprocess.Core.Abstractions
{
    /// <summary>
    /// Base class for UI Automation operations providing common functionality
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResult">Result type</typeparam>
    public abstract class BaseUIAutomationOperation<TRequest, TResult> : IUIAutomationOperation<TRequest, TResult>, IUIAutomationOperation
        where TRequest : class
        where TResult : class
    {
        protected readonly ElementFinderService _elementFinderService;
        protected readonly ILogger _logger;

        /// <summary>
        /// Default operation timeout in seconds. Override in derived classes for specific timeout values.
        /// </summary>
        protected virtual int OperationTimeoutSeconds => 55;

        /// <summary>
        /// Cancellation token for the current operation. Operations should check this token
        /// periodically, especially in loops or between COM calls.
        /// Note: Synchronous COM calls cannot be interrupted, but checking this token allows
        /// operations to stop processing between calls when a timeout or cancellation occurs.
        /// </summary>
        protected CancellationToken OperationCancellationToken { get; private set; }

        protected BaseUIAutomationOperation(ElementFinderService elementFinderService, ILogger logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        /// <summary>
        /// Execute operation with typed request and result
        /// </summary>
        public async Task<OperationResult<TResult>> ExecuteAsync(TRequest request)
        {
            return await ExecuteAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Execute operation with typed request, result, and cancellation support
        /// </summary>
        public async Task<OperationResult<TResult>> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
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

                // Execute the operation with timeout protection
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(OperationTimeoutSeconds));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

                // Expose the cancellation token so operations can check it cooperatively
                OperationCancellationToken = combinedCts.Token;

                var operationTask = Task.Run(() => ExecuteOperationAsync(request), combinedCts.Token);
                var timeoutTask = Task.Delay(Timeout.Infinite, combinedCts.Token);

                var completedTask = await Task.WhenAny(operationTask, timeoutTask);

                if (completedTask != operationTask)
                {
                    // Timeout or cancellation fired before operation completed
                    var reason = timeoutCts.Token.IsCancellationRequested
                        ? $"Operation '{GetType().Name}' timed out after {OperationTimeoutSeconds} seconds"
                        : $"Operation '{GetType().Name}' was cancelled";
                    _logger.LogWarning("{Reason}. Note: the background operation may still be running until the next COM call completes.", reason);
                    return OperationResult<TResult>.FromError(reason);
                }

                var result = await operationTask;

                _logger.LogDebug("Completed {OperationType} successfully", GetType().Name);
                return OperationResult<TResult>.FromSuccess(result);
            }
            catch (OperationCanceledException)
            {
                var message = $"Operation '{GetType().Name}' was cancelled or timed out after {OperationTimeoutSeconds} seconds";
                _logger.LogWarning("{Message}", message);
                return OperationResult<TResult>.FromError(message);
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
        /// JSON string-based interface implementation
        /// </summary>
        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            return await ExecuteAsync(parametersJson, CancellationToken.None);
        }

        /// <summary>
        /// JSON string-based interface implementation with cancellation support
        /// </summary>
        public async Task<OperationResult> ExecuteAsync(string parametersJson, CancellationToken cancellationToken)
        {
            try
            {
                TRequest? request;
                if (string.IsNullOrEmpty(parametersJson))
                {
                    request = default(TRequest);
                }
                else
                {
                    // Deserialize JSON string to TRequest
                    request = JsonSerializationHelper.Deserialize<TRequest>(parametersJson);
                }

                if (request == null)
                {
                    return OperationResult.FromError("Failed to deserialize JSON parameters to request type");
                }

                var typedResult = await ExecuteAsync(request, cancellationToken);

                return new OperationResult
                {
                    Success = typedResult.Success,
                    Error = typedResult.Error,
                    Data = typedResult.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteAsync (JSON parameters) for {OperationType}", GetType().Name);
                return OperationResult.FromError($"Operation failed: {ex.Message}");
            }
        }


        /// <summary>
        /// Validate the request parameters
        /// Override in derived classes for specific validation logic
        /// </summary>
        /// <param name="request">Request to validate</param>
        /// <returns>Validation result</returns>
        protected virtual UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(TRequest request)
        {
            if (request == null)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Request cannot be null");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        /// <summary>
        /// Execute the specific operation logic
        /// Implement in derived classes.
        /// Operations can check <see cref="OperationCancellationToken"/> for cooperative cancellation,
        /// especially in loops or between multiple COM calls.
        /// </summary>
        /// <param name="request">Validated request</param>
        /// <returns>Operation result</returns>
        protected abstract Task<TResult> ExecuteOperationAsync(TRequest request);
    }
}

