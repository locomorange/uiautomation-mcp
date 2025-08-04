using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === Wait操作 ===

    public class WaitForInputIdleRequest : TypedWorkerRequest
    {
        public override string Operation => "WaitForInputIdle";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("windowHandle")]
        public long? WindowHandle { get; set; }

        [JsonPropertyName("timeoutMilliseconds")]
        public int TimeoutMilliseconds { get; set; } = 10000;

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }
}
