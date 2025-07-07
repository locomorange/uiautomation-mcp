using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class TableService : ITableService
    {
        private readonly ILogger<TableService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public TableService(ILogger<TableService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper, ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        public async Task<object> GetTableInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting table information for element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj) &&
                        tablePatternObj is TablePattern tablePattern)
                    {
                        var rowHeaders = tablePattern.Current.GetRowHeaders();
                        var columnHeaders = tablePattern.Current.GetColumnHeaders();

                        return new
                        {
                            elementId,
                            rowHeadersCount = rowHeaders?.Length ?? 0,
                            columnHeadersCount = columnHeaders?.Length ?? 0,
                            rowOrColumnMajor = tablePattern.Current.RowOrColumnMajor.ToString(),
                            rowHeaders = rowHeaders?.Select(h => _elementInfoExtractor.ExtractElementInfo(h)).ToArray() ?? Array.Empty<object>(),
                            columnHeaders = columnHeaders?.Select(h => _elementInfoExtractor.ExtractElementInfo(h)).ToArray() ?? Array.Empty<object>(),
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TablePattern");
                    }
                }, timeoutSeconds, $"GetTableInfo_{elementId}");

                _logger.LogInformation("Table information retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get table information for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetRowHeadersAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting row headers for element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj) &&
                        tablePatternObj is TablePattern tablePattern)
                    {
                        var rowHeaders = tablePattern.Current.GetRowHeaders();
                        var headerInfos = rowHeaders?.Select((header, index) => new
                        {
                            index,
                            info = _elementInfoExtractor.ExtractElementInfo(header)
                        }).ToArray() ?? Array.Empty<object>();

                        return new
                        {
                            elementId,
                            count = headerInfos.Length,
                            headers = headerInfos,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TablePattern");
                    }
                }, timeoutSeconds, $"GetRowHeaders_{elementId}");

                _logger.LogInformation("Row headers retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get row headers for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetColumnHeadersAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting column headers for element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj) &&
                        tablePatternObj is TablePattern tablePattern)
                    {
                        var columnHeaders = tablePattern.Current.GetColumnHeaders();
                        var headerInfos = columnHeaders?.Select((header, index) => new
                        {
                            index,
                            info = _elementInfoExtractor.ExtractElementInfo(header)
                        }).ToArray() ?? Array.Empty<object>();

                        return new
                        {
                            elementId,
                            count = headerInfos.Length,
                            headers = headerInfos,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TablePattern");
                    }
                }, timeoutSeconds, $"GetColumnHeaders_{elementId}");

                _logger.LogInformation("Column headers retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get column headers for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
