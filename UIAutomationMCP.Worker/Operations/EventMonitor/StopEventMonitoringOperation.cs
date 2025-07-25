using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.EventMonitor
{
    /// <summary>
    /// MS Learn ベストプラクティスに従った継続的なイベント監視の停止操作
    /// StartEventMonitoringで開始されたセッションを停止する
    /// </summary>
    public class StopEventMonitoringOperation : BaseUIAutomationOperation<StopEventMonitoringRequest, EventMonitoringStopResult>
    {
        public StopEventMonitoringOperation(ElementFinderService elementFinderService, ILogger<StopEventMonitoringOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(StopEventMonitoringRequest request)
        {
            if (string.IsNullOrEmpty(request.MonitorId))
            {
                return Core.Validation.ValidationResult.Failure("MonitorId is required");
            }

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<EventMonitoringStopResult> ExecuteOperationAsync(StopEventMonitoringRequest request)
        {
            _logger.LogInformation($"Stopping event monitoring session: {request.MonitorId}");

            // StartEventMonitoringOperationから監視セッションを取得して停止
            var session = StartEventMonitoringOperation.GetSession(request.MonitorId);
            if (session == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, $"Event monitoring session not found: {request.MonitorId}");
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

            return Task.FromResult(result);
        }
    }
}
