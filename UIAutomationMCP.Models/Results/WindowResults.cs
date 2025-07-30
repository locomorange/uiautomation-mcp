namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// Result for window action operations
    /// </summary>
    public class WindowActionResult : BaseOperationResult 
    {
        public string ActionName { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public long WindowHandle { get; set; }
        public string PreviousState { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public new DateTime ExecutedAt { get; set; }
        public string WindowStyle { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}