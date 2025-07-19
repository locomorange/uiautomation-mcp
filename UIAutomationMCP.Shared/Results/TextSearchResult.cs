using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextSearchResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string? SearchText { get; set; }
        public bool TextFound { get; set; }
        public List<TextMatch> Matches { get; set; } = new();
        public int MatchCount { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public string? Pattern { get; set; }
        public Dictionary<string, object> SearchParameters { get; set; } = new();
        public bool CaseSensitive { get; set; }
        public bool UseRegex { get; set; }
        public string? SearchDirection { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string? FullText { get; set; }
        public bool Found { get; set; }
        public string? Text { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public int StartIndex { get; set; }
        public int Length { get; set; }
    }

    public class TextMatch
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get; set; }
        public string? MatchedText { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, object> Attributes { get; set; } = new();
        public bool IsHighlighted { get; set; }
        public bool IsSelected { get; set; }
    }
}