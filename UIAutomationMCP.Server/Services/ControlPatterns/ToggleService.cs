using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ToggleService : IToggleService
    {
        private readonly ILogger<ToggleService> _logger;
        private readonly SubprocessExecutor _executor;

        public ToggleService(ILogger<ToggleService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"ToggleElement validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
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
                        RequestedMethod = "ToggleElement",
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
                
                var validationJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationJson;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting ToggleElement for ElementId={elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("ToggleElement", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["operationType"] = "toggle",
                            ["actionPerformed"] = "elementToggled"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ToggleElement",
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

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in ToggleElement operation");
                
                var errorResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["elementId"] = elementId,
                            ["operationType"] = "toggle"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ToggleElement",
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
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }

        public async Task<object> GetToggleStateAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetToggleState validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ToggleStateResult>
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
                        RequestedMethod = "GetToggleState",
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
                
                var validationJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationJson;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetToggleState for ElementId={elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ToggleStateResult>("GetToggleState", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ToggleStateResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["toggleState"] = result.ToggleState ?? "unknown",
                            ["operationType"] = "getToggleState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetToggleState",
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

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetToggleState operation");
                
                var errorResponse = new ServerEnhancedResponse<ToggleStateResult>
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
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["elementId"] = elementId,
                            ["operationType"] = "getToggleState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetToggleState",
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
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }

        public async Task<object> SetToggleStateAsync(string elementId, string toggleState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"SetToggleState validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
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
                        RequestedMethod = "SetToggleState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["toggleState"] = toggleState ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                var validationJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationJson;
            }

            if (string.IsNullOrWhiteSpace(toggleState))
            {
                var validationError = "Toggle state is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"SetToggleState validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["toggleState"] = toggleState ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetToggleState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["toggleState"] = toggleState ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                var validationJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationJson;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting SetToggleState: ElementId={elementId}, ToggleState={toggleState}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "toggleState", toggleState },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("SetToggleState", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["toggleState"] = toggleState,
                            ["operationType"] = "setToggleState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetToggleState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["toggleState"] = toggleState,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in SetToggleState operation");
                
                var errorResponse = new ServerEnhancedResponse<ActionResult>
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
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["elementId"] = elementId,
                            ["toggleState"] = toggleState,
                            ["operationType"] = "setToggleState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetToggleState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["toggleState"] = toggleState,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }
    }
}
