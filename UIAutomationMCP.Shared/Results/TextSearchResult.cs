using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextSearchResult : BaseOperationResult
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
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }
        
        [JsonPropertyName("textFound")]
        public bool TextFound { get; set; }
        
        [JsonPropertyName("matches")]
        public List<TextMatch> Matches { get; set; } = new();
        
        [JsonPropertyName("matchCount")]
        public int MatchCount { get; set; }
        
        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("searchParameters")]
        public Dictionary<string, object> SearchParameters { get; set; } = new();
        
        [JsonPropertyName("caseSensitive")]
        public bool CaseSensitive { get; set; }
        
        [JsonPropertyName("useRegex")]
        public bool UseRegex { get; set; }
        
        [JsonPropertyName("searchDirection")]
        public string? SearchDirection { get; set; }
        
        [JsonPropertyName("startPosition")]
        public int StartPosition { get; set; }
        
        [JsonPropertyName("endPosition")]
        public int EndPosition { get; set; }
        
        [JsonPropertyName("fullText")]
        public string? FullText { get; set; }
        
        [JsonPropertyName("found")]
        public bool Found { get; set; }
        
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; }
        
        [JsonPropertyName("length")]
        public int Length { get; set; }
    }

    public class TextMatch
    {
        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; }
        
        [JsonPropertyName("endIndex")]
        public int EndIndex { get; set; }
        
        [JsonPropertyName("length")]
        public int Length { get; set; }
        
        [JsonPropertyName("matchedText")]
        public string? MatchedText { get; set; }
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        
        [JsonPropertyName("attributes")]
        public Dictionary<string, object> Attributes { get; set; } = new();
        
        [JsonPropertyName("isHighlighted")]
        public bool IsHighlighted { get; set; }
        
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
    }
}