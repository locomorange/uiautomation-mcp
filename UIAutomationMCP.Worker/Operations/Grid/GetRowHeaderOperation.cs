using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetRowHeaderOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetRowHeaderOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var row = request.Parameters?.GetValueOrDefault("row")?.ToString() is string rowStr && 
                int.TryParse(rowStr, out var parsedRow) ? parsedRow : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "GridPattern not supported" });

            try
            {
                // Check if row is within bounds
                if (row >= gridPattern.Current.RowCount)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Row index out of range" });

                // Try to get the first item in the specified row (assuming header is at column 0)
                var headerElement = gridPattern.GetItem(row, 0);
                if (headerElement == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = "No header element found at specified row" });

                var headerInfo = new Dictionary<string, object>
                {
                    ["AutomationId"] = headerElement.Current.AutomationId,
                    ["Name"] = headerElement.Current.Name,
                    ["ControlType"] = headerElement.Current.ControlType.LocalizedControlType,
                    ["IsEnabled"] = headerElement.Current.IsEnabled,
                    ["Row"] = row,
                    ["Column"] = 0,
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
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting row header: {ex.Message}" });
            }
        }
    }
}