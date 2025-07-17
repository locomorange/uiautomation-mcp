using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class InvokeService : IInvokeService
    {
        private readonly ILogger<InvokeService> _logger;
        private readonly SubprocessExecutor _executor;

        public InvokeService(ILogger<InvokeService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> InvokeElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting InvokeElement for ElementId={elementId}");

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ActionResult>("InvokeElement", parameters, timeoutSeconds);

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
                            ["elementId"] = elementId,
                            ["operationType"] = "invoke",
                            ["actionPerformed"] = "elementInvoked"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "InvokeElement",
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

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in InvokeElement operation");
                
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
                            ["elementId"] = elementId,
                            ["operationType"] = "invoke",
                            ["actionPerformed"] = "elementInvoked"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "InvokeElement",
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
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }
    }
}
