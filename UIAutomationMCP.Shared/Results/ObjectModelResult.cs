using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class ObjectModelResult
    {
        [JsonPropertyName("objectModel")]
        public object? ObjectModel { get; set; }
        
        [JsonPropertyName("typeName")]
        public string TypeName { get; set; } = string.Empty;
        
        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
        
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
        
        [JsonPropertyName("elementInfo")]
        public Dictionary<string, object> ElementInfo { get; set; } = new();
    }
}