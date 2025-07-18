using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IToggleService
    {
        Task<ServerEnhancedResponse<ActionResult>> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ToggleStateResult>> GetToggleStateAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SetToggleStateAsync(string elementId, string toggleState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
