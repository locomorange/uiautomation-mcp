using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public class ControlTypeService : IControlTypeService
    {
        private readonly ILogger<ControlTypeService> _logger;
        private readonly SubprocessExecutor _executor;

        public ControlTypeService(ILogger<ControlTypeService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }


        public async Task<ServerEnhancedResponse<ElementSearchResult>> ValidateControlTypePatternsAsync(
            string? automationId = null, 
            string? name = null,
            string? controlType = null,
            int? processId = null, 
            int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required for element identification";
                _logger.LogWarningWithOperation(operationId, $"ValidateControlTypePatterns validation failed: {validationError}");
                
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
                            ["automationId"] = automationId ?? "<null>",
                            ["name"] = name ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ValidateControlTypePatterns",
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
                _logger.LogInformationWithOperation(operationId, $"Validating control type patterns for AutomationId={automationId}, Name={name}, ControlType={controlType}");

                var request = new ValidateControlTypePatternsRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<ValidateControlTypePatternsRequest, ElementSearchResult>("ValidateControlTypePatterns", request, timeoutSeconds);

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
                        RequestedMethod = "ValidateControlTypePatterns",
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
                
                _logger.LogInformationWithOperation(operationId, $"Control type patterns validated successfully for AutomationId={automationId}, Name={name}, ControlType={controlType}");
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
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "ExecutionError",
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "ValidateControlTypePatterns",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to validate control type patterns for AutomationId={automationId}, Name={name}, ControlType={controlType}");
                return errorResponse;
            }
        }

    }
}