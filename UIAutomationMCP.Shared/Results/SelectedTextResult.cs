using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class SelectedTextResult : BaseOperationResult
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
        [JsonPropertyName("selectedText")]
        public string? SelectedText { get; set; }
        
        [JsonPropertyName("selectionStart")]
        public int SelectionStart { get; set; }
        
        [JsonPropertyName("selectionEnd")]
        public int SelectionEnd { get; set; }
        
        [JsonPropertyName("selectionLength")]
        public int SelectionLength { get; set; }
        
        [JsonPropertyName("hasSelection")]
        public bool HasSelection { get; set; }
        
        [JsonPropertyName("selectionBoundingRectangle")]
        public BoundingRectangle SelectionBoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("selectionAttributes")]
        public Dictionary<string, object> SelectionAttributes { get; set; } = new();
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("isReadOnly")]
        public bool IsReadOnly { get; set; }
        
        [JsonPropertyName("canSelectText")]
        public bool CanSelectText { get; set; }
        
        [JsonPropertyName("fullText")]
        public string? FullText { get; set; }
        
        [JsonPropertyName("textLength")]
        public int TextLength { get; set; }
        
        [JsonPropertyName("textRanges")]
        public List<TextRangeAttributes> TextRanges { get; set; } = new();

        // Missing property for GetTextSelectionOperation
        [JsonPropertyName("selectedTexts")]
        public List<string> SelectedTexts { get; set; } = new();
    }
}