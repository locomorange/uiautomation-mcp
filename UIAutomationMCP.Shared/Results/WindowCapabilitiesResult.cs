using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Window capabilities result
    /// </summary>
    public class WindowCapabilitiesResult : BaseOperationResult
    {
        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        [JsonPropertyName("canMove")]
        public bool CanMove { get; set; }

        [JsonPropertyName("canResize")]
        public bool CanResize { get; set; }

        [JsonPropertyName("canRotate")]
        public bool CanRotate { get; set; }

        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }

        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }

        [JsonPropertyName("canClose")]
        public bool CanClose { get; set; }

        [JsonPropertyName("supportsTransform")]
        public bool SupportsTransform { get; set; }

        [JsonPropertyName("supportsWindow")]
        public bool SupportsWindow { get; set; }

        [JsonPropertyName("isResizable")]
        public bool IsResizable { get; set; }

        [JsonPropertyName("windowInteractionState")]
        public string WindowInteractionState { get; set; } = "";

        [JsonPropertyName("isMovable")]
        public bool IsMovable { get; set; }

        [JsonPropertyName("hasSystemMenu")]
        public bool HasSystemMenu { get; set; }

        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }

        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }

        [JsonPropertyName("windowVisualState")]
        public string WindowVisualState { get; set; } = "";
    }
}