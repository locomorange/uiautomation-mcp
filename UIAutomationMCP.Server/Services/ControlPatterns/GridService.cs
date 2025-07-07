using System;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services.ControlPatterns
{
    public class GridService : IGridService
    {
        private readonly ILogger<GridService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public GridService(ILogger<GridService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper, ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        public async Task<object> GetGridInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting grid information for element: {ElementId}", elementId);

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
                    if (element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPatternObj) &&
                        gridPatternObj is GridPattern gridPattern)
                    {
                        var rowCount = gridPattern.Current.RowCount;
                        var columnCount = gridPattern.Current.ColumnCount;

                        return new
                        {
                            elementId,
                            rowCount,
                            columnCount,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support GridPattern");
                    }
                }, timeoutSeconds, $"GetGridInfo_{elementId}");

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

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(gridElementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{gridElementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{gridElementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPatternObj) &&
                        gridPatternObj is GridPattern gridPattern)
                    {
                        var gridItem = gridPattern.GetItem(row, column);
                        if (gridItem == null)
                        {
                            throw new InvalidOperationException($"No item found at row {row}, column {column}");
                        }

                        var itemInfo = _elementInfoExtractor.ExtractElementInfo(gridItem);

                        return new
                        {
                            gridElementId,
                            row,
                            column,
                            item = itemInfo,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support GridPattern");
                    }
                }, timeoutSeconds, $"GetGridItem_{gridElementId}");

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

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(gridElementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{gridElementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{gridElementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj) &&
                        tablePatternObj is TablePattern tablePattern)
                    {
                        var rowHeaders = tablePattern.Current.GetRowHeaders();
                        if (row < 0 || row >= rowHeaders.Length)
                        {
                            throw new InvalidOperationException($"Row {row} is out of bounds. Available rows: 0-{rowHeaders.Length - 1}");
                        }

                        var headerInfo = _elementInfoExtractor.ExtractElementInfo(rowHeaders[row]);

                        return new
                        {
                            gridElementId,
                            row,
                            header = headerInfo,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TablePattern for row headers");
                    }
                }, timeoutSeconds, $"GetRowHeader_{gridElementId}");

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

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(gridElementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{gridElementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{gridElementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj) &&
                        tablePatternObj is TablePattern tablePattern)
                    {
                        var columnHeaders = tablePattern.Current.GetColumnHeaders();
                        if (column < 0 || column >= columnHeaders.Length)
                        {
                            throw new InvalidOperationException($"Column {column} is out of bounds. Available columns: 0-{columnHeaders.Length - 1}");
                        }

                        var headerInfo = _elementInfoExtractor.ExtractElementInfo(columnHeaders[column]);

                        return new
                        {
                            gridElementId,
                            column,
                            header = headerInfo,
                            timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException("Element does not support TablePattern for column headers");
                    }
                }, timeoutSeconds, $"GetColumnHeader_{gridElementId}");

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
