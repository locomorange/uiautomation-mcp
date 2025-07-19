using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextInfoResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string? Text { get; set; }
        public int Length { get; set; }
        public string? SelectedText { get; set; }
        public bool HasSelection { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public int SelectionLength { get; set; }
        public BoundingRectangle SelectionBoundingRectangle { get; set; } = new();
        public Dictionary<string, object> TextAttributes { get; set; } = new();
        public string? Pattern { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanSelectText { get; set; }
        public List<TextRangeAttributes> TextRanges { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public List<string> SupportedTextAttributes { get; set; } = new();
    }
}