using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IWindowService
    {
        Task<object> WindowOperationAsync(string operation, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetWindowStateAsync(string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetWindowStateAsync(string windowState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> MoveWindowAsync(int x, int y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ResizeWindowAsync(int width, int height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}