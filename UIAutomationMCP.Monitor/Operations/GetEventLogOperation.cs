using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.Monitor.Abstractions;

namespace UIAutomationMCP.Monitor.Operations
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

        protected override async Task<EventLogResult> ExecuteOperationAsync(GetEventLogRequest request)
        {
            _logger.LogInformation("Getting event log - MonitorId: {MonitorId}, MaxCount: {MaxCount}", 
                request.MonitorId, request.MaxCount);

            var session = _sessionManager.GetSession(request.MonitorId);
            if (session == null)
            {
                _logger.LogWarning("Session not found: {MonitorId}", request.MonitorId);
                
                return new EventLogResult
                {
                    Success = false,
                    MonitorId = request.MonitorId,
                    Events = new List<EventData>(),
                    SessionActive = false
                };
            }

            var typedEvents = session.GetCapturedEvents(request.MaxCount);
            
            // Convert typed events to legacy format for compatibility
            var legacyEvents = typedEvents.Select(ConvertToLegacyEventData).ToList();

            var result = new EventLogResult
            {
                Success = true,
                MonitorId = request.MonitorId,
                Events = legacyEvents,
                SessionActive = session.IsActive
            };

            _logger.LogInformation("Retrieved {EventCount} events for session {SessionId}", 
                legacyEvents.Count, request.MonitorId);

            return result;
        }

        private EventData ConvertToLegacyEventData(TypedEventData typedEvent)
        {
            var legacyEvent = new EventData
            {
                EventType = typedEvent.EventType,
                Timestamp = typedEvent.Timestamp,
                SourceElement = typedEvent.SourceElement,
                EventDataProperties = new Dictionary<string, object>
                {
                    ["SessionId"] = typedEvent.SessionId
                }
            };

            // Add type-specific properties
            switch (typedEvent)
            {
                case InvokeEventData invokeEvent:
                    legacyEvent.EventDataProperties["EventId"] = invokeEvent.EventId;
                    break;
                
                case SelectionEventData selectionEvent:
                    legacyEvent.EventDataProperties["EventId"] = selectionEvent.EventId;
                    break;
                
                case TextChangedEventData textEvent:
                    legacyEvent.EventDataProperties["EventId"] = textEvent.EventId;
                    break;
                
                case PropertyChangedEventData propertyEvent:
                    legacyEvent.EventDataProperties["PropertyId"] = propertyEvent.PropertyId;
                    legacyEvent.EventDataProperties["NewValue"] = propertyEvent.NewValue;
                    legacyEvent.EventDataProperties["OldValue"] = propertyEvent.OldValue;
                    break;
                
                case StructureChangedEventData structureEvent:
                    legacyEvent.EventDataProperties["StructureChangeType"] = structureEvent.StructureChangeType;
                    legacyEvent.EventDataProperties["RuntimeId"] = structureEvent.RuntimeId;
                    break;
                
                case GenericEventData genericEvent:
                    legacyEvent.EventDataProperties["EventId"] = genericEvent.EventId;
                    break;
            }

            return legacyEvent;
        }
    }
}