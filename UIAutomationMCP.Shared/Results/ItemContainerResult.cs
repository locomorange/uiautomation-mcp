using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class FindItemResult : BaseOperationResult
    {
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = "";

        [JsonPropertyName("boundingRectangle")]
        public Dictionary<string, double>? BoundingRectangle { get; set; }

        [JsonPropertyName("found")]
        public bool Found { get; set; }

        [JsonPropertyName("searchDetails")]
        public Dictionary<string, object> SearchDetails { get; set; } = new();
    }
}