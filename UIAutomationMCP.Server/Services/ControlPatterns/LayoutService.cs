using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class LayoutService : ILayoutService
    {
        private readonly ILogger<LayoutService> _logger;
        private readonly SubprocessExecutor _executor;

        public LayoutService(ILogger<LayoutService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ExpandCollapseElementAsync(string? automationId = null, string? name = null, string action = "toggle", string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"ExpandCollapseElement validation failed: {validationError}");
                
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
                        RequestedMethod = "ExpandCollapseElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            if (string.IsNullOrWhiteSpace(action) || !new[] { "expand", "collapse" }.Contains(action.ToLowerInvariant()))
            {
                var validationError = "Action must be either 'expand' or 'collapse'";
                _logger.LogWarningWithOperation(operationId, $"ExpandCollapseElement validation failed: {validationError}");
                
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
                        RequestedMethod = "ExpandCollapseElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Performing expand/collapse action '{action}' on element with AutomationId: {automationId}, Name: {name}");

                var request = new ExpandCollapseElementRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    Action = action,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<ExpandCollapseElementRequest, ActionResult>("ExpandCollapseElement", request, timeoutSeconds);

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
                        RequestedMethod = "ExpandCollapseElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Expand/collapse action performed successfully for AutomationId: {automationId}, Name: {name}");
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
                        RequestedMethod = "ExpandCollapseElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to perform expand/collapse action on element with AutomationId: {automationId}, Name: {name}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ScrollElementAsync(string? automationId = null, string? name = null, string direction = "down", double amount = 1.0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"ScrollElement validation failed: {validationError}");
                
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
                        RequestedMethod = "ScrollElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            if (string.IsNullOrWhiteSpace(direction) || !new[] { "up", "down", "left", "right" }.Contains(direction.ToLowerInvariant()))
            {
                var validationError = "Direction must be one of: up, down, left, right";
                _logger.LogWarningWithOperation(operationId, $"ScrollElement validation failed: {validationError}");
                
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
                        RequestedMethod = "ScrollElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Scrolling element with AutomationId: {automationId}, Name: {name} in direction: {direction} by amount: {amount}");

                var request = new ScrollElementRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    Direction = direction,
                    Amount = amount,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<ScrollElementRequest, ActionResult>("ScrollElement", request, timeoutSeconds);

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
                        RequestedMethod = "ScrollElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Element scrolled successfully with AutomationId: {automationId}, Name: {name}");
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
                        RequestedMethod = "ScrollElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to scroll element with AutomationId: {automationId}, Name: {name}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> ScrollElementIntoViewAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"ScrollElementIntoView validation failed: {validationError}");
                
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
                        RequestedMethod = "ScrollElementIntoView",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Scrolling element into view with AutomationId: {automationId}, Name: {name}");

                var request = new ScrollElementIntoViewRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<ScrollElementIntoViewRequest, ActionResult>("ScrollElementIntoView", request, timeoutSeconds);

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
                        RequestedMethod = "ScrollElementIntoView",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Element scrolled into view successfully with AutomationId: {automationId}, Name: {name}");
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
                        RequestedMethod = "ScrollElementIntoView",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to scroll element into view with AutomationId: {automationId}, Name: {name}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetScrollPercentAsync(string? automationId = null, string? name = null, double horizontalPercent = -1, double verticalPercent = -1, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"SetScrollPercent validation failed: {validationError}");
                
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
                        RequestedMethod = "SetScrollPercent",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            if (horizontalPercent < -1 || horizontalPercent > 100 || verticalPercent < -1 || verticalPercent > 100)
            {
                var validationError = "Scroll percentages must be between -1 and 100 (-1 means no change)";
                _logger.LogWarningWithOperation(operationId, $"SetScrollPercent validation failed: {validationError}");
                
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
                        RequestedMethod = "SetScrollPercent",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Setting scroll percentage for element with AutomationId: {automationId}, Name: {name} to H:{horizontalPercent}%, V:{verticalPercent}%");

                var request = new SetScrollPercentRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    HorizontalPercent = horizontalPercent,
                    VerticalPercent = verticalPercent,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<SetScrollPercentRequest, ActionResult>("SetScrollPercent", request, timeoutSeconds);

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
                        RequestedMethod = "SetScrollPercent",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Scroll percentage set successfully for element with AutomationId: {automationId}, Name: {name}");
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
                        RequestedMethod = "SetScrollPercent",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to set scroll percentage for element with AutomationId: {automationId}, Name: {name}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> DockElementAsync(string? automationId = null, string? name = null, string dockPosition = "none", string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"DockElement validation failed: {validationError}");
                
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
                        RequestedMethod = "DockElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            if (string.IsNullOrWhiteSpace(dockPosition) || !new[] { "top", "bottom", "left", "right", "fill", "none" }.Contains(dockPosition.ToLowerInvariant()))
            {
                var validationError = "Dock position must be one of: top, bottom, left, right, fill, none";
                _logger.LogWarningWithOperation(operationId, $"DockElement validation failed: {validationError}");
                
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
                        RequestedMethod = "DockElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Docking element with AutomationId: {automationId}, Name: {name} to position: {dockPosition}");

                var request = new DockElementRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    DockPosition = dockPosition,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<DockElementRequest, ActionResult>("DockElement", request, timeoutSeconds);

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
                        RequestedMethod = "DockElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Element docked successfully with AutomationId: {automationId}, Name: {name}");
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
                        RequestedMethod = "DockElement",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to dock element with AutomationId: {automationId}, Name: {name}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ScrollInfoResult>> GetScrollInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetScrollInfo validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ScrollInfoResult>
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
                        RequestedMethod = "GetScrollInfo",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting scroll info for element with AutomationId: {automationId}, Name: {name}");

                var request = new GetScrollInfoRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetScrollInfoRequest, ScrollInfoResult>("GetScrollInfo", request, timeoutSeconds);

                var response = new ServerEnhancedResponse<ScrollInfoResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetScrollInfo",
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully retrieved scroll info for element with AutomationId: {automationId}, Name: {name}");
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<ScrollInfoResult>
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
                        RequestedMethod = "GetScrollInfo",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get scroll info for element with AutomationId: {automationId}, Name: {name}");
                return errorResponse;
            }
        }
    }
}