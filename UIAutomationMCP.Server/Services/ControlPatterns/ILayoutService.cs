using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ILayoutService
    {
        Task<object> ExpandCollapseElementAsync(string elementId, string action, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ScrollElementAsync(string elementId, string direction, double amount = 1.0, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ScrollElementIntoViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> DockElementAsync(string elementId, string dockPosition, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}