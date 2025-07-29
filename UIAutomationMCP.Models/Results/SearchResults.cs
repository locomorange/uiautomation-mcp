namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Result for element search operations
    /// </summary>
    public class SearchElementsResult : BaseOperationResult 
    {
        public BasicElementInfo[] Elements { get; set; } = Array.Empty<BasicElementInfo>();
        public new SearchMetadata Metadata { get; set; } = new SearchMetadata();
    }

    /// <summary>
    /// Metadata for search operations
    /// </summary>
    public class SearchMetadata 
    {
        public int TotalFound { get; set; }
        public int Returned { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public string SearchCriteria { get; set; } = string.Empty;
        public bool WasTruncated { get; set; }
        public string[] SuggestedRefinements { get; set; } = Array.Empty<string>();
        public DateTime ExecutedAt { get; set; }
    }

    /// <summary>
    /// Basic element information
    /// </summary>
    public class BasicElementInfo 
    {
        public string AutomationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsOffscreen { get; set; }
        public int ProcessId { get; set; }
        public int? MainProcessId { get; set; }
        public string WindowTitle { get; set; } = string.Empty;
        public BoundingRectangle? BoundingRectangle { get; set; }
    }

    /// <summary>
    /// Advanced search parameters
    /// </summary>
    public class AdvancedSearchParameters 
    {
        public string SearchText { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ControlType { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public bool FuzzyMatch { get; set; }                      
        public string[] RequiredPatterns { get; set; } = Array.Empty<string>();
        public string Scope { get; set; } = "Descendants";
        public bool VisibleOnly { get; set; } = true;
        public bool EnabledOnly { get; set; } = false;
        public string SortBy { get; set; } = string.Empty;
        public string CacheRequest { get; set; } = string.Empty;
        public string[] AnyOfPatterns { get; set; } = Array.Empty<string>();
    }
}