using System;
using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IValueService
    {
        Task<ServerEnhancedResponse<ActionResult>> SetValueAsync(string value, string? automationId = null, string? name = null, string? controlType = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TextInfoResult>> GetValueAsync(string? automationId = null, string? name = null, string? controlType = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<BooleanResult>> IsReadOnlyAsync(string? automationId = null, string? name = null, string? controlType = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
