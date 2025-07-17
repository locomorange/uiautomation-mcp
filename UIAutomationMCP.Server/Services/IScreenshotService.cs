using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Services
{
    public interface IScreenshotService
    {
        Task<object> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }
}