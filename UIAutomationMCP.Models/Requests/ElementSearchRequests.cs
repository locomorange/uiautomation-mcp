using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Requests
{
    // === 要素検索 ===

    public class FindElementsRequest : TypedWorkerRequest
    {
        public override string Operation => "FindElements";

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }

        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }

        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }

        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "descendants"; // "descendants", "children", "subtree"

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;

        [JsonPropertyName("useCache")]
        public bool UseCache { get; set; } = true;

        [JsonPropertyName("useRegex")]
        public bool UseRegex { get; set; } = false;

        [JsonPropertyName("useWildcard")]
        public bool UseWildcard { get; set; } = false;

        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; } = 100;

        [JsonPropertyName("validatePatterns")]
        public bool ValidatePatterns { get; set; } = true;
    }


}