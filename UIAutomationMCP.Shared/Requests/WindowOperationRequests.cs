using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === ウィンドウ操作 ===

    public class GetDesktopWindowsRequest : TypedWorkerRequest
    {
        public override string Operation => "GetDesktopWindows";

        [JsonPropertyName("includeInvisible")]
        public bool IncludeInvisible { get; set; } = false;
    }

    public class WindowActionRequest : TypedWorkerRequest
    {
        public override string Operation => "WindowAction";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "close", "minimize", "maximize", "restore", "setfocus"
    }

    public class GetWindowInfoRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowInfo";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    public class GetWindowInteractionStateRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowInteractionState";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }

    public class GetWindowCapabilitiesRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowCapabilities";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }
}