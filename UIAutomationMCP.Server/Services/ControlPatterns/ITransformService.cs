namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface ITransformService
    {
        Task<object> GetTransformCapabilitiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> MoveElementAsync(string elementId, double x, double y, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> ResizeElementAsync(string elementId, double width, double height, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> RotateElementAsync(string elementId, double degrees, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}