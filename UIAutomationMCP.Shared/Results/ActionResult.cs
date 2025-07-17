using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ActionResult : BaseOperationResult
    {
        public string? Action { get; set; }
        public string? ActionName { get; set; }
        public string? ElementId { get; set; }
        public string? TargetName { get; set; }
        public string? TargetAutomationId { get; set; }
        public string? TargetControlType { get; set; }
        public Dictionary<string, object> ActionParameters { get; set; } = new();
        public Dictionary<string, object> ElementState { get; set; } = new();
        public double ExecutionTimeMs { get; set; }
        public string? WindowTitle { get; set; }
        public int ProcessId { get; set; }
        public bool RequiredRetries { get; set; }
        public int RetryCount { get; set; }
        public string? Pattern { get; set; }
        public string? PatternMethod { get; set; }
        public object? ReturnValue { get; set; }
        public bool StateChanged { get; set; }
        public Dictionary<string, object> BeforeState { get; set; } = new();
        public Dictionary<string, object> AfterState { get; set; } = new();
        public bool Completed { get; set; }
        public string? Details { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}