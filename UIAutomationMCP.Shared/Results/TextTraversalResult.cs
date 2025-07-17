using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextTraversalResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string? TextContent { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public int TextLength { get; set; }
        public string? TraversalDirection { get; set; }
        public string? TextUnit { get; set; }
        public List<TextRange> TextRanges { get; set; } = new();
        public string? SelectedText { get; set; }
        public bool HasSelection { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public Dictionary<string, object> TextAttributes { get; set; } = new();
        public bool IsReadOnly { get; set; }
        public string? Pattern { get; set; }
        public List<TextMoveInfo> MoveResults { get; set; } = new();
    }

    public class TextMoveInfo
    {
        public string Direction { get; set; } = string.Empty;
        public string TextUnit { get; set; } = string.Empty;
        public int MoveCount { get; set; }
        public int ActualMoveCount { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string MovedText { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int MovedUnits { get; set; }
        public string Text { get; set; } = string.Empty;
        public BoundingRectangle BoundingRectangle { get; set; } = new();
    }
}