namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Metadata for detailed information
    /// </summary>
    public class DetailMetadata 
    { 
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Element detail information
    /// </summary>
    public class ElementDetail 
    { 
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyValue { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
    }

    /// <summary>
    /// Event time range information
    /// </summary>
    public class EventTimeRange 
    { 
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }
}