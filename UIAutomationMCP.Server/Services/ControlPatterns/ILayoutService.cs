using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ILayoutService
    {
        Task<ServerEnhancedResponse<ActionResult>> ExpandCollapseElementAsync(string elementId, string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ScrollElementAsync(string elementId, string direction, double amount = 1.0, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SetScrollPercentAsync(string elementId, double horizontalPercent, double verticalPercent, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> DockElementAsync(string elementId, string dockPosition, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ScrollInfoResult>> GetScrollInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
