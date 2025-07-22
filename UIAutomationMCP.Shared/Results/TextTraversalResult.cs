using System.Text.Json.Serialization;

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
        public List<TextRangeAttributes> TextRanges { get; set; } = new();
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
        [JsonPropertyName("direction")]
        public string Direction { get; set; } = string.Empty;
        
        [JsonPropertyName("textUnit")]
        public string TextUnit { get; set; } = string.Empty;
        
        [JsonPropertyName("moveCount")]
        public int MoveCount { get; set; }
        
        [JsonPropertyName("actualMoveCount")]
        public int ActualMoveCount { get; set; }
        
        [JsonPropertyName("startPosition")]
        public int StartPosition { get; set; }
        
        [JsonPropertyName("endPosition")]
        public int EndPosition { get; set; }
        
        [JsonPropertyName("movedText")]
        public string MovedText { get; set; } = string.Empty;
        
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
        
        [JsonPropertyName("movedUnits")]
        public int MovedUnits { get; set; }
        
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
    }
}