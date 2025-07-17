using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// Result class for control type information operations
    /// </summary>
    public class ControlTypeInfoResult : BaseOperationResult
    {
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        [JsonPropertyName("localizedControlType")]
        public string LocalizedControlType { get; set; } = string.Empty;

        [JsonPropertyName("supportedPatterns")]
        public List<string> SupportedPatterns { get; set; } = new();

        [JsonPropertyName("requiredPatterns")]
        public List<string> RequiredPatterns { get; set; } = new();

        [JsonPropertyName("neverSupportedPatterns")]
        public List<string> NeverSupportedPatterns { get; set; } = new();

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isValidForPattern")]
        public bool IsValidForPattern { get; set; }

        [JsonPropertyName("validationResults")]
        public List<string> ValidationResults { get; set; } = new();
        
        [JsonPropertyName("controlTypeName")]
        public string? ControlTypeName { get; set; }
        
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("availablePatterns")]
        public List<string> AvailablePatterns { get; set; } = new();
        
        [JsonPropertyName("patternValidation")]
        public Dictionary<string, bool> PatternValidation { get; set; } = new();
        
        [JsonPropertyName("defaultProperties")]
        public Dictionary<string, object> DefaultProperties { get; set; } = new();
    }
}