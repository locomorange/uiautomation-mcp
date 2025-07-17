using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for multiple view operations
    /// </summary>
    public class MultipleViewResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = string.Empty;

        [JsonPropertyName("currentView")]
        public int CurrentView { get; set; }

        [JsonPropertyName("supportedViews")]
        public List<int> SupportedViews { get; set; } = new();

        [JsonPropertyName("viewNames")]
        public Dictionary<int, string> ViewNames { get; set; } = new();

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
    }
}