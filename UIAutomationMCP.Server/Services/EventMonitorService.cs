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

        public async Task<ServerEnhancedResponse<EventMonitoringResult>> MonitorEventsAsync(string eventType, int duration, string? elementId = null, string? windowTitle = null, int? processId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting MonitorEvents for EventType={eventType}, Duration={duration}");

                var request = new MonitorEventsRequest
                {
                    EventTypes = new[] { eventType },
                    Duration = duration,
                    ElementId = elementId ?? "",
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<MonitorEventsRequest, EventMonitoringResult>("MonitorEvents", request, duration + 30); // Add buffer to timeout

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
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["eventTypes"] = request.EventTypes,
                            ["duration"] = request.Duration,
                            ["elementId"] = request.ElementId,
                            ["windowTitle"] = request.WindowTitle ?? "",
                            ["processId"] = request.ProcessId ?? 0
                        },
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

                var request = new StartEventMonitoringRequest
                {
                    EventTypes = new[] { eventType },
                    ElementId = elementId ?? "",
                    WindowTitle = windowTitle,
                    ProcessId = processId
                };

                var result = await _executor.ExecuteAsync<StartEventMonitoringRequest, EventMonitoringStartResult>("StartEventMonitoring", request, 30);

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
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["eventTypes"] = request.EventTypes,
                            ["elementId"] = request.ElementId,
                            ["windowTitle"] = request.WindowTitle ?? "",
                            ["processId"] = request.ProcessId ?? 0
                        },
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

                var request = new StopEventMonitoringRequest
                {
                    MonitorId = ""
                };

                var result = await _executor.ExecuteAsync<StopEventMonitoringRequest, EventMonitoringStopResult>("StopEventMonitoring", request, 30);

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
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["monitorId"] = request.MonitorId
                        },
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

                var request = new GetEventLogRequest
                {
                    MonitorId = "",
                    MaxCount = 100
                };

                var result = await _executor.ExecuteAsync<GetEventLogRequest, EventLogResult>("GetEventLog", request, 30);

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
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["monitorId"] = request.MonitorId,
                            ["maxCount"] = request.MaxCount
                        },
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