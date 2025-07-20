using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    public class TextInfoResult : BaseOperationResult
    {
        [JsonPropertyName("elementId")]
        public string? ElementId { get; set; }
        
        [JsonPropertyName("elementName")]
        public string? ElementName { get; set; }
        
        [JsonPropertyName("elementAutomationId")]
        public string? ElementAutomationId { get; set; }
        
        [JsonPropertyName("elementControlType")]
        public string? ElementControlType { get; set; }
        
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }
        
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }
        
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        [JsonPropertyName("length")]
        public int Length { get; set; }
        
        [JsonPropertyName("selectedText")]
        public string? SelectedText { get; set; }
        
        [JsonPropertyName("hasSelection")]
        public bool HasSelection { get; set; }
        
        [JsonPropertyName("selectionStart")]
        public int SelectionStart { get; set; }
        
        [JsonPropertyName("selectionEnd")]
        public int SelectionEnd { get; set; }
        
        [JsonPropertyName("selectionLength")]
        public int SelectionLength { get; set; }
        
        [JsonPropertyName("selectionBoundingRectangle")]
        public BoundingRectangle SelectionBoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("textAttributes")]
        public Dictionary<string, object> TextAttributes { get; set; } = new();
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
        
        [JsonPropertyName("canSelectText")]
        public bool CanSelectText { get; set; }
        
        [JsonPropertyName("textRanges")]
        public List<TextRangeAttributes> TextRanges { get; set; } = new();
        
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();
        
        [JsonPropertyName("supportedPatterns")]
        public List<string> SupportedPatterns { get; set; } = new();
        
        [JsonPropertyName("supportedTextAttributes")]
        public List<string> SupportedTextAttributes { get; set; } = new();

        // Additional properties referenced in tests
        [JsonPropertyName("textLength")]
        public int TextLength { get; set; }
        
        [JsonPropertyName("isPasswordField")]
        public bool IsPasswordField { get; set; }
        
        [JsonPropertyName("isMultiline")]
        public bool IsMultiline { get; set; }
        
        [JsonPropertyName("canEditText")]
        public bool CanEditText { get; set; }
        
        [JsonPropertyName("hasText")]
        public bool HasText { get; set; }
        
        [JsonPropertyName("textPattern")]
        public string? TextPattern { get; set; }
        
        [JsonPropertyName("inputType")]
        public string? InputType { get; set; }
    }
}