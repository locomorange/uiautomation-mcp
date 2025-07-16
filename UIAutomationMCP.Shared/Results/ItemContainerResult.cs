using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class FindItemResult : BaseOperationResult
    {
        [JsonPropertyName("foundElementId")]
        public string FoundElementId { get; set; } = "";

        [JsonPropertyName("foundElementName")]
        public string FoundElementName { get; set; } = "";

        [JsonPropertyName("foundElementType")]
        public string FoundElementType { get; set; } = "";

        [JsonPropertyName("foundElementBounds")]
        public Dictionary<string, double>? FoundElementBounds { get; set; }

        [JsonPropertyName("found")]
        public bool Found { get; set; }

        [JsonPropertyName("searchDetails")]
        public Dictionary<string, object> SearchDetails { get; set; } = new();
    }
}