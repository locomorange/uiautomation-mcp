using Microsoft.Extensions.Logging;
using UIAutomationMCP.Core.Models;
using UIAutomationMCP.Monitor.Infrastructure;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Serialization;

namespace UIAutomationMCP.Monitor.Operations
{
    /// <summary>
    /// Get event log operation for Monitor process
    /// </summary>
    public class GetEventLogOperation
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<GetEventLogOperation> _logger;

        public GetEventLogOperation(
            SessionManager sessionManager,
            ILogger<GetEventLogOperation> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        public async Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                if (string.IsNullOrEmpty(parametersJson))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Error = "ParametersJson is null or empty",
                        Data = new EventLogResult()
                    };
                }

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

                _logger.LogInformation("Getting event log - MonitorId: {MonitorId}, MaxCount: {MaxCount}", 
                    request.MonitorId, request.MaxCount);

                var session = _sessionManager.GetSession(request.MonitorId);
                if (session == null)
                {
                    _logger.LogWarning("Session not found: {MonitorId}", request.MonitorId);
                    
                    return new OperationResult
                    {
                        Success = false,
                        Error = $"Monitoring session '{request.MonitorId}' not found",
                        Data = new EventLogResult
                        {
                            Success = false,
                            MonitorId = request.MonitorId,
                            Events = new List<EventData>(),
                            SessionActive = false
                        }
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