using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for patterns information operations
    /// </summary>
    public class PatternsInfoResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = string.Empty;

        [JsonPropertyName("supportedPatterns")]
        public List<string> SupportedPatterns { get; set; } = new();

        [JsonPropertyName("patternDetails")]
        public Dictionary<string, object> PatternDetails { get; set; } = new();

        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }
        
        [JsonPropertyName("patterns")]
        public List<PatternInfoResult> Patterns { get; set; } = new();
    }
    
    public class PatternInfoResult
    {
        [JsonPropertyName("patternName")]
        public string PatternName { get; set; } = string.Empty;
        
        [JsonPropertyName("isSupported")]
        public bool IsSupported { get; set; }
        
        [JsonPropertyName("patternProperties")]
        public Dictionary<string, object> PatternProperties { get; set; } = new();
        
        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
        
        [JsonPropertyName("currentState")]
        public Dictionary<string, object> CurrentState { get; set; } = new();
    }
}