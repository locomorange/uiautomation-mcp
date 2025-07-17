using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class SelectionInfoResult : BaseOperationResult
    {
        public string? ContainerElementId { get; set; }
        public List<SelectionItem> SelectedItems { get; set; } = new();
        public bool CanSelectMultiple { get; set; }
        public bool IsSelectionRequired { get; set; }
        public int SelectedCount { get; set; }
        public int TotalCount { get; set; }
        public string? SelectionMode { get; set; }
        public string? ContainerName { get; set; }
        public string? ContainerAutomationId { get; set; }
        public string? ContainerControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> SelectionProperties { get; set; } = new();
        public bool HasSelection { get; set; }
        public string? FirstSelectedItemId { get; set; }
        public string? LastSelectedItemId { get; set; }
        public List<string> SelectedItemIds { get; set; } = new();
        public List<string> AvailableItemIds { get; set; } = new();
    }

    public class SelectionItem
    {
        public string? ElementId { get; set; }
        public string? Name { get; set; }
        public string? AutomationId { get; set; }
        public string? ControlType { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSelectable { get; set; }
        public int Index { get; set; }
        public string? Value { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public Rectangle BoundingRectangle { get; set; } = new();
        public bool IsEnabled { get; set; }
        public bool IsKeyboardFocusable { get; set; }
        public bool HasKeyboardFocus { get; set; }
        public bool IsOffscreen { get; set; }
    }
}