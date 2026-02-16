using UIAutomationMCP.Models.Abstractions;
using System;
using System.Threading.Tasks;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IInvokeService
    {
        Task<ServerEnhancedResponse<ActionResult>> InvokeElementAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}

