using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetColumnHeaderOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetColumnHeaderOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var column = request.Parameters?.GetValueOrDefault("column")?.ToString() is string columnStr && 
                int.TryParse(columnStr, out var parsedColumn) ? parsedColumn : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "GridPattern not supported" });

            try
            {
                // Check if column is within bounds
                if (column >= gridPattern.Current.ColumnCount)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Column index out of range" });

                // Try to get the first item in the specified column (assuming header is at row 0)
                var headerElement = gridPattern.GetItem(0, column);
                if (headerElement == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = "No header element found at specified column" });

                var headerInfo = new Dictionary<string, object>
                {
                    ["AutomationId"] = headerElement.Current.AutomationId,
                    ["Name"] = headerElement.Current.Name,
                    ["ControlType"] = headerElement.Current.ControlType.LocalizedControlType,
                    ["IsEnabled"] = headerElement.Current.IsEnabled,
                    ["Row"] = 0,
                    ["Column"] = column,
                    ["BoundingRectangle"] = new BoundingRectangle
                    {
                        X = headerElement.Current.BoundingRectangle.X,
                        Y = headerElement.Current.BoundingRectangle.Y,
                        Width = headerElement.Current.BoundingRectangle.Width,
                        Height = headerElement.Current.BoundingRectangle.Height
                    }
                };

                return Task.FromResult(new OperationResult { Success = true, Data = headerInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting column header: {ex.Message}" });
            }
        }
    }
}