using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IWindowService
    {
        Task<ServerEnhancedResponse<ActionResult>> WindowOperationAsync(string operation, string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> SetWindowStateAsync(string windowState, string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> MoveWindowAsync(int x, int y, string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ResizeWindowAsync(int width, int height, string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<BooleanResult>> WaitForInputIdleAsync(int timeoutMilliseconds = 10000, string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<WindowInteractionStateResult>> GetWindowInteractionStateAsync(string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<WindowCapabilitiesResult>> GetWindowCapabilitiesAsync(string? windowTitle = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}

