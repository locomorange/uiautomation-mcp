using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IMultipleViewService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetAvailableViewsAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> SetViewAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetCurrentViewAsync(string? automationId = null, string? name = null, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetViewNameAsync(string? automationId = null, string? name = null, int viewId = 0, string? controlType = null, int? processId = null, int timeoutSeconds = 30);
    }
}
