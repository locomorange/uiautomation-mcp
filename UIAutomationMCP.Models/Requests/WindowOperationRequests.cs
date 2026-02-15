using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === ウィンドウ操作 ===


    public class WindowActionRequest : ElementTargetRequest
    {
        public override string Operation => "WindowAction";

        [JsonPropertyName("action")]
        public string Action { get; set; } = ""; // "close", "minimize", "maximize", "restore", "setfocus"

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class GetWindowInfoRequest : ElementTargetRequest
    {
        public override string Operation => "GetWindowInfo";

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class GetWindowInteractionStateRequest : ElementTargetRequest
    {
        public override string Operation => "GetWindowInteractionState";

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class GetWindowCapabilitiesRequest : ElementTargetRequest
    {
        public override string Operation => "GetWindowCapabilities";

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class SetWindowStateRequest : ElementTargetRequest
    {
        public override string Operation => "SetWindowState";

        [JsonPropertyName("windowState")]
        public string WindowState { get; set; } = ""; // "normal", "minimized", "maximized"

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }

    public class MoveWindowRequest : ElementTargetRequest
    {
        public override string Operation => "MoveWindow";

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

    public class ResizeWindowRequest : ElementTargetRequest
    {
        public override string Operation => "ResizeWindow";

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
