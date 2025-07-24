using System;
using System.Threading.Tasks;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IFocusService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetFocusAsync(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            string? requiredPattern = null, 
            int? processId = null, 
            int timeoutSeconds = 30);
    }
}