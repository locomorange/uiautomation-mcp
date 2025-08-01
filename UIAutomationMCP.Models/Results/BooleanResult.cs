using System.Text.Json.Serialization;
using UIAutomationMCP.Models.Results;
using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    [MessagePackObject]
    public class BooleanResult : BaseOperationResult
    {
        [Key(6)]
        [JsonPropertyName("value")]
        public bool Value { get; set; }
        
        [Key(7)]
        [JsonPropertyName("propertyName")]
        public string? PropertyName { get; set; }
        
        [Key(8)]
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [Key(9)]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [Key(10)]
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }
        
        [Key(11)]
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [Key(12)]
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [Key(13)]
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [Key(14)]
        [JsonPropertyName("method")]
        public string? Method { get; set; }
        
        [Key(15)]
        [JsonPropertyName("context")]
        public Dictionary<string, object> Context { get; set; } = new();
        
        [Key(16)]
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}