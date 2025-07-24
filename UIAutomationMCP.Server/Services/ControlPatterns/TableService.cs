using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TableService : ITableService
    {
        private readonly ILogger<TableService> _logger;
        private readonly ISubprocessExecutor _executor;

        public TableService(ILogger<TableService> logger, ISubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }


        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeadersAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetRowHeaders validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
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
                        RequestedMethod = "GetRowHeaders",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting row headers for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetRowHeadersRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetRowHeadersRequest, ElementSearchResult>("GetRowHeaders", request, timeoutSeconds);

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
                        RequestedMethod = "GetRowHeaders",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Row headers retrieved successfully for element (AutomationId: {automationId}, Name: {name})");
                return successResponse;
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
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRowHeaders",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get row headers for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeadersAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetColumnHeaders validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
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
                        RequestedMethod = "GetColumnHeaders",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting column headers for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetColumnHeadersRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetColumnHeadersRequest, ElementSearchResult>("GetColumnHeaders", request, timeoutSeconds);

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
                        RequestedMethod = "GetColumnHeaders",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Column headers retrieved successfully for element (AutomationId: {automationId}, Name: {name})");
                return successResponse;
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
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetColumnHeaders",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get column headers for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }


        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetColumnHeaderItemsAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetColumnHeaderItems validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
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
                        RequestedMethod = "GetColumnHeaderItems",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting column header items for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetColumnHeaderItemsRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetColumnHeaderItemsRequest, ElementSearchResult>("GetColumnHeaderItems", request, timeoutSeconds);

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
                        RequestedMethod = "GetColumnHeaderItems",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Column header items retrieved successfully for element (AutomationId: {automationId}, Name: {name})");
                return successResponse;
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
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetColumnHeaderItems",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get column header items for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> GetRowHeaderItemsAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetRowHeaderItems validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
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
                        RequestedMethod = "GetRowHeaderItems",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting row header items for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetRowHeaderItemsRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetRowHeaderItemsRequest, ElementSearchResult>("GetRowHeaderItems", request, timeoutSeconds);

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
                        RequestedMethod = "GetRowHeaderItems",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Row header items retrieved successfully for element (AutomationId: {automationId}, Name: {name})");
                return successResponse;
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
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRowHeaderItems",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get row header items for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<ActionResult>> GetRowOrColumnMajorAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetRowOrColumnMajor validation failed: {validationError}");
                
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
                        RequestedMethod = "GetRowOrColumnMajor",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting row or column major for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetTableInfoRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetTableInfoRequest, ActionResult>("GetRowOrColumnMajor", request, timeoutSeconds);

                var response = new ServerEnhancedResponse<ActionResult>
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
                        RequestedMethod = "GetRowOrColumnMajor",
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully retrieved row or column major for element (AutomationId: {automationId}, Name: {name})");
                return response;
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
                        RequestedMethod = "GetRowOrColumnMajor",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get row or column major for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }

        public async Task<ServerEnhancedResponse<TableInfoResult>> GetTableInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetTableInfo validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<TableInfoResult>
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
                        RequestedMethod = "GetTableInfo",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Getting table info for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetTableInfoRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetTableInfoRequest, TableInfoResult>("GetTableInfo", request, timeoutSeconds);

                var response = new ServerEnhancedResponse<TableInfoResult>
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
                        RequestedMethod = "GetTableInfo",
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully retrieved table info for element (AutomationId: {automationId}, Name: {name})");
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<TableInfoResult>
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
                        RequestedMethod = "GetTableInfo",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get table info for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }
    }
}