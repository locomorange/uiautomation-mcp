using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Event Monitor操作 ===

    public class MonitorEventsRequest : TypedWorkerRequest
    {
        public override string Operation => "MonitorEvents";

        [JsonPropertyName("eventTypes")]
        public string[] EventTypes { get; set; } = [];

        [JsonPropertyName("duration")]
        public int Duration { get; set; } = 30;

        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class StartEventMonitoringRequest : TypedWorkerRequest
    {
        public override string Operation => "StartEventMonitoring";

        [JsonPropertyName("eventTypes")]
        public string[] EventTypes { get; set; } = [];

        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class StopEventMonitoringRequest : TypedWorkerRequest
    {
        public override string Operation => "StopEventMonitoring";

        [JsonPropertyName("monitorId")]
        public string MonitorId { get; set; } = "";

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class GetEventLogRequest : TypedWorkerRequest
    {
        public override string Operation => "GetEventLog";

        [JsonPropertyName("monitorId")]
        public string MonitorId { get; set; } = "";

        [JsonPropertyName("maxCount")]
        public int MaxCount { get; set; } = 100;

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }
}