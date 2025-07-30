using UIAutomationMCP.Models.Abstractions;
using System.Threading.Tasks;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITransformService
    {
        Task<ServerEnhancedResponse<ActionResult>> MoveElementAsync(string? automationId = null, string? name = null, double x = 0, double y = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> ResizeElementAsync(string? automationId = null, string? name = null, double width = 100, double height = 100, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<ActionResult>> RotateElementAsync(string? automationId = null, string? name = null, double degrees = 0, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
        Task<ServerEnhancedResponse<TransformCapabilitiesResult>> GetTransformCapabilitiesAsync(string? automationId = null, string? name = null, string? controlType = null, long? windowHandle = null, int timeoutSeconds = 30);
    }
}