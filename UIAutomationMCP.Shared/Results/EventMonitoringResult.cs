using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result for event monitoring operations
    /// </summary>
    public class EventMonitoringResult : BaseOperationResult
    {
        /// <summary>
        /// Type of event being monitored
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = "";

        /// <summary>
        /// Duration of monitoring in seconds
        /// </summary>
        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// Element ID being monitored (if specified)
        /// </summary>
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }

        /// <summary>
        /// Window title being monitored (if specified)
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// Process ID being monitored (if specified)
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Events captured during monitoring
        /// </summary>
        [JsonPropertyName("capturedEvents")]
        public List<EventData> CapturedEvents { get; set; } = new();

        /// <summary>
        /// Number of events captured
        /// </summary>
        [JsonPropertyName("eventCount")]
        public int EventCount => CapturedEvents.Count;
    }

    /// <summary>
    /// Result for starting event monitoring
    /// </summary>
    public class EventMonitoringStartResult : BaseOperationResult
    {
        /// <summary>
        /// Type of event being monitored
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = "";

        /// <summary>
        /// Element ID being monitored (if specified)
        /// </summary>
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }

        /// <summary>
        /// Window title being monitored (if specified)
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// Process ID being monitored (if specified)
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Monitor session ID
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = "";

        /// <summary>
        /// Status of monitoring
        /// </summary>
        [JsonPropertyName("monitoringStatus")]
        public string MonitoringStatus { get; set; } = "Started";
    }

    /// <summary>
    /// Result for stopping event monitoring
    /// </summary>
    public class EventMonitoringStopResult : BaseOperationResult
    {
        /// <summary>
        /// Monitor session ID that was stopped
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = "";

        /// <summary>
        /// Status of monitoring
        /// </summary>
        [JsonPropertyName("monitoringStatus")]
        public string MonitoringStatus { get; set; } = "Stopped";

        /// <summary>
        /// Final count of events captured
        /// </summary>
        [JsonPropertyName("finalEventCount")]
        public int FinalEventCount { get; set; }
    }

    /// <summary>
    /// Result for retrieving event log
    /// </summary>
    public class EventLogResult : BaseOperationResult
    {
        /// <summary>
        /// Events in the log
        /// </summary>
        [JsonPropertyName("events")]
        public List<EventData> Events { get; set; } = new();

        /// <summary>
        /// Total number of events in log
        /// </summary>
        [JsonPropertyName("totalEvents")]
        public int TotalEvents => Events.Count;

        /// <summary>
        /// Time range of events
        /// </summary>
        [JsonPropertyName("timeRange")]
        public EventTimeRange TimeRange { get; set; } = new();
    }

    /// <summary>
    /// Data structure for captured event information
    /// </summary>
    public class EventData
    {
        /// <summary>
        /// Type of event
        /// </summary>
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = "";

        /// <summary>
        /// Timestamp when event occurred
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Element that triggered the event
        /// </summary>
        [JsonPropertyName("sourceElement")]
        public string? SourceElement { get; set; }

        /// <summary>
        /// Additional event-specific data
        /// </summary>
        [JsonPropertyName("eventData")]
        public Dictionary<string, object> EventDataProperties { get; set; } = new();
    }

    /// <summary>
    /// Time range information for event logs
    /// </summary>
    public class EventTimeRange
    {
        /// <summary>
        /// Start time of the range
        /// </summary>
        [JsonPropertyName("startTime")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// End time of the range
        /// </summary>
        [JsonPropertyName("endTime")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Duration of the range
        /// </summary>
        [JsonPropertyName("duration")]
        public TimeSpan Duration => EndTime.HasValue && StartTime.HasValue ? EndTime.Value - StartTime.Value : TimeSpan.Zero;
    }
}