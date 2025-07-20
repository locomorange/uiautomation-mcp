using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Window interaction state result
    /// </summary>
    public class WindowInteractionStateResult : BaseOperationResult
    {
        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        [JsonPropertyName("windowState")]
        public string WindowState { get; set; } = "";

        [JsonPropertyName("isModal")]
        public bool IsModal { get; set; }

        [JsonPropertyName("isTopmost")]
        public bool IsTopmost { get; set; }

        [JsonPropertyName("canMinimize")]
        public bool CanMinimize { get; set; }

        [JsonPropertyName("canMaximize")]
        public bool CanMaximize { get; set; }

        [JsonPropertyName("canClose")]
        public bool CanClose { get; set; }

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }

        [JsonPropertyName("interactionState")]
        public string InteractionState { get; set; } = "";

        [JsonPropertyName("interactionStateValue")]
        public string InteractionStateValue { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("windowVisualState")]
        public string WindowVisualState { get; set; } = "";
    }
}