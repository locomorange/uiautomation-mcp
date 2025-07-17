using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Shared.Results
{
    public class ProcessResult : BaseOperationResult
    {
        public int ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public string? MainWindowTitle { get; set; }
        public string? ApplicationPath { get; set; }
        public string? WorkingDirectory { get; set; }
        public string? Arguments { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsRunning { get; set; }
        public bool HasExited { get; set; }
        public int? ExitCode { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public Dictionary<string, object> ProcessProperties { get; set; } = new();
        public List<WindowInfo> Windows { get; set; } = new();
        public bool MainWindowExists { get; set; }
        public IntPtr MainWindowHandle { get; set; }
        public bool IsMainWindowVisible { get; set; }
        public string? MainWindowState { get; set; }
        public Rectangle MainWindowBounds { get; set; } = new();
        public string? LaunchMethod { get; set; }
        public double LaunchTimeMs { get; set; }
        public bool RequiredElevation { get; set; }
        public string? User { get; set; }
        public string? Domain { get; set; }
        public long WorkingSetSize { get; set; }
        public long VirtualMemorySize { get; set; }
        public long PrivateMemorySize { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public string? PriorityClass { get; set; }
        public bool Responding { get; set; }
        public string? SessionId { get; set; }
        public string? StartInfo { get; set; }
    }
}