using System.Text.Json.Serialization;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Models.Results
{
    public class BooleanResult : BaseOperationResult
    {
        [JsonPropertyName("value")]
        public bool Value { get; set; }
        
        [JsonPropertyName("propertyName")]
        public string? PropertyName { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("method")]
        public string? Method { get; set; }
        
        [JsonPropertyName("context")]
        public Dictionary<string, object> Context { get; set; } = new();
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}