using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ControlTypeSearchResult : CollectionOperationResult<ElementInfo>
    {
        public string? ControlType { get; set; }
        public string? SearchCriteria { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public int ElementsFound { get; set; }
        public string? SearchScope { get; set; }
        public Dictionary<string, object> SearchParameters { get; set; } = new();
        public DateTime SearchExecutedAt { get; set; } = DateTime.UtcNow;
        public bool SearchCompleted { get; set; }
        public string? SearchError { get; set; }
        public ControlTypeSearchSummary? SearchSummary { get; set; }
        
        // Alias for backwards compatibility
        public List<ElementInfo> Elements 
        { 
            get => Items;
            set => Items = value ?? new List<ElementInfo>();
        }
    }
    
    public class ControlTypeSearchSummary
    {
        public string? ControlType { get; set; }
        public int TotalElements { get; set; }
        public int VisibleElements { get; set; }
        public int EnabledElements { get; set; }
        public Dictionary<string, int> ByProcessCount { get; set; } = new();
        public Dictionary<string, int> ByWindowCount { get; set; } = new();
        public List<string> UniqueAutomationIds { get; set; } = new();
        public List<string> UniqueClassNames { get; set; } = new();
        public string? Scope { get; set; }
        public int TotalFound { get; set; }
        public int ValidElements { get; set; }
        public int InvalidElements { get; set; }
        public int MaxResults { get; set; }
        public bool ValidationEnabled { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }
}