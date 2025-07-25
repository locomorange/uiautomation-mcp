using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Worker.Results
{
    /// <summary>
    /// Text segment with attributes
    /// </summary>
    public class TextSegment 
    {
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string Text { get; set; } = string.Empty;
        public SegmentAttributes Attributes { get; set; } = new SegmentAttributes();
    }

    /// <summary>
    /// Text attributes for segments
    /// </summary>
    public class SegmentAttributes 
    { 
        public string FontFamily { get; set; } = string.Empty;
        public string FontName { get; set; } = string.Empty;
        public double FontSize { get; set; }
        public string FontWeight { get; set; } = string.Empty;
        public string ForegroundColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string UnderlineStyle { get; set; } = string.Empty;
        public bool IsStrikethrough { get; set; }
        public string StrikethroughStyle { get; set; } = string.Empty;
        public string HorizontalTextAlignment { get; set; } = string.Empty;
        public string Culture { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSubscript { get; set; }
        public bool IsSuperscript { get; set; }
    }

    // Text operation results
    public class SetTextResult : BaseOperationResult { }
    public class SelectTextResult : BaseOperationResult { }
    public class FindTextResult : BaseOperationResult { }
    public class GetTextResult : BaseOperationResult { }
    public class TextInfoResult : BaseOperationResult { }
    public class TextAttributesResult : BaseOperationResult { }
    public class TextSearchResult : BaseOperationResult { }
    public class TextAttributeRange : BaseOperationResult { }
    public class TextRangeAttributes : BaseOperationResult { }
    public class TextAttributes : BaseOperationResult { }
}