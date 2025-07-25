using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Monitor.Results
{
    /// <summary>
    /// Result for starting event monitoring in Monitor process
    /// </summary>
    public class EventMonitoringStartResult : BaseOperationResult 
    {
        public string EventType { get; set; } = string.Empty;
        public string ElementId { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string MonitoringStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for stopping event monitoring in Monitor process
    /// </summary>
    public class EventMonitoringStopResult : BaseOperationResult 
    { 
        public string SessionId { get; set; } = string.Empty;
        public int FinalEventCount { get; set; }
        public string MonitoringStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for getting event log from Monitor process
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