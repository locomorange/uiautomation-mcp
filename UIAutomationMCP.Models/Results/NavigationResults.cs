namespace UIAutomationMCP.Models.Results
{
    // Navigation and element discovery results

    /// <summary>
    /// Result for tree navigation operations
    /// </summary>
    public class TreeNavigationResult : BaseOperationResult 
    { 
        public List<ElementInfo> Elements { get; set; } = new List<ElementInfo>();
        public string NavigationType { get; set; } = string.Empty;
        public string SourceElementId { get; set; } = string.Empty;
        public int ElementsFound { get; set; }
    }

    /// <summary>
    /// Result for item search operations
    /// </summary>
    public class FindItemResult : BaseOperationResult 
    { 
        public ElementInfo? FoundElement { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public int TotalMatches { get; set; }
        public TimeSpan SearchDuration { get; set; }
    }

    /// <summary>
    /// Result for element detail operations
    /// </summary>
    public class ElementDetailResult : BaseOperationResult 
    { 
        public ElementInfo? Element { get; set; }
        public List<string> SupportedPatterns { get; set; } = new List<string>();
        public List<string> AvailableProperties { get; set; } = new List<string>();
        public ElementState? ElementState { get; set; }
        public BoundingRectangle? BoundingRectangle { get; set; }
    }

    /// <summary>
    /// Result for event monitoring operations
    /// </summary>
    public class EventMonitoringResult : BaseOperationResult 
    { 
        public string EventType { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ElementId { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public int? ProcessId { get; set; }
        public List<TypedEventData> CapturedEvents { get; set; } = new List<TypedEventData>();
    }

    /// <summary>
    /// Result for table information operations
    /// </summary>
    public class TableInfoResult : BaseOperationResult 
    { 
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string RowOrColumnMajor { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<ElementInfo> Headers { get; set; } = new List<ElementInfo>();
        public bool HasRowHeaders { get; set; }
        public bool HasColumnHeaders { get; set; }
    }

    /// <summary>
    /// Result for grid item operations
    /// </summary>
    public class GetGridItemResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public GridItemResult? GridItem { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Grid item information
    /// </summary>
    public class GridItemResult : BaseOperationResult 
    { 
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string ContainingGridId { get; set; } = string.Empty;
        public ElementInfo? Element { get; set; }
    }

    /// <summary>
    /// Result for grid row header operations
    /// </summary>
    public class GetRowHeaderResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public List<ElementInfo> RowHeaders { get; set; } = new List<ElementInfo>();
        public int RowIndex { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result for grid column header operations
    /// </summary>
    public class GetColumnHeaderResult : BaseOperationResult 
    { 
        public string ActionName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public List<ElementInfo> ColumnHeaders { get; set; } = new List<ElementInfo>();
        public int ColumnIndex { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Grid information result
    /// </summary>
    public class GridInfoResult : BaseOperationResult
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string RowOrColumnMajor { get; set; } = string.Empty;
        public bool HasRowHeaders { get; set; }
        public bool HasColumnHeaders { get; set; }
        public bool CanSelectMultiple { get; set; }
    }
}