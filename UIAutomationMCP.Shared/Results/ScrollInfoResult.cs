using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ScrollInfoResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
        public double HorizontalScrollPercent { get; set; }
        public double VerticalScrollPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public Dictionary<string, object> ScrollProperties { get; set; } = new();
        public bool CanScrollHorizontally { get; set; }
        public bool CanScrollVertically { get; set; }
        public double MaxHorizontalScrollPercent { get; set; }
        public double MaxVerticalScrollPercent { get; set; }
        public double MinHorizontalScrollPercent { get; set; }
        public double MinVerticalScrollPercent { get; set; }
        public string? ScrollDirection { get; set; }
        public double ScrollAmount { get; set; }
    }
}