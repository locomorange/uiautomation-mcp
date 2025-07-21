using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ILayoutService
    {
        Task<ServerEnhancedResponse<ActionResult>> ExpandCollapseElementAsync(string? automationId = null, string? name = null, string action = "toggle", string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ScrollElementAsync(string? automationId = null, string? name = null, string direction = "down", double amount = 1.0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ScrollElementIntoViewAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SetScrollPercentAsync(string? automationId = null, string? name = null, double horizontalPercent = -1, double verticalPercent = -1, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> DockElementAsync(string? automationId = null, string? name = null, string dockPosition = "none", string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ScrollInfoResult>> GetScrollInfoAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
