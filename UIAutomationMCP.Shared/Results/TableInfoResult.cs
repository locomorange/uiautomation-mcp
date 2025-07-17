using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class TableInfoResult : BaseOperationResult
    {
        public string? TableElementId { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<TableRow> Rows { get; set; } = new();
        public List<TableColumn> Columns { get; set; } = new();
        public List<TableHeader> Headers { get; set; } = new();
        public string? TableName { get; set; }
        public string? TableAutomationId { get; set; }
        public string? TableControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> TableProperties { get; set; } = new();
        public bool HasRowHeaders { get; set; }
        public bool HasColumnHeaders { get; set; }
        public bool IsScrollable { get; set; }
        public bool IsSelectable { get; set; }
        public bool CanSelectMultiple { get; set; }
        public string? SelectionMode { get; set; }
        public List<string> SelectedCellIds { get; set; } = new();
        public int TotalCellCount { get; set; }
        public int VisibleCellCount { get; set; }
        public string? RowOrColumnMajor { get; set; }
    }

    public class TableRow
    {
        public string? ElementId { get; set; }
        public int Index { get; set; }
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public List<TableCell> Cells { get; set; } = new();
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsOffscreen { get; set; }
        public Rectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class TableColumn
    {
        public string? ElementId { get; set; }
        public int Index { get; set; }
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public List<TableCell> Cells { get; set; } = new();
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsOffscreen { get; set; }
        public Rectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class TableCell
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
        public string? HeaderText { get; set; }
        public bool IsHeader { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class TableHeader
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