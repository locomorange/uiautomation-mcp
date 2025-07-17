using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ViewResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public int CurrentView { get; set; }
        public string? CurrentViewName { get; set; }
        public List<int> AvailableViews { get; set; } = new();
        public List<string> AvailableViewNames { get; set; } = new();
        public bool CanSetView { get; set; }
        public string? Pattern { get; set; }
        public Dictionary<string, object> ViewProperties { get; set; } = new();
        public object? ReturnValue { get; set; }
        public bool ViewChanged { get; set; }
        public int PreviousView { get; set; }
        public string? PreviousViewName { get; set; }
        public int ViewId { get; set; }
        public string? ViewName { get; set; }
    }
}