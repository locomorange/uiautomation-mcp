using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Worker.Results
{
    /// <summary>
    /// Result for window action operations
    /// </summary>
    public class WindowActionResult : BaseOperationResult 
    {
        public string ActionName { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public IntPtr WindowHandle { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }
}