using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class SubprocessBasedListService : IListService
    {
        private readonly ILogger<SubprocessBasedListService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedListService(ILogger<SubprocessBasedListService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> ListOperationAsync(string elementId, string operation, string? itemName = null, int? itemIndex = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing List operation: {Operation} on element: {ElementId}", operation, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "operation", operation },
                    { "itemName", itemName ?? "" },
                    { "itemIndex", itemIndex ?? -1 },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("ListOperation", parameters, timeoutSeconds);

                _logger.LogInformation("List operation completed successfully: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform List operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}