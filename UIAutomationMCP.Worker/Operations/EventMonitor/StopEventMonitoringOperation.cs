using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;

namespace UIAutomationMCP.Worker.Operations.EventMonitor
{
    /// <summary>
    /// MS Learn ベストプラクティスに従った継続的なイベント監視の停止操作
    /// StartEventMonitoringで開始されたセッションを停止する
    /// </summary>
    public class StopEventMonitoringOperation : IUIAutomationOperation
    {
        private readonly ILogger<StopEventMonitoringOperation> _logger;

        public StopEventMonitoringOperation(ILogger<StopEventMonitoringOperation> logger)
        {
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var request = JsonSerializationHelper.Deserialize<StopEventMonitoringRequest>(parametersJson);
                if (request == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Failed to deserialize StopEventMonitoringRequest",
                        Data = new EventMonitoringResult()
                    });
                }

                _logger.LogInformation($"Stopping event monitoring session: {request.MonitorId}");

                // StartEventMonitoringOperationから監視セッションを取得して停止
                var session = StartEventMonitoringOperation.GetSession(request.MonitorId);
                if (session == null)
                {
                    var errorResult = new EventMonitoringStopResult
                    {
                        Success = false,
                        SessionId = request.MonitorId,
                        MonitoringStatus = $"Event monitoring session not found: {request.MonitorId}"
                    };

                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Session not found: {request.MonitorId}",
                        Data = errorResult
                    });
                }

                // MS Learn推奨: 適切なリソース解放
                var capturedEvents = session.GetCapturedEvents();
                var eventCount = capturedEvents.Count;
                session.Dispose();
                StartEventMonitoringOperation.RemoveSession(request.MonitorId);

                var result = new EventMonitoringStopResult
                {
                    Success = true,
                    SessionId = request.MonitorId,
                    FinalEventCount = eventCount,
                    MonitoringStatus = $"Event monitoring session stopped. Captured {eventCount} events."
                };

                _logger.LogInformation($"Successfully stopped event monitoring session {request.MonitorId} with {eventCount} events");

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StopEventMonitoring operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"StopEventMonitoring failed: {ex.Message}",
                    Data = new EventMonitoringResult()
                });
            }
        }
    }
}
