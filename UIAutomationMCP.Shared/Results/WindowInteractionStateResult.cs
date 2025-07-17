using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for window interaction state operations
    /// </summary>
    public class WindowInteractionStateResult : BaseOperationResult
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

        [JsonPropertyName("windowVisualState")]
        public string WindowVisualState { get; set; } = string.Empty;

        [JsonPropertyName("windowInteractionState")]
        public string WindowInteractionState { get; set; } = string.Empty;

        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }

        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }

        [JsonPropertyName("interactionState")]
        public string InteractionState { get; set; } = string.Empty;

        [JsonPropertyName("interactionStateValue")]
        public int InteractionStateValue { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}