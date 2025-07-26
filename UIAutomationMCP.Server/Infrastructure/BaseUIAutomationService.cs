using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Core.Abstractions;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Core.Validation;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Abstractions;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// Base class for all UI Automation services providing unified execution patterns with type-safe metadata
    /// </summary>
    public abstract class BaseUIAutomationService<TMetadata> 
        where TMetadata : ServiceMetadata, new()
    {
        protected readonly IProcessManager _processManager;
        protected readonly ILogger _logger;
        
        protected BaseUIAutomationService(IProcessManager processManager, ILogger logger)
        {
            _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Execute a service operation with unified error handling, logging, and response creation
        /// </summary>
        protected async Task<ServerEnhancedResponse<TResult>> ExecuteServiceOperationAsync<TRequest, TResult>(
            string operationName,
            TRequest request,
            string methodName,
            int timeoutSeconds = 30,
            Func<TRequest, ValidationResult>? customValidation = null)
            where TRequest : notnull
            where TResult : notnull
        {
            var context = new ServiceContext(methodName, timeoutSeconds);
            
            try
            {
                // 1. Unified validation
                var validation = ValidateRequest(request, customValidation);
                if (!validation.IsValid)
                {
                    return CreateValidationErrorResponse<TResult>(validation, context);
                }
                
                // 2. Unified logging - start
                LogOperationStart(operationName, context);
                
                // 3. Execute operation in Worker process
                var result = await _processManager.ExecuteWorkerOperationAsync<TRequest, TResult>(operationName, request, timeoutSeconds);
                
                // 4. Handle result
                if (result.Success)
                {
                    LogOperationSuccess(operationName, context);
                    return CreateSuccessResponse(result.Data!, context, methodName, timeoutSeconds);
                }
                else
                {
                    LogOperationError(operationName, result.Error!, context);
                    return CreateOperationErrorResponse<TResult>(result, context, methodName, timeoutSeconds);
                }
            }
            catch (Exception ex)
            {
                LogUnhandledException(operationName, ex, context);
                return CreateUnhandledErrorResponse<TResult>(ex, context, methodName, timeoutSeconds);
            }
            finally
            {
                context.Stopwatch.Stop();
                CleanupLogs(context.OperationId);
            }
        }
        
        /// <summary>
        /// Execute a service operation in Monitor process with unified error handling, logging, and response creation
        /// </summary>
        protected async Task<ServerEnhancedResponse<TResult>> ExecuteMonitorServiceOperationAsync<TRequest, TResult>(
            string operationName,
            TRequest request,
            string methodName,
            int timeoutSeconds = 30,
            Func<TRequest, ValidationResult>? customValidation = null)
            where TRequest : notnull
            where TResult : notnull
        {
            var context = new ServiceContext(methodName, timeoutSeconds);
            
            try
            {
                // 1. Unified validation
                var validation = ValidateRequest(request, customValidation);
                if (!validation.IsValid)
                {
                    return CreateValidationErrorResponse<TResult>(validation, context);
                }
                
                // 2. Unified logging - start
                LogOperationStart(operationName, context);
                
                // 3. Execute operation in Monitor process
                var result = await _processManager.ExecuteMonitorOperationAsync<TRequest, TResult>(operationName, request, timeoutSeconds);
                
                // 4. Handle result
                if (result.Success)
                {
                    LogOperationSuccess(operationName, context);
                    return CreateSuccessResponse(result.Data!, context, methodName, timeoutSeconds);
                }
                else
                {
                    LogOperationError(operationName, result.Error!, context);
                    return CreateOperationErrorResponse<TResult>(result, context, methodName, timeoutSeconds);
                }
            }
            catch (Exception ex)
            {
                LogUnhandledException(operationName, ex, context);
                return CreateUnhandledErrorResponse<TResult>(ex, context, methodName, timeoutSeconds);
            }
            finally
            {
                context.Stopwatch.Stop();
                CleanupLogs(context.OperationId);
            }
        }
        
        /// <summary>
        /// Validate request - override for custom validation logic
        /// </summary>
        protected virtual ValidationResult ValidateRequest<TRequest>(
            TRequest request, 
            Func<TRequest, ValidationResult>? customValidation = null)
        {
            return customValidation?.Invoke(request) ?? ValidationResult.Success;
        }
        
        #region Response Creation
        
        private ServerEnhancedResponse<TResult> CreateSuccessResponse<TResult>(
            TResult data, 
            IServiceContext context, 
            string methodName, 
            int timeoutSeconds)
        {
            return new ServerEnhancedResponse<TResult>
            {
                Success = true,
                Data = data,
                ExecutionInfo = new ServerExecutionInfo
                {
                    ServerProcessingTime = context.Stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                    OperationId = context.OperationId,
                    ServerExecutedAt = context.StartedAt,
                    ServerLogs = LogCollectorExtensions.Instance.GetLogs(context.OperationId),
                    Metadata = CreateSuccessMetadata(data, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = methodName,
                    OperationId = context.OperationId,
                    TimeoutSeconds = timeoutSeconds
                }
            };
        }
        
        private ServerEnhancedResponse<TResult> CreateValidationErrorResponse<TResult>(
            ValidationResult validation, 
            IServiceContext context)
        {
            var errorMessage = $"Validation failed: {string.Join(", ", validation.Errors)}";
            
            return new ServerEnhancedResponse<TResult>
            {
                Success = false,
                ErrorMessage = errorMessage,
                ExecutionInfo = new ServerExecutionInfo
                {
                    ServerProcessingTime = context.Stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                    OperationId = context.OperationId,
                    ServerExecutedAt = context.StartedAt,
                    ServerLogs = LogCollectorExtensions.Instance.GetLogs(context.OperationId),
                    Metadata = CreateValidationErrorMetadata(validation, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = context.MethodName,
                    OperationId = context.OperationId,
                    TimeoutSeconds = context.TimeoutSeconds
                }
            };
        }
        
        private ServerEnhancedResponse<TResult> CreateOperationErrorResponse<TResult>(
            ServiceOperationResult<TResult> result, 
            IServiceContext context, 
            string methodName, 
            int timeoutSeconds)
        {
            return new ServerEnhancedResponse<TResult>
            {
                Success = false,
                ErrorMessage = result.Error ?? string.Empty,
                ExecutionInfo = new ServerExecutionInfo
                {
                    ServerProcessingTime = context.Stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                    OperationId = context.OperationId,
                    ServerExecutedAt = context.StartedAt,
                    ServerLogs = LogCollectorExtensions.Instance.GetLogs(context.OperationId),
                    Metadata = CreateOperationErrorMetadata(result, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = methodName,
                    OperationId = context.OperationId,
                    TimeoutSeconds = timeoutSeconds
                }
            };
        }
        
        private ServerEnhancedResponse<TResult> CreateUnhandledErrorResponse<TResult>(
            Exception ex, 
            IServiceContext context, 
            string methodName, 
            int timeoutSeconds)
        {
            return new ServerEnhancedResponse<TResult>
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionInfo = new ServerExecutionInfo
                {
                    ServerProcessingTime = context.Stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                    OperationId = context.OperationId,
                    ServerExecutedAt = context.StartedAt,
                    ServerLogs = LogCollectorExtensions.Instance.GetLogs(context.OperationId),
                    Metadata = CreateUnhandledErrorMetadata(ex, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = methodName,
                    OperationId = context.OperationId,
                    TimeoutSeconds = timeoutSeconds
                }
            };
        }
        
        #endregion
        
        #region Metadata Creation - Type-safe and extensible
        
        /// <summary>
        /// Create success metadata - override in derived classes for service-specific information
        /// </summary>
        protected virtual TMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            return new TMetadata
            {
                OperationType = GetOperationType(),
                OperationCompleted = true,
                ExecutionTimeMs = context.Stopwatch.Elapsed.TotalMilliseconds,
                MethodName = context.MethodName
            };
        }
        
        /// <summary>
        /// Create validation error metadata
        /// </summary>
        protected virtual TMetadata CreateValidationErrorMetadata(ValidationResult validation, IServiceContext context)
        {
            return new TMetadata
            {
                OperationType = GetOperationType(),
                OperationCompleted = false,
                ExecutionTimeMs = context.Stopwatch.Elapsed.TotalMilliseconds,
                MethodName = context.MethodName
            };
        }
        
        /// <summary>
        /// Create operation error metadata
        /// </summary>
        protected virtual TMetadata CreateOperationErrorMetadata<TResult>(ServiceOperationResult<TResult> result, IServiceContext context)
        {
            return new TMetadata
            {
                OperationType = GetOperationType(),
                OperationCompleted = false,
                ExecutionTimeMs = context.Stopwatch.Elapsed.TotalMilliseconds,
                MethodName = context.MethodName
            };
        }
        
        /// <summary>
        /// Create unhandled error metadata
        /// </summary>
        protected virtual TMetadata CreateUnhandledErrorMetadata(Exception ex, IServiceContext context)
        {
            return new TMetadata
            {
                OperationType = GetOperationType(),
                OperationCompleted = false,
                ExecutionTimeMs = context.Stopwatch.Elapsed.TotalMilliseconds,
                MethodName = context.MethodName
            };
        }
        
        /// <summary>
        /// Get the operation type for this service - implement in derived classes
        /// </summary>
        protected abstract string GetOperationType();
        
        #endregion
        
        #region Logging
        
        private void LogOperationStart(string operationName, IServiceContext context)
        {
            _logger.LogInformationWithOperation(context.OperationId, 
                $"Starting {operationName} operation via {context.MethodName}");
        }
        
        private void LogOperationSuccess(string operationName, IServiceContext context)
        {
            _logger.LogInformationWithOperation(context.OperationId, 
                $"Successfully completed {operationName} operation in {context.Stopwatch.Elapsed.TotalMilliseconds:F2}ms");
        }
        
        private void LogOperationError(string operationName, string error, IServiceContext context)
        {
            _logger.LogError("Operation {OperationName} failed: {Error} (OperationId: {OperationId})", 
                operationName, error, context.OperationId);
        }
        
        private void LogUnhandledException(string operationName, Exception ex, IServiceContext context)
        {
            _logger.LogErrorWithOperation(context.OperationId, ex,
                $"Unhandled exception in {operationName} operation");
        }
        
        private void CleanupLogs(string operationId)
        {
            LogCollectorExtensions.Instance.ClearLogs(operationId);
        }
        
        #endregion
    }
}