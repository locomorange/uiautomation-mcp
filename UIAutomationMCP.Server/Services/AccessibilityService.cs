using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public class AccessibilityService : IAccessibilityService
    {
        private readonly ILogger<AccessibilityService> _logger;
        private readonly SubprocessExecutor _executor;

        public AccessibilityService(ILogger<AccessibilityService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetAccessibilityInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetAccessibilityInfo validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["elementId"] = elementId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetAccessibilityInfo",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting accessibility info for element: {elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("GetAccessibilityInfo", parameters, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetAccessibilityInfo",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Accessibility info retrieved successfully for element: {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "ExecutionError",
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetAccessibilityInfo",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get accessibility info for element {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }

        public async Task<object> VerifyAccessibilityAsync(string? elementId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Verifying accessibility for element: {elementId ?? "window"}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("VerifyAccessibility", parameters, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "VerifyAccessibility",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Accessibility verification completed for element: {elementId ?? "window"}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "ExecutionError",
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "VerifyAccessibility",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to verify accessibility for element: {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }

        public async Task<object> GetLabeledByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetLabeledBy validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["elementId"] = elementId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetLabeledBy",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting labeled by info for element: {elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("GetLabeledBy", parameters, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetLabeledBy",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Labeled by info retrieved successfully for element: {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "ExecutionError",
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetLabeledBy",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get labeled by info for element {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }

        public async Task<object> GetDescribedByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetDescribedBy validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["elementId"] = elementId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetDescribedBy",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting described by info for element: {elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("GetDescribedBy", parameters, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetDescribedBy",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Described by info retrieved successfully for element: {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "ExecutionError",
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetDescribedBy",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get described by info for element {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }
    }
}