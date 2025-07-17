using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextAttributesResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> TextAttributes { get; set; } = new();
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string? TextContent { get; set; }
        public string? Pattern { get; set; }
        public List<TextAttributeRange> AttributeRanges { get; set; } = new();
        public bool HasAttributes { get; set; }
        public int AttributeCount { get; set; }
        public List<string> SupportedAttributes { get; set; } = new();
        public List<TextRangeAttributes> TextRanges { get; set; } = new();
    }

    public class TextAttributeRange
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get; set; }
        public string? Text { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public Rectangle BoundingRectangle { get; set; } = new();
        public string? FontName { get; set; }
        public double FontSize { get; set; }
        public string? FontWeight { get; set; }
        public string? FontStyle { get; set; }
        public string? ForegroundColor { get; set; }
        public string? BackgroundColor { get; set; }
        public bool IsUnderline { get; set; }
        public bool IsStrikethrough { get; set; }
        public bool IsItalic { get; set; }
        public bool IsBold { get; set; }
    }
}