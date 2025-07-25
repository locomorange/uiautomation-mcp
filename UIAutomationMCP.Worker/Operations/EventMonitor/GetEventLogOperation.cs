using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.EventMonitor
{
    /// <summary>
    /// MS Learn ベストプラクティスに従ったイベントログ取得操作
    /// 継続監視中のセッションからイベントログを取得する
    /// </summary>
    public class GetEventLogOperation : BaseUIAutomationOperation<GetEventLogRequest, EventLogResult>
    {
        public GetEventLogOperation(ElementFinderService elementFinderService, ILogger<GetEventLogOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(GetEventLogRequest request)
        {
            if (string.IsNullOrEmpty(request.MonitorId))
            {
                return Core.Validation.ValidationResult.Failure("MonitorId is required");
            }

            if (request.MaxCount <= 0)
            {
                return Core.Validation.ValidationResult.Failure("MaxCount must be greater than 0");
            }

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<EventLogResult> ExecuteOperationAsync(GetEventLogRequest request)
        {
            _logger.LogInformation($"Getting event log for session: {request.MonitorId}. Available sessions: {string.Join(", ", StartEventMonitoringOperation.GetActiveSessions().Select(s => s.SessionId))}");

            // セッションからイベントログを取得
            var session = StartEventMonitoringOperation.GetSession(request.MonitorId);
            if (session == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, $"Session not found: {request.MonitorId}");
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

            return Task.FromResult(result);
        }
    }
}
