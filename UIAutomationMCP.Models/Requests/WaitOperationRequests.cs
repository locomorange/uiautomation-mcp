using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === Wait操作 ===

    public class WaitForInputIdleRequest : ElementTargetRequest
    {
        public override string Operation => "WaitForInputIdle";

        [JsonPropertyName("timeoutMilliseconds")]
        public int TimeoutMilliseconds { get; set; } = 10000;

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }
}
