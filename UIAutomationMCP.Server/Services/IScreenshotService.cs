using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;

namespace UIAutomationMCP.Server.Services
{
    public interface IScreenshotService
    {
        Task<ServerEnhancedResponse<ScreenshotResult>> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }
}