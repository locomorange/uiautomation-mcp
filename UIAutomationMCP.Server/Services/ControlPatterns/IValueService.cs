using System;
using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IValueService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetValueAsync(string elementId, string value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TextInfoResult>> GetValueAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<BooleanResult>> IsReadOnlyAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
