namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public interface IAnnotationService
    {
        Task<object> GetAnnotationInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetAnnotationTargetAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }
}