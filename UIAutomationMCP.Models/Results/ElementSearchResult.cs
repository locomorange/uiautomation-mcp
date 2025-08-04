using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMCP.Models.Results
{
    public class ElementSearchResult : CollectionOperationResult<UIAutomationMCP.Models.ElementInfo>
    {
        public List<UIAutomationMCP.Models.ElementInfo> Elements
        {
            get => Items;
            set => Items = value ?? new List<UIAutomationMCP.Models.ElementInfo>();
        }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("searchCriteria")]
        public string? SearchCriteria { get; set; }

        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }
    }

}
