using System;
using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IToggleService
    {
        Task<ServerEnhancedResponse<ActionResult>> ToggleElementAsync(string? automationId = null, string? name = null, string? controlType = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SetToggleStateAsync(string toggleState, string? automationId = null, string? name = null, string? controlType = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
