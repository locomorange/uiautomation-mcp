using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === ItemContainer Pattern ===

    public class FindItemByPropertyRequest : TypedWorkerRequest
    {
        public override string Operation => "FindItemByProperty";

        [JsonPropertyName("containerId")]
        public string ContainerId { get; set; } = "";

        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = "";

        [JsonPropertyName("value")]
        public string Value { get; set; } = "";

        [JsonPropertyName("startAfterId")]
        public string StartAfterId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }
}