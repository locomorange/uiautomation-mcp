using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public class EventMonitorService : IEventMonitorService
    {
        private readonly ILogger<EventMonitorService> _logger;
        private readonly SubprocessExecutor _executor;

        public EventMonitorService(ILogger<EventMonitorService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ServerEnhancedResponse<EventMonitoringResult>> MonitorEventsAsync(string eventType, int duration, string? elementId = null, string? windowTitle = null, int? processId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting MonitorEvents for EventType={eventType}, Duration={duration}");

                var parameters = new Dictionary<string, object>
                {
                    { "eventType", eventType },
                    { "duration", duration },
                    { "elementId", elementId ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<EventMonitoringResult>("MonitorEvents", parameters, duration + 30); // Add buffer to timeout

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<EventMonitoringResult>
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
                            { "eventType", eventType },
                            { "duration", duration },
                            { "elementId", elementId ?? "All" },
                            { "windowTitle", windowTitle ?? "All" },
                            { "processId", processId?.ToString() ?? "All" }
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MonitorEventsAsync",
                        RequestParameters = parameters,
                        TimeoutSeconds = duration + 30
                    }
                };

                if (!result.Success)
                {
                    serverResponse.ErrorMessage = result.ErrorMessage;
                    _logger.LogWarningWithOperation(operationId, $"MonitorEvents failed: {result.ErrorMessage}");
                }

                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "MonitorEvents operation failed");
                
                return new ServerEnhancedResponse<EventMonitoringResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MonitorEventsAsync",
                        TimeoutSeconds = duration + 30
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<EventMonitoringStartResult>> StartEventMonitoringAsync(string eventType, string? elementId = null, string? windowTitle = null, int? processId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting StartEventMonitoring for EventType={eventType}");

                var parameters = new Dictionary<string, object>
                {
                    { "eventType", eventType },
                    { "elementId", elementId ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<EventMonitoringStartResult>("StartEventMonitoring", parameters, 30);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<EventMonitoringStartResult>
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
                            { "eventType", eventType },
                            { "elementId", elementId ?? "All" },
                            { "windowTitle", windowTitle ?? "All" },
                            { "processId", processId?.ToString() ?? "All" }
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "StartEventMonitoringAsync",
                        RequestParameters = parameters,
                        TimeoutSeconds = 30
                    }
                };

                if (!result.Success)
                {
                    serverResponse.ErrorMessage = result.ErrorMessage;
                    _logger.LogWarningWithOperation(operationId, $"StartEventMonitoring failed: {result.ErrorMessage}");
                }

                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "StartEventMonitoring operation failed");
                
                return new ServerEnhancedResponse<EventMonitoringStartResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "StartEventMonitoringAsync",
                        TimeoutSeconds = 30
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<EventMonitoringStopResult>> StopEventMonitoringAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, "Starting StopEventMonitoring");

                var parameters = new Dictionary<string, object>();

                var result = await _executor.ExecuteAsync<EventMonitoringStopResult>("StopEventMonitoring", parameters, 30);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<EventMonitoringStopResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "StopEventMonitoringAsync",
                        RequestParameters = parameters,
                        TimeoutSeconds = 30
                    }
                };

                if (!result.Success)
                {
                    serverResponse.ErrorMessage = result.ErrorMessage;
                    _logger.LogWarningWithOperation(operationId, $"StopEventMonitoring failed: {result.ErrorMessage}");
                }

                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "StopEventMonitoring operation failed");
                
                return new ServerEnhancedResponse<EventMonitoringStopResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "StopEventMonitoringAsync",
                        TimeoutSeconds = 30
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<EventLogResult>> GetEventLogAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, "Starting GetEventLog");

                var parameters = new Dictionary<string, object>();

                var result = await _executor.ExecuteAsync<EventLogResult>("GetEventLog", parameters, 30);

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<EventLogResult>
                {
                    Success = result.Success,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetEventLogAsync",
                        RequestParameters = parameters,
                        TimeoutSeconds = 30
                    }
                };

                if (!result.Success)
                {
                    serverResponse.ErrorMessage = result.ErrorMessage;
                    _logger.LogWarningWithOperation(operationId, $"GetEventLog failed: {result.ErrorMessage}");
                }

                return serverResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "GetEventLog operation failed");
                
                return new ServerEnhancedResponse<EventLogResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId)
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "GetEventLogAsync",
                        TimeoutSeconds = 30
                    }
                };
            }
        }
    }
}