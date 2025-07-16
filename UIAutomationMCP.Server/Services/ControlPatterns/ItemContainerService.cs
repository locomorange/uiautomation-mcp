using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Interfaces;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class ItemContainerService : IItemContainerService
    {
        private readonly ILogger<ItemContainerService> _logger;
        private readonly ISubprocessExecutor _executor;

        public ItemContainerService(ILogger<ItemContainerService> logger, ISubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> FindItemByPropertyAsync(string containerId, string? propertyName = null, string? value = null, string? startAfterId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Finding item in container: {ContainerId} with property: {PropertyName}={Value}", containerId, propertyName, value);

                var parameters = new Dictionary<string, object>
                {
                    { "containerId", containerId },
                    { "propertyName", propertyName ?? "" },
                    { "value", value ?? "" },
                    { "startAfterId", startAfterId ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("FindItemByProperty", parameters, timeoutSeconds);

                _logger.LogInformation("Item search completed in container: {ContainerId}", containerId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find item in container: {ContainerId}", containerId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}