using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ItemContainerService : IItemContainerService
    {
        private readonly ILogger<ItemContainerService> _logger;
        private readonly ISubprocessExecutor _executor;

        public ItemContainerService(ILogger<ItemContainerService> logger, ISubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindItemByPropertyAsync(string? automationId = null, string? name = null, string? propertyName = null, string? value = null, string? startAfterId = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(automationId) && string.IsNullOrWhiteSpace(name))
            {
                var validationError = "Either AutomationId or Name is required for container identification";
                _logger.LogWarningWithOperation(operationId, $"FindItemByProperty validation failed: {validationError}");
                
                var validationResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = _executor is SubprocessExecutor executor ? LogCollectorExtensions.Instance.GetLogs(operationId) : new List<string>(),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindItemByProperty",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                return validationResponse;
            }
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Finding item in container: AutomationId={automationId}, Name={name}, ControlType={controlType} with property: {propertyName}={value}");

                var request = new FindItemByPropertyRequest
                {
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    PropertyName = propertyName ?? "",
                    Value = value ?? "",
                    StartAfterId = startAfterId ?? "",
                    ProcessId = processId ?? 0
                };

                var result = await _executor.ExecuteAsync<FindItemByPropertyRequest, ElementSearchResult>("FindItemByProperty", request, timeoutSeconds);

                var successResponse = new ServerEnhancedResponse<ElementSearchResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = _executor is SubprocessExecutor executor ? LogCollectorExtensions.Instance.GetLogs(operationId) : new List<string>()
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindItemByProperty",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Item search completed in container: AutomationId={automationId}, Name={name}");
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
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindItemByProperty",
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to find item in container: AutomationId={automationId}, Name={name}");
                return errorResponse;
            }
        }
    }
}