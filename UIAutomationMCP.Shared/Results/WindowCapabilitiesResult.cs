using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for window capabilities operations
    /// </summary>
    public class WindowCapabilitiesResult : BaseOperationResult
    {
        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = string.Empty;

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }

        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }

        [JsonPropertyName("canMove")]
        public bool CanMove { get; set; }

        [JsonPropertyName("canResize")]
        public bool CanResize { get; set; }

        [JsonPropertyName("canClose")]
        public bool CanClose { get; set; }

        [JsonPropertyName("isResizable")]
        public bool IsResizable { get; set; }

        [JsonPropertyName("isMovable")]
        public bool IsMovable { get; set; }

        [JsonPropertyName("hasSystemMenu")]
        public bool HasSystemMenu { get; set; }

        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }

        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }

        [JsonPropertyName("windowVisualState")]
        public string WindowVisualState { get; set; } = string.Empty;

        [JsonPropertyName("windowInteractionState")]
        public string WindowInteractionState { get; set; } = string.Empty;
    }
}