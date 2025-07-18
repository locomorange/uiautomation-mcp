using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IScrollService
    {
        Task<ServerEnhancedResponse<ScrollInfoResult>> GetScrollInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SetScrollPercentAsync(string elementId, double horizontalPercent, double verticalPercent, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}