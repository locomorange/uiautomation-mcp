using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;
using UIAutomationMCP.Server.Models;

namespace UIAutomationMCP.Server.Services
{
    public class SubprocessBasedScreenshotService : IScreenshotService
    {
        private readonly ILogger<SubprocessBasedScreenshotService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedScreenshotService(ILogger<SubprocessBasedScreenshotService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<ScreenshotResult> TakeScreenshotAsync(string? windowTitle = null, string? outputPath = null, int maxTokens = 0, int? processId = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Taking screenshot of window: {WindowTitle}, maxTokens: {MaxTokens}", windowTitle, maxTokens);

                var parameters = new Dictionary<string, object>
                {
                    { "windowTitle", windowTitle ?? "" },
                    { "outputPath", outputPath ?? "" },
                    { "maxTokens", maxTokens },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<ScreenshotResult>("TakeScreenshot", parameters, timeoutSeconds);

                if (result != null && result.Success)
                {
                    _logger.LogInformation("Screenshot taken successfully for window: {WindowTitle}", windowTitle);
                    return result;
                }
                else
                {
                    _logger.LogError("Screenshot failed for window: {WindowTitle}", windowTitle);
                    return result ?? new ScreenshotResult { Success = false, Error = "Subprocess returned null result" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to take screenshot for window: {WindowTitle}", windowTitle);
                return new ScreenshotResult { Success = false, Error = ex.Message };
            }
        }
    }
}