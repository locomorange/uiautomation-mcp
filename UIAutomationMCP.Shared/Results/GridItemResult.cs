using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class GridItemResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string? ContainerElementId { get; set; }
        public string? ContainerName { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsOffscreen { get; set; }
        public Rectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
        public ElementInfo? Element { get; set; }
        public string? Value { get; set; }
        public string? Pattern { get; set; }
    }
}