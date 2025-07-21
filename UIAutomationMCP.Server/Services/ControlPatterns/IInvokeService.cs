using System;
using System.Threading.Tasks;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IInvokeService
    {
        Task<ServerEnhancedResponse<ActionResult>> InvokeElementAsync(string? automationId = null, string? name = null, string? controlType = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
