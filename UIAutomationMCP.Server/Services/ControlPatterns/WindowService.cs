using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
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

        public async Task<ServerEnhancedResponse<ActionResult>> WindowOperationAsync(string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting WindowOperation: {operation} on window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var request = new WindowActionRequest
                {
                    Action = operation,
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<WindowActionRequest, ActionResult>("WindowAction", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting TransformElement: ElementId={elementId}, Action={action}, Coordinates=({x},{y}), Size=({width},{height})");

                var request = new TransformElementRequest
                {
                    AutomationId = elementId,
                    Action = action,
                    X = x ?? 0,
                    Y = y ?? 0,
                    Width = width ?? 0,
                    Height = height ?? 0,
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<TransformElementRequest, ActionResult>("TransformElement", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }


        public async Task<ServerEnhancedResponse<ActionResult>> SetWindowStateAsync(string windowState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting SetWindowState: {windowState} for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var request = new SetWindowStateRequest
                {
                    WindowState = windowState,
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<SetWindowStateRequest, ActionResult>("SetWindowState", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> MoveWindowAsync(int x, int y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting MoveWindow to position: ({x}, {y}) for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var request = new MoveWindowRequest
                {
                    X = x,
                    Y = y,
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<MoveWindowRequest, ActionResult>("MoveWindow", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ResizeWindowAsync(int width, int height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting ResizeWindow to size: ({width}, {height}) for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var request = new ResizeWindowRequest
                {
                    Width = width,
                    Height = height,
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<ResizeWindowRequest, ActionResult>("ResizeWindow", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }



        public async Task<ServerEnhancedResponse<BooleanResult>> WaitForInputIdleAsync(int timeoutMilliseconds = 10000, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting WaitForInputIdle for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0}, Timeout: {timeoutMilliseconds}ms)");

                var request = new WaitForInputIdleRequest
                {
                    TimeoutMilliseconds = timeoutMilliseconds,
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<WaitForInputIdleRequest, BooleanResult>("WaitForInputIdle", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<WindowInteractionStateResult>> GetWindowInteractionStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetWindowInteractionState for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var request = new GetWindowInfoRequest
                {
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetWindowInfoRequest, WindowInteractionStateResult>("GetWindowInteractionState", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<WindowCapabilitiesResult>> GetWindowCapabilitiesAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetWindowCapabilities for window: {windowTitle ?? "any"} (ProcessId: {processId ?? 0})");

                var request = new GetWindowInfoRequest
                {
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetWindowInfoRequest, WindowCapabilitiesResult>("GetWindowCapabilities", request, timeoutSeconds);

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

                _logger.LogInformationWithOperation(operationId, $"Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
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
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }
    }
}
