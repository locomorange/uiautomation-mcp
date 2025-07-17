using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class GridInfoResult : BaseOperationResult
    {
        public string? GridElementId { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<GridItem> Items { get; set; } = new();
        public List<GridHeader> Headers { get; set; } = new();
        public string? GridName { get; set; }
        public string? GridAutomationId { get; set; }
        public string? GridControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> GridProperties { get; set; } = new();
        public bool HasHeaders { get; set; }
        public bool IsScrollable { get; set; }
        public bool IsSelectable { get; set; }
        public bool CanSelectMultiple { get; set; }
        public string? SelectionMode { get; set; }
        public List<string> SelectedItemIds { get; set; } = new();
        public int TotalItemCount { get; set; }
        public int VisibleItemCount { get; set; }
    }

    public class GridItem
    {
        public string? ElementId { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public string? ControlType { get; set; }
        public string? Value { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsOffscreen { get; set; }
        public Rectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> SupportedPatterns { get; set; } = new();
    }

    public class GridHeader
    {
        public string? ElementId { get; set; }
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public string? ControlType { get; set; }
        public int Index { get; set; }
        public string? HeaderType { get; set; }
        public bool IsClickable { get; set; }
        public bool IsSortable { get; set; }
        public string? SortDirection { get; set; }
        public Rectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}