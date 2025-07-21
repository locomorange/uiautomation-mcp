using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class RangeService : IRangeService
    {
        private readonly ILogger<RangeService> _logger;
        private readonly SubprocessExecutor _executor;

        public RangeService(ILogger<RangeService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<ActionResult>> SetRangeValueAsync(string? automationId = null, string? name = null, double value = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"SetRangeValue validation failed: {validationError}");
                
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
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetRangeValue",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["value"] = value,
                            ["controlType"] = controlType ?? "",
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
                _logger.LogInformationWithOperation(operationId, $"Starting SetRangeValue: AutomationId={automationId}, Name={name}, Value={value}");

                var request = new SetRangeValueRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    Value = value,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<SetRangeValueRequest, ActionResult>("SetRangeValue", request, timeoutSeconds);

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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["value"] = value,
                            ["operationType"] = "setRangeValue"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetRangeValue",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["value"] = value,
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully set range value for element (AutomationId: {automationId}, Name: {name})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in SetRangeValue operation");
                
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
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["value"] = value,
                            ["operationType"] = "setRangeValue"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "SetRangeValue",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["value"] = value,
                            ["controlType"] = controlType ?? "",
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

        public async Task<ServerEnhancedResponse<RangeValueResult>> GetRangeValueAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required";
                _logger.LogWarningWithOperation(operationId, $"GetRangeValue validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<RangeValueResult>
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
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRangeValue",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
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
                _logger.LogInformationWithOperation(operationId, $"Getting range value for element (AutomationId: {automationId}, Name: {name})");

                var request = new GetRangeValueRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<GetRangeValueRequest, RangeValueResult>("GetRangeValue", request, timeoutSeconds);

                var response = new ServerEnhancedResponse<RangeValueResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["operationCompleted"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRangeValue",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                _logger.LogInformationWithOperation(operationId, $"Successfully retrieved range value for element (AutomationId: {automationId}, Name: {name})");
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = new ServerEnhancedResponse<RangeValueResult>
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
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetRangeValue",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["automationId"] = automationId ?? "",
                            ["name"] = name ?? "",
                            ["controlType"] = controlType ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to get range value for element (AutomationId: {automationId}, Name: {name})");
                return errorResponse;
            }
        }

    }
}
