using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    public class GridOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public GridOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }
        public OperationResult GetGridInfo(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                return new OperationResult { Success = false, Error = "GridPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var rowCount = gridPattern.Current.RowCount;
            var columnCount = gridPattern.Current.ColumnCount;

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    RowCount = rowCount,
                    ColumnCount = columnCount,
                    CanSelectMultiple = element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selPattern) && 
                                       selPattern is SelectionPattern sp && sp.Current.CanSelectMultiple
                }
            };
        }

        public OperationResult GetGridItem(string gridElementId, int row, int column, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(gridElementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Grid element not found" };

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                return new OperationResult { Success = false, Error = "GridPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var item = gridPattern.GetItem(row, column);
            if (item == null)
                return new OperationResult { Success = false, Error = $"Grid item at [{row}, {column}] not found" };

            return new OperationResult
            {
                Success = true,
                Data = new
                {
                    AutomationId = item.Current.AutomationId,
                    Name = item.Current.Name,
                    ControlType = item.Current.ControlType.LocalizedControlType,
                    IsEnabled = item.Current.IsEnabled,
                    Row = row,
                    Column = column
                }
            };
        }

        public OperationResult GetRowHeader(string gridElementId, int row, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(gridElementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Grid element not found" };

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return new OperationResult { Success = false, Error = "TablePattern not supported for row headers" };

            // Let exceptions flow naturally - no try-catch
            var rowHeaders = tablePattern.Current.GetRowHeaders();
            if (row >= 0 && row < rowHeaders.Length)
            {
                var headerElement = rowHeaders[row];
                return new OperationResult
                {
                    Success = true,
                    Data = new
                    {
                        AutomationId = headerElement.Current.AutomationId,
                        Name = headerElement.Current.Name,
                        Text = headerElement.Current.Name,
                        Row = row
                    }
                };
            }
            else
            {
                return new OperationResult { Success = false, Error = $"Row header at index {row} not found" };
            }
        }

        public OperationResult GetColumnHeader(string gridElementId, int column, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(gridElementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Grid element not found" };

            if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                return new OperationResult { Success = false, Error = "TablePattern not supported for column headers" };

            // Let exceptions flow naturally - no try-catch
            var columnHeaders = tablePattern.Current.GetColumnHeaders();
            if (column >= 0 && column < columnHeaders.Length)
            {
                var headerElement = columnHeaders[column];
                return new OperationResult
                {
                    Success = true,
                    Data = new
                    {
                        AutomationId = headerElement.Current.AutomationId,
                        Name = headerElement.Current.Name,
                        Text = headerElement.Current.Name,
                        Column = column
                    }
                };
            }
            else
            {
                return new OperationResult { Success = false, Error = $"Column header at index {column} not found" };
            }
        }

    }
}
