using UIAutomationMCP.Models.Abstractions;
using System;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IValueService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetValueAsync(string value, string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TextInfoResult>> GetValueAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}
