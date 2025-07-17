using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class WaitForInputIdleResult : BaseOperationResult
    {
        public int ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public string? WindowTitle { get; set; }
        public TimeSpan WaitDuration { get; set; }
        public TimeSpan TimeoutDuration { get; set; }
        public bool TimedOut { get; set; }
        public bool InputIdle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Dictionary<string, object> ProcessState { get; set; } = new();
        public string? WaitReason { get; set; }
        public bool ProcessStillRunning { get; set; }
        public int ThreadCount { get; set; }
        public long WorkingSetSize { get; set; }
        public bool HasExited { get; set; }
        public int ExitCode { get; set; }
        public string? ActionName { get; set; }
        public bool Completed { get; set; }
        public int TimeoutMilliseconds { get; set; }
        public int ElapsedMilliseconds { get; set; }
        public string? Message { get; set; }
        public string? WindowInteractionState { get; set; }
    }
}