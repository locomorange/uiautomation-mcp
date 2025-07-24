using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    // === ウィンドウ操作 ===


    public class WindowActionRequest : TypedWorkerRequest
    {
        public override string Operation => "WindowAction";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "close", "minimize", "maximize", "restore", "setfocus"

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class GetWindowInfoRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowInfo";

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

    public class GetWindowInteractionStateRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowInteractionState";

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

    public class GetWindowCapabilitiesRequest : TypedWorkerRequest
    {
        public override string Operation => "GetWindowCapabilities";

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

    public class SetWindowStateRequest : TypedWorkerRequest
    {
        public override string Operation => "SetWindowState";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("windowState")]
        public string WindowState { get; set; } = ""; // "normal", "minimized", "maximized"

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class MoveWindowRequest : TypedWorkerRequest
    {
        public override string Operation => "MoveWindow";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class ResizeWindowRequest : TypedWorkerRequest
    {
        public override string Operation => "ResizeWindow";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }
}