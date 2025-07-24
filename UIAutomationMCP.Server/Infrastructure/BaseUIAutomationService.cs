using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Abstractions;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Validation;

namespace UIAutomationMCP.Server.Infrastructure
{
    /// <summary>
    /// Base class for all UI Automation services providing unified execution patterns
    /// </summary>
    public abstract class BaseUIAutomationService
    {
        protected readonly IOperationExecutor _executor;
        protected readonly ILogger _logger;
        
        protected BaseUIAutomationService(IOperationExecutor executor, ILogger logger)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
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
                
                // 3. Execute operation
                var result = await _executor.ExecuteAsync<TRequest, TResult>(operationName, request, timeoutSeconds);
                
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
                    AdditionalInfo = CreateSuccessAdditionalInfo(data, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = methodName,
                    RequestParameters = CreateRequestParameters(context),
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
                    AdditionalInfo = CreateValidationErrorAdditionalInfo(validation, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = context.MethodName,
                    RequestParameters = CreateRequestParameters(context),
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
                ErrorMessage = result.Error,
                ExecutionInfo = new ServerExecutionInfo
                {
                    ServerProcessingTime = context.Stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                    OperationId = context.OperationId,
                    ServerExecutedAt = context.StartedAt,
                    ServerLogs = LogCollectorExtensions.Instance.GetLogs(context.OperationId),
                    AdditionalInfo = CreateOperationErrorAdditionalInfo(result, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = methodName,
                    RequestParameters = CreateRequestParameters(context),
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
                    AdditionalInfo = CreateUnhandledErrorAdditionalInfo(ex, context)
                },
                RequestMetadata = new RequestMetadata
                {
                    RequestedMethod = methodName,
                    RequestParameters = CreateRequestParameters(context),
                    TimeoutSeconds = timeoutSeconds
                }
            };
        }
        
        #endregion
        
        #region Additional Info Creation - Virtual for extensibility
        
        protected virtual Dictionary<string, object> CreateSuccessAdditionalInfo<TResult>(TResult data, IServiceContext context)
        {
            return new Dictionary<string, object>
            {
                ["operationCompleted"] = true,
                ["operationType"] = context.MethodName
            };
        }
        
        protected virtual Dictionary<string, object> CreateValidationErrorAdditionalInfo(ValidationResult validation, IServiceContext context)
        {
            return new Dictionary<string, object>
            {
                ["errorCategory"] = "Validation",
                ["validationErrors"] = validation.Errors,
                ["operationType"] = context.MethodName
            };
        }
        
        protected virtual Dictionary<string, object> CreateOperationErrorAdditionalInfo<TResult>(ServiceOperationResult<TResult> result, IServiceContext context)
        {
            return new Dictionary<string, object>
            {
                ["errorCategory"] = result.ErrorCategory ?? "OperationError",
                ["exceptionType"] = result.ExceptionType ?? "Unknown",
                ["operationType"] = context.MethodName
            };
        }
        
        protected virtual Dictionary<string, object> CreateUnhandledErrorAdditionalInfo(Exception ex, IServiceContext context)
        {
            return new Dictionary<string, object>
            {
                ["errorCategory"] = "UnhandledException",
                ["exceptionType"] = ex.GetType().Name,
                ["stackTrace"] = ex.StackTrace ?? "",
                ["operationType"] = context.MethodName
            };
        }
        
        protected virtual Dictionary<string, object> CreateRequestParameters(IServiceContext context)
        {
            return new Dictionary<string, object>
            {
                ["operationId"] = context.OperationId,
                ["timeoutSeconds"] = context.TimeoutSeconds
            };
        }
        
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