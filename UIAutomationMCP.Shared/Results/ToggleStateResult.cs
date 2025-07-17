using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ToggleStateResult : BaseOperationResult
    {
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string? ToggleState { get; set; }
        public bool IsToggled { get; set; }
        public string? Pattern { get; set; }
        public Dictionary<string, object> ElementState { get; set; } = new();
        public bool CanToggle { get; set; }
        public List<string> AvailableStates { get; set; } = new();
        public string? StateDescription { get; set; }
        public bool IsIndeterminate { get; set; }
        public bool IsChecked { get; set; }
        public bool IsPressed { get; set; }
        public string? State { get; set; }
    }
}