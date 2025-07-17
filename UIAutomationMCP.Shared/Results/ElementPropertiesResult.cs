using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for element properties operations
    /// </summary>
    public class ElementPropertiesResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        [JsonPropertyName("className")]
        public string ClassName { get; set; } = string.Empty;

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }

        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();

        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("basicInfo")]
        public ElementInfo? BasicInfo { get; set; }
        
        [JsonPropertyName("extendedProperties")]
        public Dictionary<string, object> ExtendedProperties { get; set; } = new();
        
        [JsonPropertyName("frameworkId")]
        public string? FrameworkId { get; set; }
        
        [JsonPropertyName("runtimeId")]
        public string? RuntimeId { get; set; }
        
        [JsonPropertyName("supportedPatterns")]
        public List<string> SupportedPatterns { get; set; } = new();
    }
}