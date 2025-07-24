using System.Text.Json.Serialization;
using System.Linq;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TextSearchResult : BaseOperationResult
    {
        // Element identification properties
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
        
        // Search parameters
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }
        
        [JsonPropertyName("caseSensitive")]
        public bool CaseSensitive { get; set; }
        
        [JsonPropertyName("useRegex")]
        public bool UseRegex { get; set; }
        
        [JsonPropertyName("searchDirection")]
        public string? SearchDirection { get; set; }
        
        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }
        
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }
        
        [JsonPropertyName("searchParameters")]
        public Dictionary<string, object> SearchParameters { get; set; } = new();
        
        // Primary search results
        [JsonPropertyName("matches")]
        public List<TextMatch> Matches { get; set; } = new();
        
        // Computed properties (derived from Matches)
        [JsonPropertyName("textFound")]
        public bool TextFound => Matches.Count > 0;
        
        [JsonPropertyName("matchCount")]
        public int MatchCount => Matches.Count;
        
        // Backward compatibility properties (derived from first match)
        [JsonPropertyName("found")]
        public bool Found => TextFound;
        
        [JsonPropertyName("text")]
        public string? Text => Matches.FirstOrDefault()?.MatchedText;
        
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle BoundingRectangle => Matches.FirstOrDefault()?.BoundingRectangle ?? new BoundingRectangle();
        
        [JsonPropertyName("startIndex")]
        public int StartIndex => Matches.FirstOrDefault()?.StartIndex ?? 0;
        
        [JsonPropertyName("length")]
        public int Length => Matches.FirstOrDefault()?.Length ?? 0;
        
        // Additional properties
        [JsonPropertyName("startPosition")]
        public int StartPosition { get; set; }
        
        [JsonPropertyName("endPosition")]
        public int EndPosition { get; set; }
        
        [JsonPropertyName("fullText")]
        public string? FullText { get; set; }
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