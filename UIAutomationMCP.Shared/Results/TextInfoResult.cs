using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextInfoResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? Text { get; set; }
        public string? SelectedText { get; set; }
        public int TextLength { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public int SelectionLength { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsPasswordField { get; set; }
        public bool IsMultiline { get; set; }
        public bool CanSelectText { get; set; }
        public bool CanEditText { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> TextProperties { get; set; } = new();
        public List<TextRange> TextRanges { get; set; } = new();
        public string? PlaceholderText { get; set; }
        public int MaxLength { get; set; }
        public string? TextPattern { get; set; }
        public string? InputType { get; set; }
        public bool HasText { get; set; }
        public bool HasSelection { get; set; }
        public int CaretPosition { get; set; }
        public List<string> SupportedTextUnits { get; set; } = new();
        public List<string> SupportedTextAttributes { get; set; } = new();
    }

    public class TextRange
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Length { get; set; }
        public string? Text { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public Rectangle BoundingRectangle { get; set; } = new();
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
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
        public string? TextDecoration { get; set; }
        public string? TextAlign { get; set; }
    }
}