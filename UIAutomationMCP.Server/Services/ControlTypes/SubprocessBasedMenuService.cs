using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class SubprocessBasedMenuService : IMenuService
    {
        private readonly ILogger<SubprocessBasedMenuService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedMenuService(ILogger<SubprocessBasedMenuService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> MenuOperationAsync(string menuPath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Menu operation: {MenuPath}", menuPath);

                var parameters = new Dictionary<string, object>
                {
                    { "menuPath", menuPath },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("MenuOperation", parameters, timeoutSeconds);

                _logger.LogInformation("Menu operation completed successfully: {MenuPath}", menuPath);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform Menu operation: {MenuPath}", menuPath);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
