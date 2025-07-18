using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
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

        public async Task<ServerEnhancedResponse<ElementSearchResult>> FindItemByPropertyAsync(string containerId, string? propertyName = null, string? value = null, string? startAfterId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Input validation
            if (string.IsNullOrWhiteSpace(containerId))
            {
                var validationError = "Container ID is required and cannot be empty";
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
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["errorCategory"] = "Validation",
                            ["containerId"] = containerId ?? "<null>",
                            ["validationFailed"] = true
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "FindItemByProperty",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["containerId"] = containerId ?? "",
                            ["propertyName"] = propertyName ?? "",
                            ["value"] = value ?? "",
                            ["startAfterId"] = startAfterId ?? "",
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
                _logger.LogInformationWithOperation(operationId, $"Finding item in container: {containerId} with property: {propertyName}={value}");

                var parameters = new Dictionary<string, object>
                {
                    { "containerId", containerId },
                    { "propertyName", propertyName ?? "" },
                    { "value", value ?? "" },
                    { "startAfterId", startAfterId ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ElementSearchResult>("FindItemByProperty", parameters, timeoutSeconds);

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
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["containerId"] = containerId,
                            ["propertyName"] = propertyName ?? "",
                            ["value"] = value ?? "",
                            ["startAfterId"] = startAfterId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogInformationWithOperation(operationId, $"Item search completed in container: {containerId}");
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
                        RequestedMethod = "FindItemByProperty",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["containerId"] = containerId,
                            ["propertyName"] = propertyName ?? "",
                            ["value"] = value ?? "",
                            ["startAfterId"] = startAfterId ?? "",
                            ["windowTitle"] = windowTitle ?? "",
                            ["processId"] = processId ?? 0,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                _logger.LogErrorWithOperation(operationId, ex, $"Failed to find item in container: {containerId}");
                return errorResponse;
            }
        }
    }
}