using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services
{
    public interface IMultipleViewService
    {
        Task<object> GetAvailableViewsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetViewAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetCurrentViewAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetViewNameAsync(string elementId, int viewId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
