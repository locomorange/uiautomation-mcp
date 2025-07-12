using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITransformService
    {
        Task<OperationResult> GetTransformCapabilitiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<OperationResult> MoveElementAsync(string elementId, double x, double y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<OperationResult> ResizeElementAsync(string elementId, double width, double height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<OperationResult> RotateElementAsync(string elementId, double degrees, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}