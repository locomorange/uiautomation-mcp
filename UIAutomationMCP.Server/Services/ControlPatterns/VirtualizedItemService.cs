using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class VirtualizedItemService : IVirtualizedItemService
    {
        private readonly ILogger<VirtualizedItemService> _logger;
        private readonly ISubprocessExecutor _executor;

        public VirtualizedItemService(ILogger<VirtualizedItemService> logger, ISubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> RealizeItemAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required for element identification";
                _logger.LogWarningWithOperation(operationId, $"RealizeItem validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = _executor is SubprocessExecutor executor ? LogCollectorExtensions.Instance.GetLogs(operationId) : new List<string>(),
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
                        RequestedMethod = "RealizeItem",
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
                _logger.LogInformationWithOperation(operationId, $"Realizing virtualized item: AutomationId={automationId}, Name={name}, ControlType={controlType}");

                var request = new RealizeVirtualizedItemRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<RealizeVirtualizedItemRequest, ElementSearchResult>("RealizeVirtualizedItem", request, timeoutSeconds);

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
                        RequestedMethod = "RealizeItem",
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
                
                _logger.LogInformationWithOperation(operationId, $"Virtualized item realized successfully: AutomationId={automationId}, Name={name}");
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
                        ServerLogs = _executor is SubprocessExecutor executor ? LogCollectorExtensions.Instance.GetLogs(operationId) : new List<string>(),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "ExecutionError",
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "RealizeItem",
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
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to realize virtualized item: AutomationId={automationId}, Name={name}");
                return errorResponse;
            }
        }
    }
}