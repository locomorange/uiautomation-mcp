using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Result for element search operations
    /// </summary>
    [MessagePackObject]
    public class SearchElementsResult : BaseOperationResult 
    {
        [Key(10)] // Start after base class keys
        public ElementInfo[] Elements { get; set; } = Array.Empty<ElementInfo>();
        
        [Key(11)]
        public new SearchMetadata Metadata { get; set; } = new SearchMetadata();
    }

    /// <summary>
    /// Metadata for search operations
    /// </summary>
    [MessagePackObject]
    public class SearchMetadata 
    {
        [Key(0)]
        public int TotalFound { get; set; }
        
        [Key(1)]
        public int Returned { get; set; }
        
        [Key(2)]
        public TimeSpan SearchDuration { get; set; }
        
        [Key(3)]
        public string SearchCriteria { get; set; } = string.Empty;
        
        [Key(4)]
        public bool WasTruncated { get; set; }
        
        [Key(5)]
        public string[] SuggestedRefinements { get; set; } = Array.Empty<string>();
        
        [Key(6)]
        public DateTime ExecutedAt { get; set; }
    }

    /// <summary>
    /// Advanced search parameters
    /// </summary>
    [MessagePackObject]
    public class AdvancedSearchParameters 
    {
        [Key(0)]
        public string SearchText { get; set; } = string.Empty;
        
        [Key(1)]
        public string AutomationId { get; set; } = string.Empty;
        
        [Key(2)]
        public string Name { get; set; } = string.Empty;
        
        [Key(3)]
        public string ClassName { get; set; } = string.Empty;
        
        [Key(4)]
        public string ControlType { get; set; } = string.Empty;
        
        [Key(5)]
        public string WindowTitle { get; set; } = string.Empty;
        
        [Key(6)]
        public int? ProcessId { get; set; }
        
        [Key(7)]
        public bool FuzzyMatch { get; set; }
        
        [Key(8)]
        public string[] RequiredPatterns { get; set; } = Array.Empty<string>();
        
        [Key(9)]
        public string Scope { get; set; } = "Descendants";
        
        [Key(10)]
        public bool VisibleOnly { get; set; } = true;
        
        [Key(11)]
        public bool EnabledOnly { get; set; } = false;
        
        [Key(12)]
        public string SortBy { get; set; } = string.Empty;
        
        [Key(13)]
        public string CacheRequest { get; set; } = string.Empty;
        
        [Key(14)]
        public string[] AnyOfPatterns { get; set; } = Array.Empty<string>();
    }
}