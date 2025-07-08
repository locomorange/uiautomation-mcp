using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class SubprocessBasedGridService : IGridService
    {
        private readonly ILogger<SubprocessBasedGridService> _logger;
        private readonly SubprocessExecutor _executor;

        public SubprocessBasedGridService(ILogger<SubprocessBasedGridService> logger, SubprocessExecutor executor)
        {
            _logger = logger;
            _executor = executor;
        }

        public async Task<object> GetGridInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting grid information for element: {ElementId}", elementId);

                var parameters = new Dictionary<string, object>
                {
                    { "elementId", elementId },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetGridInfo", parameters, timeoutSeconds);

                _logger.LogInformation("Grid information retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get grid information for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetGridItemAsync(string gridElementId, int row, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting grid item at row {Row}, column {Column} for element: {ElementId}", row, column, gridElementId);

                var parameters = new Dictionary<string, object>
                {
                    { "gridElementId", gridElementId },
                    { "row", row },
                    { "column", column },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetGridItem", parameters, timeoutSeconds);

                _logger.LogInformation("Grid item retrieved successfully for element: {ElementId}", gridElementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get grid item at row {Row}, column {Column} for element {ElementId}", row, column, gridElementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetRowHeaderAsync(string gridElementId, int row, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting row header for row {Row} in element: {ElementId}", row, gridElementId);

                var parameters = new Dictionary<string, object>
                {
                    { "gridElementId", gridElementId },
                    { "row", row },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetRowHeader", parameters, timeoutSeconds);

                _logger.LogInformation("Row header retrieved successfully for element: {ElementId}", gridElementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get row header for row {Row} in element {ElementId}", row, gridElementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetColumnHeaderAsync(string gridElementId, int column, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting column header for column {Column} in element: {ElementId}", column, gridElementId);

                var parameters = new Dictionary<string, object>
                {
                    { "gridElementId", gridElementId },
                    { "column", column },
                    { "windowTitle", windowTitle ?? "" },
                    { "processId", processId ?? 0 }
                };

                var result = await _executor.ExecuteAsync<object>("GetColumnHeader", parameters, timeoutSeconds);

                _logger.LogInformation("Column header retrieved successfully for element: {ElementId}", gridElementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get column header for column {Column} in element {ElementId}", column, gridElementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}