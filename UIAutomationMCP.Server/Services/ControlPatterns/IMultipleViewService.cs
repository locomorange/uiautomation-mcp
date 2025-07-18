using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IMultipleViewService
    {
        Task<ServerEnhancedResponse<ElementSearchResult>> GetAvailableViewsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> SetViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetCurrentViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ElementSearchResult>> GetViewNameAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
