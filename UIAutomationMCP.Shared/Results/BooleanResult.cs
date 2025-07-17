using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class BooleanResult : BaseOperationResult
    {
        public bool Value { get; set; }
        public string? PropertyName { get; set; }
        public string? ElementId { get; set; }
        public string? ElementName { get; set; }
        public string? ElementAutomationId { get; set; }
        public string? ElementControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public string? Pattern { get; set; }
        public string? Method { get; set; }
        public Dictionary<string, object> Context { get; set; } = new();
        public string? Description { get; set; }
    }
}