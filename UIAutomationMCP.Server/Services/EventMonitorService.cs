using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
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

        public async Task<ServerEnhancedResponse<EventMonitoringResult>> MonitorEventsAsync(string eventType, int duration, string? automationId = null, string? name = null, string? controlType = null, int? processId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            // Ensure sufficient timeout for event monitoring operations
            var timeoutSeconds = Math.Max(duration + 60, 90); // Minimum 90 seconds or duration + 60
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting MonitorEvents for EventType={eventType}, Duration={duration}, AutomationId={automationId}, Name={name}, ControlType={controlType}");

                var request = new MonitorEventsRequest
                {
                    EventTypes = new[] { eventType },
                    Duration = duration,
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    WindowTitle = null,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<MonitorEventsRequest, EventMonitoringResult>("MonitorEvents", request, timeoutSeconds);

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
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "MonitorEventsAsync",
                        TimeoutSeconds = timeoutSeconds
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
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<EventMonitoringStartResult>> StartEventMonitoringAsync(string eventType, string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"[EventMonitor] Starting StartEventMonitoring - EventType={eventType}, AutomationId={automationId ?? "null"}, Name={name ?? "null"}, ControlType={controlType ?? "null"}, ProcessId={processId?.ToString() ?? "null"}, TimeoutSeconds={timeoutSeconds}");

                var request = new StartEventMonitoringRequest
                {
                    EventTypes = new[] { eventType },
                    AutomationId = automationId,
                    Name = name,
                    ControlType = controlType,
                    WindowTitle = null,
                    ProcessId = processId
                };

                _logger.LogInformationWithOperation(operationId, $"[EventMonitor] Serialized request: EventTypes=[{string.Join(", ", request.EventTypes)}], AutomationId={request.AutomationId ?? "null"}, Name={request.Name ?? "null"}, ControlType={request.ControlType ?? "null"}, ProcessId={request.ProcessId?.ToString() ?? "null"}");
                _logger.LogInformationWithOperation(operationId, $"[EventMonitor] Calling _executor.ExecuteAsync with operation='StartEventMonitoring', timeoutSeconds={timeoutSeconds}");

                var result = await _executor.ExecuteAsync<StartEventMonitoringRequest, EventMonitoringStartResult>("StartEventMonitoring", request, timeoutSeconds);

                _logger.LogInformationWithOperation(operationId, $"[EventMonitor] _executor.ExecuteAsync completed - Success={result?.Success}, SessionId={result?.SessionId ?? "null"}, ErrorMessage={result?.ErrorMessage ?? "null"}");

                stopwatch.Stop();
                
                var serverResponse = new ServerEnhancedResponse<EventMonitoringStartResult>
                {
                    Success = result?.Success ?? false,
                    Data = result,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "StartEventMonitoringAsync",
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                if (result != null && !result.Success)
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
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<EventMonitoringStopResult>> StopEventMonitoringAsync(string? sessionId = null, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting StopEventMonitoring for session: {sessionId ?? "null"}");

                var request = new StopEventMonitoringRequest
                {
                    MonitorId = sessionId ?? ""
                };

                var result = await _executor.ExecuteAsync<StopEventMonitoringRequest, EventMonitoringStopResult>("StopEventMonitoring", request, timeoutSeconds);

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
                        TimeoutSeconds = timeoutSeconds
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
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }

        public async Task<ServerEnhancedResponse<EventLogResult>> GetEventLogAsync(string? sessionId = null, int maxCount = 100, int timeoutSeconds = 60)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting GetEventLog for session: {sessionId ?? "null"}");

                var request = new GetEventLogRequest
                {
                    MonitorId = sessionId ?? "",
                    MaxCount = maxCount
                };

                var result = await _executor.ExecuteAsync<GetEventLogRequest, EventLogResult>("GetEventLog", request, timeoutSeconds);

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
                        TimeoutSeconds = timeoutSeconds
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
                        TimeoutSeconds = timeoutSeconds
                    }
                };
            }
        }
    }
}