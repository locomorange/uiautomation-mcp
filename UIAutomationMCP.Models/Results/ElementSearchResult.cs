using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    [MessagePackObject]
    public class ElementSearchResult : CollectionOperationResult<UIAutomationMCP.Models.ElementInfo>
    {
        public List<UIAutomationMCP.Models.ElementInfo> Elements 
        { 
            get => Items;
            set => Items = value ?? new List<UIAutomationMCP.Models.ElementInfo>();
        }
        
        [Key(8)]
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [Key(9)]
        [JsonPropertyName("searchCriteria")]
        public string? SearchCriteria { get; set; }
        
        [Key(10)]
        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }
    }

}