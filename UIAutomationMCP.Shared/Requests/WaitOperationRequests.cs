using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === Wait操作 ===

    public class WaitForInputIdleRequest : TypedWorkerRequest
    {
        public override string Operation => "WaitForInputIdle";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("timeoutMilliseconds")]
        public int TimeoutMilliseconds { get; set; } = 10000;
    }
}