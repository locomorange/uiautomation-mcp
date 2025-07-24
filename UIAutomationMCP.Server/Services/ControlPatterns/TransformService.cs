using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TransformService : ITransformService
    {
        private readonly ILogger<TransformService> _logger;
        private readonly SubprocessExecutor _executor;

        public TransformService(ILogger<TransformService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<ActionResult>> MoveElementAsync(string? automationId = null, string? name = null, double x = 0, double y = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"MoveElement validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MoveElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Moving element with AutomationId: {automationId}, Name: {name} to position: ({x}, {y})");

                var request = new MoveElementRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    X = x,
                    Y = y,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<MoveElementRequest, ActionResult>("MoveElement", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ActionResult>
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
                        RequestedMethod = "MoveElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Element moved successfully with AutomationId: {automationId}, Name: {name} to ({x}, {y})");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MoveElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to move element with AutomationId: {automationId}, Name: {name} to ({x}, {y})");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ResizeElementAsync(string? automationId = null, string? name = null, double width = 100, double height = 100, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"ResizeElement validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ResizeElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            if (width <= 0 || height <= 0)
            {
                var validationError = "Width and height must be positive values";
                _logger.LogWarningWithOperation(operationId, $"ResizeElement validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ResizeElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Resizing element with AutomationId: {automationId}, Name: {name} to size: ({width}, {height})");

                var request = new ResizeElementRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    Width = width,
                    Height = height,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<ResizeElementRequest, ActionResult>("ResizeElement", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ActionResult>
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
                        RequestedMethod = "ResizeElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Element resized successfully with AutomationId: {automationId}, Name: {name} to ({width}, {height})");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ResizeElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to resize element with AutomationId: {automationId}, Name: {name} to ({width}, {height})");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> RotateElementAsync(string? automationId = null, string? name = null, double degrees = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"RotateElement validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "RotateElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Rotating element with AutomationId: {automationId}, Name: {name} by {degrees} degrees");

                var request = new RotateElementRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    Degrees = degrees,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<RotateElementRequest, ActionResult>("RotateElement", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ActionResult>
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
                        RequestedMethod = "RotateElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Element rotated successfully with AutomationId: {automationId}, Name: {name} by {degrees} degrees");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ActionResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "RotateElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to rotate element with AutomationId: {automationId}, Name: {name} by {degrees} degrees");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<TransformCapabilitiesResult>> GetTransformCapabilitiesAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetTransformCapabilities validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TransformCapabilitiesResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetTransformCapabilities",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                return validationResponse;
            }

            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetTransformCapabilities for AutomationId={automationId}, Name={name}");

                var request = new GetTransformCapabilitiesRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetTransformCapabilitiesRequest, TransformCapabilitiesResult>("GetTransformCapabilities", request, timeoutSeconds);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<TransformCapabilitiesResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetTransformCapabilities",
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
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetTransformCapabilities operation");
                
                var errorResponse = new ServerEnhancedResponse<TransformCapabilitiesResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetTransformCapabilities",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }
    }
}