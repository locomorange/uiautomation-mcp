using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class StyleInfoResult
    {
        [JsonPropertyName("styleId")]
        public int StyleId { get; set; }
        
        [JsonPropertyName("styleName")]
        public string StyleName { get; set; } = string.Empty;
        
        [JsonPropertyName("fillColor")]
        public int FillColor { get; set; }
        
        [JsonPropertyName("fillPatternColor")]
        public int FillPatternColor { get; set; }
        
        [JsonPropertyName("shape")]
        public string Shape { get; set; } = string.Empty;
        
        [JsonPropertyName("fillPatternStyle")]
        public string FillPatternStyle { get; set; } = string.Empty;
        
        [JsonPropertyName("extendedProperties")]
        public string ExtendedProperties { get; set; } = string.Empty;
        
        [JsonPropertyName("isPatternAvailable")]
        public bool IsPatternAvailable { get; set; }
        
        [JsonPropertyName("elementInfo")]
        public Dictionary<string, object> ElementInfo { get; set; } = new();
    }
}