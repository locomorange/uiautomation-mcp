using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextAttributesResult : BaseOperationResult
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
        [JsonPropertyName("textAttributes")]
        public TextAttributes TextAttributes { get; set; } = new();
        
        [JsonPropertyName("startPosition")]
        public int StartPosition { get; set; }
        
        [JsonPropertyName("endPosition")]
        public int EndPosition { get; set; }
        
        [JsonPropertyName("textContent")]
        public string? TextContent { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("attributeRanges")]
        public List<TextAttributeRange> AttributeRanges { get; set; } = new();
        
        [JsonPropertyName("hasAttributes")]
        public bool HasAttributes { get; set; }
        
        
        [JsonPropertyName("supportedAttributes")]
        public List<string> SupportedAttributes { get; set; } = new();
        
        [JsonPropertyName("textRanges")]
        public List<TextRangeAttributes> TextRanges { get; set; } = new();
    }

    public class TextAttributeRange
    {
        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; }
        
        [JsonPropertyName("endIndex")]
        public int EndIndex { get; set; }
        
        [JsonPropertyName("length")]
        public int Length { get; set; }
        
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        [JsonPropertyName("attributes")]
        public TextAttributes Attributes { get; set; } = new();
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("fontName")]
        public string? FontName { get; set; }
        
        [JsonPropertyName("fontSize")]
        public double FontSize { get; set; }
        
        [JsonPropertyName("fontWeight")]
        public string? FontWeight { get; set; }
        
        [JsonPropertyName("fontStyle")]
        public string? FontStyle { get; set; }
        
        [JsonPropertyName("foregroundColor")]
        public string? ForegroundColor { get; set; }
        
        [JsonPropertyName("backgroundColor")]
        public string? BackgroundColor { get; set; }
        
        [JsonPropertyName("isUnderline")]
        public bool IsUnderline { get; set; }
        
        [JsonPropertyName("isStrikethrough")]
        public bool IsStrikethrough { get; set; }
        
        [JsonPropertyName("isItalic")]
        public bool IsItalic { get; set; }
        
        [JsonPropertyName("isBold")]
        public bool IsBold { get; set; }
    }
}