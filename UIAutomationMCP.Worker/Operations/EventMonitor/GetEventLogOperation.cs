using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;

namespace UIAutomationMCP.Worker.Operations.EventMonitor
{
    /// <summary>
    /// MS Learn ベストプラクティスに従ったイベントログ取得操作
    /// 継続監視中のセッションからイベントログを取得する
    /// </summary>
    public class GetEventLogOperation : IUIAutomationOperation
    {
        private readonly ILogger<GetEventLogOperation> _logger;

        public GetEventLogOperation(ILogger<GetEventLogOperation> logger)
        {
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var request = JsonSerializationHelper.Deserialize<GetEventLogRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "Failed to deserialize GetEventLogRequest",
                        Data = new EventLogResult()
                    };
                }

                _logger.LogInformation($"Getting event log for session: {request.MonitorId}");

                // セッションからイベントログを取得
                var session = StartEventMonitoringOperation.GetSession(request.MonitorId);
                if (session == null)
                {
                    var errorResult = new EventLogResult
                    {
                        Success = false,
                        MonitorId = request.MonitorId,
                        Events = new List<EventData>()
                    };

                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Session not found: {request.MonitorId}",
                        Data = errorResult
                    };
                }

                // MS Learn推奨: スレッドセーフなデータ取得
                var events = session.PeekCapturedEvents(request.MaxCount);

                var result = new EventLogResult
                {
                    Success = true,
                    MonitorId = request.MonitorId,
                    Events = events,
                    TotalEventCount = session.EventCount,
                    SessionActive = session.IsActive,
                    StartTime = session.StartTime
                };

                _logger.LogInformation($"Retrieved {events.Count} events from session {request.MonitorId}");

                return new OperationResult 
                { 
                    Success = true, 
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEventLog operation failed");
                return new OperationResult 
                { 
                    Success = false, 
                    Error = $"GetEventLog failed: {ex.Message}",
                    Data = new EventLogResult()
                };
            }
        }
    }
}
