using Microsoft.Extensions.Logging;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.Monitor.Abstractions;

namespace UIAutomationMCP.Monitor.Operations
{
    /// <summary>
    /// Stop event monitoring operation for Monitor process
    /// </summary>
    public class StopEventMonitoringOperation : BaseMonitorOperation<StopEventMonitoringRequest, EventMonitoringStopResult>
    {
        public StopEventMonitoringOperation(
            SessionManager sessionManager,
            ElementFinderService elementFinderService,
            ILogger<StopEventMonitoringOperation> logger)
            : base(sessionManager, elementFinderService, logger)
        {
        }

        protected override async Task<EventMonitoringStopResult> ExecuteOperationAsync(StopEventMonitoringRequest request)
        {
            _logger.LogInformation("Stopping event monitoring - MonitorId: {MonitorId}", request.MonitorId);

            var session = _sessionManager.GetSession(request.MonitorId);
            if (session == null)
            {
                _logger.LogWarning("Session not found: {MonitorId}", request.MonitorId);
                
                return new EventMonitoringStopResult
                {
                    Success = false,
                    SessionId = request.MonitorId,
                    FinalEventCount = 0,
                    MonitoringStatus = "Session not found"
                };
            }

            var finalEventCount = session.EventCount;
            session.Stop();
            
            var removed = _sessionManager.RemoveSession(request.MonitorId);
            if (!removed)
            {
                _logger.LogWarning("Failed to remove session from manager: {MonitorId}", request.MonitorId);
            }

            var result = new EventMonitoringStopResult
            {
                Success = true,
                SessionId = request.MonitorId,
                FinalEventCount = finalEventCount,
                MonitoringStatus = "Stopped"
            };

            _logger.LogInformation("Event monitoring stopped successfully - SessionId: {SessionId}, FinalEventCount: {EventCount}",
                request.MonitorId, finalEventCount);

            return result;
        }
    }
}