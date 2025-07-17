using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Interfaces
{
    /// <summary>
    /// Strongly typed screenshot service interface
    /// </summary>
    public interface ITypedScreenshotService
    {
        Task<UIAutomationMCP.Shared.ScreenshotResult> TakeScreenshotAsync(
            string? windowTitle = null,
            int? processId = null,
            string? outputPath = null,
            int maxTokens = 0,
            int timeoutSeconds = 60);

        Task<UIAutomationMCP.Shared.ScreenshotResult> TakeElementScreenshotAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            string? outputPath = null,
            int maxTokens = 0,
            int timeoutSeconds = 60);

        Task<UIAutomationMCP.Shared.ScreenshotResult> TakeWindowScreenshotAsync(
            string? windowTitle = null,
            int? processId = null,
            string? outputPath = null,
            int maxTokens = 0,
            int timeoutSeconds = 60);
    }
}