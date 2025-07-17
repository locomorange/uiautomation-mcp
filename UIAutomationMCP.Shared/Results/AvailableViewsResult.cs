using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class AvailableViewsResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public List<ViewInfo> AvailableViews { get; set; } = new();
        public int CurrentView { get; set; }
        public string? CurrentViewName { get; set; }
        public bool CanSetView { get; set; }
        public string? Pattern { get; set; }
        public Dictionary<string, object> ViewProperties { get; set; } = new();
        public int ViewCount { get; set; }
        public bool HasMultipleViews { get; set; }
        public int CurrentViewId { get; set; }
        public List<ViewInfo> Views { get; set; } = new();
    }

    public class ViewInfo
    {
        public int ViewId { get; set; }
        public string? ViewName { get; set; }
        public bool IsCurrentView { get; set; }
        public bool IsSelectable { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public string? Description { get; set; }
        public bool IsCurrent { get; set; }
    }
}