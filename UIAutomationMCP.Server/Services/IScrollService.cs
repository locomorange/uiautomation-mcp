namespace UIAutomationMCP.Server.Services
{
    public interface IScrollService
    {
        Task<object> GetScrollInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetScrollPercentAsync(string elementId, double horizontalPercent, double verticalPercent, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}