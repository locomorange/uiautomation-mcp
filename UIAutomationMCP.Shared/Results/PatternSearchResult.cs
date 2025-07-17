using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class PatternSearchResult : CollectionOperationResult<ElementInfo>
    {
        public string? PatternName { get; set; }
        public string? SearchCriteria { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public int ElementsFound { get; set; }
        public string? SearchScope { get; set; }
        public Dictionary<string, object> SearchParameters { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public DateTime SearchExecutedAt { get; set; } = DateTime.UtcNow;
        public bool SearchCompleted { get; set; }
        public string? SearchError { get; set; }
        public string? PatternSearched { get; set; }
        public bool ValidationPerformed { get; set; }
        
        // Alias for backwards compatibility
        public List<ElementInfo> Elements 
        { 
            get => Items;
            set => Items = value ?? new List<ElementInfo>();
        }
    }
}