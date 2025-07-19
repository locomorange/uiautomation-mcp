namespace UIAutomationMCP.Shared.Results
{
    public class SearchCriteria
    {
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public string? ClassName { get; set; }
        public string? ControlType { get; set; }
        public string? ProcessName { get; set; }
        public int? ProcessId { get; set; }
        public string? WindowTitle { get; set; }
        public string? Text { get; set; }
        public string? HelpText { get; set; }
        public string? AcceleratorKey { get; set; }
        public string? AccessKey { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsVisible { get; set; }
        public bool? IsKeyboardFocusable { get; set; }
        public bool? HasKeyboardFocus { get; set; }
        public bool? IsPassword { get; set; }
        public bool? IsOffscreen { get; set; }
        public string? FrameworkId { get; set; }
        public string? ItemType { get; set; }
        public string? ItemStatus { get; set; }
        public string? LocalizedControlType { get; set; }
        public string? Orientation { get; set; }
        public string? LiveSetting { get; set; }
        public BoundingRectangle? BoundingRectangle { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public string? SearchScope { get; set; }
        public int? MaxResults { get; set; }
        public TimeSpan? Timeout { get; set; }
        public bool UseRegex { get; set; }
        public bool CaseSensitive { get; set; }
        public bool IncludeInvisible { get; set; }
        public bool IncludeOffscreen { get; set; }
        public string? PatternType { get; set; }
        public bool UsePatternMatching { get; set; }
        public string? Scope { get; set; }
        public Dictionary<string, object> AdditionalCriteria { get; set; } = new();
        public string? SearchText { get; set; }
    }
}