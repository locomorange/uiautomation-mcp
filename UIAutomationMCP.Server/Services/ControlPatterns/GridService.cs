using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class GridService : IGridService
    {
        private readonly ILogger<GridService> _logger;
        private readonly SubprocessExecutor _executor;

        public GridService(ILogger<GridService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetGridInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(elementId))
            {
                var validationError = "Element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetGridInfo validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<GridInfoResult>
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
                        RequestedMethod = "GetGridInfo",
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
                _logger.LogInformationWithOperation(operationId, $"Getting grid information for element: {elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<GridInfoResult>("GetGridInfo", parameters, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<GridInfoResult>
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
                        RequestedMethod = "GetGridInfo",
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
                
                _logger.LogInformationWithOperation(operationId, $"Grid information retrieved successfully for element: {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<GridInfoResult>
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
                        RequestedMethod = "GetGridInfo",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get grid information for element {elementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }

        public async Task<object> GetGridItemAsync(string gridElementId, int row, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(gridElementId))
            {
                var validationError = "Grid element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetGridItem validation failed: {validationError}");
                
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
                            ["gridElementId"] = gridElementId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetGridItem",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId ?? "",
                            ["row"] = row,
                            ["column"] = column,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
            }
            
            if (row < 0 || column < 0)
            {
                var validationError = "Row and column indices must be non-negative";
                _logger.LogWarningWithOperation(operationId, $"GetGridItem validation failed: {validationError}");
                
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
                            ["row"] = row,
                            ["column"] = column,
                            ["validationFailed"] = true,
                            ["requirement"] = "Non-negative indices"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetGridItem",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["row"] = row,
                            ["column"] = column,
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
                _logger.LogInformationWithOperation(operationId, $"Getting grid item at row {row}, column {column} for element: {gridElementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "gridElementId", gridElementId },
                    { "row", row },
                    { "column", column },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("GetGridItem", parameters, timeoutSeconds);

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
                        RequestedMethod = "GetGridItem",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["row"] = row,
                            ["column"] = column,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Grid item retrieved successfully for element: {gridElementId}");
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
                        RequestedMethod = "GetGridItem",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["row"] = row,
                            ["column"] = column,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get grid item at row {row}, column {column} for element {gridElementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }

        public async Task<object> GetRowHeaderAsync(string gridElementId, int row, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(gridElementId))
            {
                var validationError = "Grid element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetRowHeader validation failed: {validationError}");
                
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
                            ["gridElementId"] = gridElementId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRowHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId ?? "",
                            ["row"] = row,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
            }
            
            if (row < 0)
            {
                var validationError = "Row index must be non-negative";
                _logger.LogWarningWithOperation(operationId, $"GetRowHeader validation failed: {validationError}");
                
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
                            ["row"] = row,
                            ["validationFailed"] = true,
                            ["requirement"] = "Non-negative row index"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRowHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["row"] = row,
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
                _logger.LogInformationWithOperation(operationId, $"Getting row header for row {row} in element: {gridElementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "gridElementId", gridElementId },
                    { "row", row },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("GetRowHeader", parameters, timeoutSeconds);

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
                        RequestedMethod = "GetRowHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["row"] = row,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Row header retrieved successfully for element: {gridElementId}");
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
                        RequestedMethod = "GetRowHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["row"] = row,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get row header for row {row} in element {gridElementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }

        public async Task<object> GetColumnHeaderAsync(string gridElementId, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(gridElementId))
            {
                var validationError = "Grid element ID is required and cannot be empty";
                _logger.LogWarningWithOperation(operationId, $"GetColumnHeader validation failed: {validationError}");
                
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
                            ["gridElementId"] = gridElementId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetColumnHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId ?? "",
                            ["column"] = column,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(validationResponse);
            }
            
            if (column < 0)
            {
                var validationError = "Column index must be non-negative";
                _logger.LogWarningWithOperation(operationId, $"GetColumnHeader validation failed: {validationError}");
                
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
                            ["column"] = column,
                            ["validationFailed"] = true,
                            ["requirement"] = "Non-negative column index"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetColumnHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["column"] = column,
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
                _logger.LogInformationWithOperation(operationId, $"Getting column header for column {column} in element: {gridElementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "gridElementId", gridElementId },
                    { "column", column },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("GetColumnHeader", parameters, timeoutSeconds);

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
                        RequestedMethod = "GetColumnHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["column"] = column,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Column header retrieved successfully for element: {gridElementId}");
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
                        RequestedMethod = "GetColumnHeader",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["gridElementId"] = gridElementId,
                            ["column"] = column,
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get column header for column {column} in element {gridElementId}");
                return UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
            }
        }
    }
}