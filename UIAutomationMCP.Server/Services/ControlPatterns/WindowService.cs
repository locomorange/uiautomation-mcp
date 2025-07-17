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
                
                var validationJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.SerializeObject(validationResponse);
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

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.SerializeObject(serverResponse);
                
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
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.SerializeObject(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }

        public async Task<object> TransformElementAsync(string elementId, string action, double? x = null, double? y = null, double? width = null, double? height = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            var validationResult = SubprocessErrorHandler.ValidateElementId(elementId, "TransformElement", _logger);
            if (validationResult != null) return validationResult;

            if (string.IsNullOrWhiteSpace(action))
            {
                var validationError = "Transform action is required and cannot be empty";
                _logger.LogWarning("TransformElement operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Transforming element: {ElementId} with action: {Action} (x:{X}, y:{Y}, w:{Width}, h:{Height})", 
                    elementId, action, x ?? 0, y ?? 0, width ?? 0, height ?? 0);

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

                await _executor.ExecuteAsync<object>("TransformElement", parameters, timeoutSeconds);

                _logger.LogInformation("Element transformed successfully: {ElementId}", elementId);
                return new { 
                    Success = true, 
                    Message = $"Element '{elementId}' transformed with action '{action}' successfully",
                    ElementId = elementId,
                    Action = action,
                    Coordinates = new { X = x ?? 0, Y = y ?? 0, Width = width ?? 0, Height = height ?? 0 },
                    Operation = "TransformElement"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "TransformElement", elementId, timeoutSeconds, _logger);
            }
        }

        public async Task<object> GetWindowStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting window state for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetWindowState", parameters, timeoutSeconds);

                _logger.LogInformation("Window state retrieved successfully for window: {WindowTitle}", windowTitle ?? "any");
                return new { 
                    Success = true, 
                    Data = result,
                    Message = $"Window state retrieved successfully",
                    WindowTitle = windowTitle,
                    Operation = "GetWindowState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetWindowState", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> SetWindowStateAsync(string windowState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(windowState))
            {
                var validationError = "Window state is required and cannot be empty";
                _logger.LogWarning("SetWindowState operation failed due to validation: {Error}", validationError);
                return new { Success = false, Error = validationError, ErrorCategory = "Validation" };
            }

            try
            {
                _logger.LogInformation("Setting window state to: {WindowState} for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    windowState, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "windowState", windowState },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("SetWindowState", parameters, timeoutSeconds);

                _logger.LogInformation("Window state set successfully to: {WindowState}", windowState);
                return new { 
                    Success = true, 
                    Message = $"Window state set to '{windowState}' successfully",
                    WindowState = windowState,
                    WindowTitle = windowTitle,
                    Operation = "SetWindowState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "SetWindowState", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> MoveWindowAsync(int x, int y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Moving window to position: ({X}, {Y}) for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    x, y, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "x", x },
                    { "y", y },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("MoveWindow", parameters, timeoutSeconds);

                _logger.LogInformation("Window moved successfully to position: ({X}, {Y})", x, y);
                return new { 
                    Success = true, 
                    Message = $"Window moved to position ({x}, {y}) successfully",
                    Position = new { X = x, Y = y },
                    WindowTitle = windowTitle,
                    Operation = "MoveWindow"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "MoveWindow", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> ResizeWindowAsync(int width, int height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Resizing window to size: ({Width}, {Height}) for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    width, height, windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "width", width },
                    { "height", height },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                await _executor.ExecuteAsync<object>("ResizeWindow", parameters, timeoutSeconds);

                _logger.LogInformation("Window resized successfully to size: ({Width}, {Height})", width, height);
                return new { 
                    Success = true, 
                    Message = $"Window resized to size ({width}, {height}) successfully",
                    Size = new { Width = width, Height = height },
                    WindowTitle = windowTitle,
                    Operation = "ResizeWindow"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "ResizeWindow", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> GetWindowInteractionStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting window interaction state for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetWindowInteractionState", parameters, timeoutSeconds);

                _logger.LogInformation("Window interaction state retrieved successfully for window: {WindowTitle}", windowTitle ?? "any");
                return new { 
                    Success = true, 
                    Data = result,
                    Message = "Window interaction state retrieved successfully",
                    WindowTitle = windowTitle,
                    Operation = "GetWindowInteractionState"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetWindowInteractionState", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> GetWindowCapabilitiesAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting window capabilities for window: {WindowTitle} (ProcessId: {ProcessId})", 
                    windowTitle ?? "any", processId ?? 0);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetWindowCapabilities", parameters, timeoutSeconds);

                _logger.LogInformation("Window capabilities retrieved successfully for window: {WindowTitle}", windowTitle ?? "any");
                return new { 
                    Success = true, 
                    Data = result,
                    Message = "Window capabilities retrieved successfully",
                    WindowTitle = windowTitle,
                    Operation = "GetWindowCapabilities"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "GetWindowCapabilities", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }

        public async Task<object> WaitForInputIdleAsync(int timeoutMilliseconds = 10000, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Waiting for input idle for window: {WindowTitle} (ProcessId: {ProcessId}, Timeout: {TimeoutMs}ms)", 
                    windowTitle ?? "any", processId ?? 0, timeoutMilliseconds);

                var parameters = new Dictionary<string, object>
                {
                    { "timeoutMilliseconds", timeoutMilliseconds },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("WaitForInputIdle", parameters, timeoutSeconds);

                _logger.LogInformation("Wait for input idle completed for window: {WindowTitle}", windowTitle ?? "any");
                return new { 
                    Success = true, 
                    Data = result,
                    Message = "Wait for input idle completed",
                    WindowTitle = windowTitle,
                    Operation = "WaitForInputIdle"
                };
            }
            catch (Exception ex)
            {
                return SubprocessErrorHandler.HandleError(ex, "WaitForInputIdle", windowTitle ?? "unknown", timeoutSeconds, _logger);
            }
        }
    }
}
