using System.Threading.Tasks;
using UIAutomationMCP.Shared.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITransformService
    {
        Task<ServerEnhancedResponse<ActionResult>> MoveElementAsync(string elementId, double x, double y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ResizeElementAsync(string elementId, double width, double height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> RotateElementAsync(string elementId, double degrees, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}