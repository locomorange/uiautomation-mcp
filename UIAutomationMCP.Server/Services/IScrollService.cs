using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IScrollService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetScrollPercentAsync(string elementId, double horizontalPercent, double verticalPercent, string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}