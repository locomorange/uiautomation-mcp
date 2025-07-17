using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class WindowService : IWindowService
    {
        private readonly ILogger<WindowService> _logger;
        private readonly SubprocessExecutor _executor;

        public WindowService(ILogger<WindowService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> WindowOperationAsync(string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(operation))
            {
                var validationError = "Window operation is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"WindowOperation validation failed: {validationError}");
                
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
                            ["operation"] = operation ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "WindowOperation",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["operation"] = operation ?? "",
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
                _logger.LogInformationWithOperation(operationId, $"Starting WindowOperation: {operation} on window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "action", operation },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("WindowAction", parameters, timeoutSeconds);

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
                            ["operation"] = operation,
                            ["windowTitle"] = windowTitle ?? "",
                            ["operationType"] = "windowOperation"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "WindowOperation",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["operation"] = operation,
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in WindowOperation");
                
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
                            ["operation"] = operation,
                            ["windowTitle"] = windowTitle ?? "",
                            ["operationType"] = "windowOperation"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "WindowOperation",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["operation"] = operation,
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

        public async Task<object> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"TransformElement validation failed: {validationError}");
                
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
                        RequestedMethod = "TransformElement",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId ?? "",
                            ["action"] = action ?? "",
                            ["x"] = x ?? 0,
                            ["y"] = y ?? 0,
                            ["width"] = width ?? 0,
                            ["height"] = height ?? 0,
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

            if (string.IsNullOrWhiteSpace(action))
            {
                var validationError = "Transform action is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"TransformElement validation failed: {validationError}");
                
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
                            ["action"] = action ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "TransformElement",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["action"] = action ?? "",
                            ["x"] = x ?? 0,
                            ["y"] = y ?? 0,
                            ["width"] = width ?? 0,
                            ["height"] = height ?? 0,
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
                _logger.LogInformationWithOperation(operationId, $"Starting TransformElement: ElementId={elementId}, Action={action}, Coordinates=({x},{y}), Size=({width},{height})");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "action", action },
                    { "x", x ?? 0 },
                    { "y", y ?? 0 },
                    { "width", width ?? 0 },
                    { "height", height ?? 0 },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("TransformElement", parameters, timeoutSeconds);

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
                            ["action"] = action,
                            ["coordinates"] = new { X = x ?? 0, Y = y ?? 0 },
                            ["size"] = new { Width = width ?? 0, Height = height ?? 0 },
                            ["operationType"] = "transformElement"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "TransformElement",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["action"] = action,
                            ["x"] = x ?? 0,
                            ["y"] = y ?? 0,
                            ["width"] = width ?? 0,
                            ["height"] = height ?? 0,
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in TransformElement operation");
                
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
                            ["action"] = action,
                            ["operationType"] = "transformElement"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "TransformElement",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["elementId"] = elementId,
                            ["action"] = action,
                            ["x"] = x ?? 0,
                            ["y"] = y ?? 0,
                            ["width"] = width ?? 0,
                            ["height"] = height ?? 0,
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

        public async Task<object> GetWindowStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetWindowState for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<WindowInfoResult>("GetWindowState", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<WindowInfoResult>
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
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "getWindowState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindowState",
                        RequestParameters = new Dictionary<string, object>
                        {
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetWindowState operation");
                
                var errorResponse = new ServerEnhancedResponse<WindowInfoResult>
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
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "getWindowState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindowState",
                        RequestParameters = new Dictionary<string, object>
                        {
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

        public async Task<object> SetWindowStateAsync(string windowState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(windowState))
            {
                var validationError = "Window state is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"SetWindowState validation failed: {validationError}");
                
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
                            ["windowState"] = windowState ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetWindowState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowState"] = windowState ?? "",
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
                _logger.LogInformationWithOperation(operationId, $"Starting SetWindowState: {windowState} for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "windowState", windowState },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("SetWindowState", parameters, timeoutSeconds);

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
                            ["windowState"] = windowState,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "setWindowState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetWindowState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowState"] = windowState,
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in SetWindowState operation");
                
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
                            ["windowState"] = windowState,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "setWindowState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetWindowState",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowState"] = windowState,
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

        public async Task<object> MoveWindowAsync(int x, int y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting MoveWindow to position: ({x}, {y}) for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "x", x },
                    { "y", y },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("MoveWindow", parameters, timeoutSeconds);

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
                            ["position"] = new { X = x, Y = y },
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "moveWindow"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MoveWindow",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["x"] = x,
                            ["y"] = y,
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in MoveWindow operation");
                
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
                            ["position"] = new { X = x, Y = y },
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "moveWindow"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MoveWindow",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["x"] = x,
                            ["y"] = y,
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

        public async Task<object> ResizeWindowAsync(int width, int height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting ResizeWindow to size: ({width}, {height}) for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "width", width },
                    { "height", height },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("ResizeWindow", parameters, timeoutSeconds);

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
                            ["size"] = new { Width = width, Height = height },
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "resizeWindow"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ResizeWindow",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["width"] = width,
                            ["height"] = height,
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in ResizeWindow operation");
                
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
                            ["size"] = new { Width = width, Height = height },
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "resizeWindow"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ResizeWindow",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["width"] = width,
                            ["height"] = height,
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

        public async Task<object> GetWindowInteractionStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetWindowInteractionState for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<WindowInteractionStateResult>("GetWindowInteractionState", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<WindowInteractionStateResult>
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
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "getWindowInteractionState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindowInteractionState",
                        RequestParameters = new Dictionary<string, object>
                        {
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetWindowInteractionState operation");
                
                var errorResponse = new ServerEnhancedResponse<WindowInteractionStateResult>
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
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "getWindowInteractionState"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindowInteractionState",
                        RequestParameters = new Dictionary<string, object>
                        {
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

        public async Task<object> GetWindowCapabilitiesAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetWindowCapabilities for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<WindowCapabilitiesResult>("GetWindowCapabilities", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<WindowCapabilitiesResult>
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
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "getWindowCapabilities"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindowCapabilities",
                        RequestParameters = new Dictionary<string, object>
                        {
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetWindowCapabilities operation");
                
                var errorResponse = new ServerEnhancedResponse<WindowCapabilitiesResult>
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
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "getWindowCapabilities"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetWindowCapabilities",
                        RequestParameters = new Dictionary<string, object>
                        {
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

        public async Task<object> WaitForInputIdleAsync(int timeoutMilliseconds = 10000, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting WaitForInputIdle for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0}, Timeout: {timeoutMilliseconds}ms)");

                var parameters = new Dictionary<string, object>
                {
                    { "timeoutMilliseconds", timeoutMilliseconds },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<BooleanResult>("WaitForInputIdle", parameters, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<BooleanResult>
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
                            ["timeoutMilliseconds"] = timeoutMilliseconds,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["inputIdleAchieved"] = result.Value,
                            ["operationType"] = "waitForInputIdle"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "WaitForInputIdle",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["timeoutMilliseconds"] = timeoutMilliseconds,
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in WaitForInputIdle operation");
                
                var errorResponse = new ServerEnhancedResponse<BooleanResult>
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
                            ["timeoutMilliseconds"] = timeoutMilliseconds,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationType"] = "waitForInputIdle"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "WaitForInputIdle",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["timeoutMilliseconds"] = timeoutMilliseconds,
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
