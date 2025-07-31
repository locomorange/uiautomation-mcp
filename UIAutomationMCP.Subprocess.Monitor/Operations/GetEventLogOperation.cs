using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Subprocess.Monitor.Infrastructure;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Monitor.Abstractions;

namespace UIAutomationMCP.Subprocess.Monitor.Operations
{
    /// <summary>
    /// Get event log operation for Monitor process
    /// </summary>
    public class GetEventLogOperation : BaseMonitorOperation<GetEventLogRequest, EventLogResult>
    {
        public GetEventLogOperation(
            SessionManager sessionManager,
            ElementFinderService elementFinderService,
            ILogger<GetEventLogOperation> logger)
            : base(sessionManager, elementFinderService, logger)
        {
        }

        protected override Task<EventLogResult> ExecuteOperationAsync(GetEventLogRequest request)
        {
            _logger.LogInformation("Getting event log - MonitorId: {MonitorId}, MaxCount: {MaxCount}", 
                request.MonitorId, request.MaxCount);

            var session = _sessionManager.GetSession(request.MonitorId);
            if (session == null)
            {
                _logger.LogWarning("Session not found: {MonitorId}", request.MonitorId);
                
                return Task.FromResult(new EventLogResult
                {
                    Success = false,
                    MonitorId = request.MonitorId,
                    Events = new List<TypedEventData>(),
                    SessionActive = false
                });
            }

            var typedEvents = session.GetCapturedEvents(request.MaxCount);

            var result = new EventLogResult
            {
                Success = true,
                MonitorId = request.MonitorId,
                Events = typedEvents,
                SessionActive = session.IsActive
            };

            _logger.LogInformation("Retrieved {EventCount} events for session {SessionId}", 
                typedEvents.Count, request.MonitorId);

            return Task.FromResult(result);
        }

    }
}

