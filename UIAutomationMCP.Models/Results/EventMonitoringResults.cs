namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Result for starting event monitoring
    /// </summary>
    public class EventMonitoringStartResult : BaseOperationResult
    {
        public string EventType { get; set; } = string.Empty;
        public string ElementId { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public long? WindowHandle { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string MonitoringStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for stopping event monitoring
    /// </summary>
    public class EventMonitoringStopResult : BaseOperationResult
    {
        public string SessionId { get; set; } = string.Empty;
        public int FinalEventCount { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for getting event log
    /// </summary>
    public class EventLogResult : BaseOperationResult
    {
        public string MonitorId { get; set; } = string.Empty;
        public List<TypedEventData> Events { get; set; } = new List<TypedEventData>();
        public bool SessionActive { get; set; }
        public int TotalEventCount { get; set; }
        public DateTime StartTime { get; set; }
    }
}
