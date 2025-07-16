namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ISynchronizedInputService
    {
        Task<object> StartListeningAsync(string elementId, string inputType, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> CancelAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}