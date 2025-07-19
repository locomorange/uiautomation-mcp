using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class SelectedTextResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string? SelectedText { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public int SelectionLength { get; set; }
        public bool HasSelection { get; set; }
        public BoundingRectangle SelectionBoundingRectangle { get; set; } = new();
        public Dictionary<string, object> SelectionAttributes { get; set; } = new();
        public string? Pattern { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanSelectText { get; set; }
        public string? FullText { get; set; }
        public int TextLength { get; set; }
        public List<TextRangeAttributes> TextRanges { get; set; } = new();

        // Missing property for GetTextSelectionOperation
        public List<string> SelectedTexts { get; set; } = new();
    }
}