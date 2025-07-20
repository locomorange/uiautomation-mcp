using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Selection pattern information result
    /// </summary>
    public class SelectionInfoResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("canSelectMultiple")]
        public bool CanSelectMultiple { get; set; }

        [JsonPropertyName("isSelectionRequired")]
        public bool IsSelectionRequired { get; set; }

        [JsonPropertyName("selectedItems")]
        public string[] SelectedItems { get; set; } = Array.Empty<string>();

        [JsonPropertyName("selectionMode")]
        public string SelectionMode { get; set; } = "";

        [JsonPropertyName("hasSelection")]
        public bool HasSelection { get; set; }

        [JsonPropertyName("selectionCount")]
        public int SelectionCount { get; set; }

        [JsonPropertyName("containerElementId")]
        public string ContainerElementId { get; set; } = "";

        [JsonPropertyName("containerName")]
        public string ContainerName { get; set; } = "";

        [JsonPropertyName("containerControlType")]
        public string ContainerControlType { get; set; } = "";
    }
}