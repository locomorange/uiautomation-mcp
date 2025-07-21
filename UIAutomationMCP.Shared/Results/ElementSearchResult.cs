using System.Text.Json;
using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class ElementSearchResult : CollectionOperationResult<UIAutomationMCP.Shared.ElementInfo>
    {
        public List<UIAutomationMCP.Shared.ElementInfo> Elements 
        { 
            get => Items;
            set => Items = value ?? new List<UIAutomationMCP.Shared.ElementInfo>();
        }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [JsonPropertyName("searchCriteria")]
        public string? SearchCriteria { get; set; }
        
        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }
    }

}