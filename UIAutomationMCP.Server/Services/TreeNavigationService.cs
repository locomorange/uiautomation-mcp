using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public class TreeNavigationService : ITreeNavigationService
    {
        private readonly ILogger<TreeNavigationService> _logger;
        private readonly SubprocessExecutor _executor;

        public TreeNavigationService(
            ILogger<TreeNavigationService> logger,
            SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetChildrenAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetChildren validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetChildren",
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
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting children for element: {elementId}");

                var request = new GetChildrenRequest
                {
                    AutomationId = elementId,
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetChildrenRequest, TreeNavigationResult>("GetChildren", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetChildren",
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
                
                _logger.LogInformationWithOperation(operationId, $"Children retrieved successfully for element: {elementId}");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetChildren",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get children for element: {elementId}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetParentAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetParent validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetParent",
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
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting parent for element: {elementId}");

                var request = new GetParentRequest
                {
                    AutomationId = elementId,
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetParentRequest, TreeNavigationResult>("GetParent", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetParent",
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
                
                _logger.LogInformationWithOperation(operationId, $"Parent retrieved successfully for element: {elementId}");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetParent",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get parent for element: {elementId}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetSiblingsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetSiblings validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetSiblings",
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
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting siblings for element: {elementId}");

                var request = new GetSiblingsRequest
                {
                    AutomationId = elementId,
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetSiblingsRequest, TreeNavigationResult>("GetSiblings", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetSiblings",
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
                
                _logger.LogInformationWithOperation(operationId, $"Siblings retrieved successfully for element: {elementId}");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetSiblings",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get siblings for element: {elementId}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetDescendantsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetDescendants validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetDescendants",
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
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting descendants for element: {elementId}");

                var request = new GetDescendantsRequest
                {
                    AutomationId = elementId,
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetDescendantsRequest, TreeNavigationResult>("GetDescendants", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetDescendants",
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
                
                _logger.LogInformationWithOperation(operationId, $"Descendants retrieved successfully for element: {elementId}");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetDescendants",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get descendants for element: {elementId}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<TreeNavigationResult>> GetAncestorsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetAncestors validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetAncestors",
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
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting ancestors for element: {elementId}");

                var request = new GetAncestorsRequest
                {
                    AutomationId = elementId,
                    WindowTitle = windowTitle ?? "",
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<GetAncestorsRequest, TreeNavigationResult>("GetAncestors", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetAncestors",
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
                
                _logger.LogInformationWithOperation(operationId, $"Ancestors retrieved successfully for element: {elementId}");
                return successResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<TreeNavigationResult>
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
                        RequestedMethod = "GetAncestors",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get ancestors for element: {elementId}");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementTreeResult>> GetElementTreeAsync(string? windowTitle = null, int? processId = null, int maxDepth = 3, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetElementTree operation with WindowTitle={windowTitle}, ProcessId={processId}, MaxDepth={maxDepth}");

                var request = new GetElementTreeRequest
                {
                    WindowTitle = windowTitle,
                    ProcessId = processId,
                    MaxDepth = maxDepth
                };

                _logger.LogInformationWithOperation(operationId, "Calling worker process for GetElementTree");
                var workerResult = await _executor.ExecuteAsync<GetElementTreeRequest, ElementTreeResult>("GetElementTree", request, timeoutSeconds);
                
                stopwatch.Stop();
                _logger.LogInformationWithOperation(operationId, $"Worker completed successfully in {stopwatch.Elapsed.TotalMilliseconds:F2}ms");

                var serverResponse = new ServerEnhancedResponse<ElementTreeResult>
                {
                    Success = workerResult.Success,
                    Data = workerResult,
                    ErrorMessage = workerResult.ErrorMessage,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["workerProcessingTime"] = workerResult.ExecutionTime?.ToString() ?? "Unknown",
                            ["totalElements"] = workerResult.TotalElements,
                            ["maxDepthReached"] = workerResult.MaxDepth,
                            ["buildDuration"] = workerResult.BuildDuration.ToString(),
                            ["includeInvisible"] = workerResult.IncludeInvisible,
                            ["includeOffscreen"] = workerResult.IncludeOffscreen
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetElementTree",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["maxDepth"] = maxDepth
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, "Successfully created enhanced response");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in GetElementTree operation");
                
                var errorResponse = new ServerEnhancedResponse<ElementTreeResult>
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
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetElementTree",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["maxDepth"] = maxDepth
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorResponse;
            }
        }


        private object? ConvertTreeNodeToDict(TreeNode? node)
        {
            if (node == null) return null;
            
            // Flatten to simple structure avoiding nested complex objects
            var simple = new Dictionary<string, object>
            {
                ["automationId"] = node.AutomationId ?? "",
                ["name"] = node.Name ?? "",
                ["className"] = node.ClassName ?? "",
                ["controlType"] = node.ControlType ?? "",
                ["isEnabled"] = node.IsEnabled,
                ["isKeyboardFocusable"] = node.IsKeyboardFocusable,
                ["hasKeyboardFocus"] = node.HasKeyboardFocus,
                ["isPassword"] = node.IsPassword,
                ["isVisible"] = node.IsVisible,
                ["boundingX"] = node.BoundingRectangle.X,
                ["boundingY"] = node.BoundingRectangle.Y,
                ["boundingWidth"] = node.BoundingRectangle.Width,
                ["boundingHeight"] = node.BoundingRectangle.Height,
                ["processId"] = node.ProcessId,
                ["runtimeId"] = node.RuntimeId ?? "",
                ["supportedPatterns"] = string.Join(",", node.SupportedPatterns),
                ["depth"] = node.Depth,
                ["isExpanded"] = node.IsExpanded,
                ["hasChildren"] = node.HasChildren,
                ["parentAutomationId"] = node.ParentAutomationId ?? "",
                ["parentName"] = node.ParentName ?? "",
                ["childrenCount"] = node.Children.Count
            };
            
            // Skip children arrays entirely for MCP compatibility
            
            return simple;
        }

        private Dictionary<string, object> ConvertTreeElementToDict(TreeElement element)
        {
            return new Dictionary<string, object>
            {
                ["automationId"] = element.AutomationId ?? "",
                ["parentAutomationId"] = element.ParentAutomationId ?? "",
                ["parentName"] = element.ParentName ?? "",
                ["name"] = element.Name ?? "",
                ["className"] = element.ClassName ?? "",
                ["controlType"] = element.ControlType ?? "",
                ["isEnabled"] = element.IsEnabled,
                ["isKeyboardFocusable"] = element.IsKeyboardFocusable,
                ["hasKeyboardFocus"] = element.HasKeyboardFocus,
                ["isPassword"] = element.IsPassword,
                ["isVisible"] = element.IsVisible,
                ["boundingX"] = element.BoundingRectangle.X,
                ["boundingY"] = element.BoundingRectangle.Y,
                ["boundingWidth"] = element.BoundingRectangle.Width,
                ["boundingHeight"] = element.BoundingRectangle.Height,
                ["supportedPatterns"] = string.Join(",", element.SupportedPatterns),
                ["processId"] = element.ProcessId,
                ["runtimeId"] = element.RuntimeId ?? "",
                ["nativeWindowHandle"] = element.NativeWindowHandle,
                ["isControlElement"] = element.IsControlElement,
                ["isContentElement"] = element.IsContentElement,
                ["hasChildren"] = element.HasChildren,
                ["childCount"] = element.ChildCount
            };
        }
    }
}
