using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class SubprocessBasedCalendarService : ICalendarService
    {
        private readonly ILogger<SubprocessBasedCalendarService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedCalendarService(ILogger<SubprocessBasedCalendarService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> CalendarOperationAsync(string elementId, string operation, DateTime? date = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Calendar operation: {Operation} on element: {ElementId}", operation, elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "operation", operation },
                    { "date", date?.ToString("yyyy-MM-dd") ?? "" },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("CalendarOperation", parameters, timeoutSeconds);

                _logger.LogInformation("Calendar operation completed successfully: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform Calendar operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
