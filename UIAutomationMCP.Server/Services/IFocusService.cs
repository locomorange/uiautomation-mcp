using System;
using System.Threading.Tasks;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;

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