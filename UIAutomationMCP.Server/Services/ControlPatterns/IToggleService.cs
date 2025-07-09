using System.Threading.Tasks;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IToggleService
    {
        Task<object> ToggleElementAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetToggleStateAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> SetToggleStateAsync(string elementId, string toggleState, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}
