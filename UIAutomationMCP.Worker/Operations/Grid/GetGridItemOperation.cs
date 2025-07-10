using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetGridItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetGridItemOperation(ElementFinderService elementFinderService)
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
            var column = request.Parameters?.GetValueOrDefault("column")?.ToString() is string columnStr && 
                int.TryParse(columnStr, out var parsedColumn) ? parsedColumn : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "GridPattern not supported" });

            try
            {
                var gridItem = gridPattern.GetItem(row, column);
                if (gridItem == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Grid item not found" });

                var itemInfo = new Dictionary<string, object>
                {
                    ["AutomationId"] = gridItem.Current.AutomationId,
                    ["Name"] = gridItem.Current.Name,
                    ["ControlType"] = gridItem.Current.ControlType.LocalizedControlType,
                    ["IsEnabled"] = gridItem.Current.IsEnabled,
                    ["BoundingRectangle"] = new BoundingRectangle
                    {
                        X = gridItem.Current.BoundingRectangle.X,
                        Y = gridItem.Current.BoundingRectangle.Y,
                        Width = gridItem.Current.BoundingRectangle.Width,
                        Height = gridItem.Current.BoundingRectangle.Height
                    }
                };

                return Task.FromResult(new OperationResult { Success = true, Data = itemInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting grid item: {ex.Message}" });
            }
        }
    }
}