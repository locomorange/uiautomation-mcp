using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class GridItemResult : BaseOperationResult
    {
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
        [JsonPropertyName("row")]
        public int Row { get; set; }
        
        [JsonPropertyName("column")]
        public int Column { get; set; }
        
        [JsonPropertyName("rowSpan")]
        public int RowSpan { get; set; }
        
        [JsonPropertyName("columnSpan")]
        public int ColumnSpan { get; set; }
        
        [JsonPropertyName("containerAutomationId")]
        public string? ContainerAutomationId { get; set; }
        
        [JsonPropertyName("containerName")]
        public string? ContainerName { get; set; }
        
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
        
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
        
        [JsonPropertyName("isKeyboardFocusable")]
        public bool IsKeyboardFocusable { get; set; }
        
        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }
        
        [JsonPropertyName("isOffscreen")]
        public bool IsOffscreen { get; set; }
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();
        
        [JsonPropertyName("supportedPatterns")]
        public List<string> SupportedPatterns { get; set; } = new();
        
        [JsonPropertyName("element")]
        public ElementInfo? Element { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
    }
}