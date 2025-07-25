namespace UIAutomationMCP.Models.Results
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
    /// Text match result
    /// </summary>
    public class TextMatch 
    {
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string MatchedText { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get; set; }
        public string BoundingRectangle { get; set; } = string.Empty;
        public bool IsHighlighted { get; set; }
        public bool IsSelected { get; set; }
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
    public class TextAttributesResult : BaseOperationResult 
    { 
        public string AutomationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string TextContent { get; set; } = string.Empty;
        public bool HasAttributes { get; set; }
        public string Pattern { get; set; } = string.Empty;
        public string SegmentationMode { get; set; } = string.Empty;
        public List<string> SupportedAttributes { get; set; } = new List<string>();
        public List<TextSegment> TextSegments { get; set; } = new List<TextSegment>();
        public TextAttributes TextAttributes { get; set; } = new TextAttributes();
    }
    public class TextSearchResult : BaseOperationResult 
    { 
        public string SearchText { get; set; } = string.Empty;
        public bool CaseSensitive { get; set; }
        public string SearchDirection { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public List<TextMatch> Matches { get; set; } = new List<TextMatch>();
        public bool Found { get; set; }
        public int StartIndex { get; set; }
    }
    public class TextAttributeRange : BaseOperationResult { }
    public class TextRangeAttributes : BaseOperationResult { }
    public class TextAttributes : BaseOperationResult 
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
}