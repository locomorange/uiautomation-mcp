using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result for property operations
    /// </summary>
    public class PropertyResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();
        
        [JsonPropertyName("propertyId")]
        public string? PropertyId { get; set; }
        
        [JsonPropertyName("value")]
        public object? Value { get; set; }
        
        [JsonPropertyName("propertyType")]
        public string? PropertyType { get; set; }
        
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
        
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("propertyCount")]
        public int PropertyCount => Properties.Count;
        
        [JsonPropertyName("hasValue")]
        public bool HasValue => Value != null;
        
        [JsonPropertyName("supportedPropertyIds")]
        public List<string> SupportedPropertyIds { get; set; } = new();
        
        [JsonPropertyName("validationErrors")]
        public List<string> ValidationErrors { get; set; } = new();
        
        [JsonPropertyName("isValid")]
        public bool IsValid => ValidationErrors.Count == 0;
    }
}