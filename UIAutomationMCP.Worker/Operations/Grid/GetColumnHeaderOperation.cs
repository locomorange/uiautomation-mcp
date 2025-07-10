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

            if (!element.TryGetCurrentPattern(GridItemPattern.Pattern, out var pattern) || pattern is not GridItemPattern gridItemPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "GridItemPattern not supported" });

            try
            {
                var columnHeaders = gridItemPattern.Current.ContainingGrid.GetColumnHeaders();
                if (columnHeaders == null || columnHeaders.Length == 0)
                    return Task.FromResult(new OperationResult { Success = false, Error = "No column headers found" });

                if (column >= columnHeaders.Length)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Column index out of range" });

                var columnHeader = columnHeaders[column];
                var headerInfo = new Dictionary<string, object>
                {
                    ["AutomationId"] = columnHeader.Current.AutomationId,
                    ["Name"] = columnHeader.Current.Name,
                    ["ControlType"] = columnHeader.Current.ControlType.LocalizedControlType,
                    ["IsEnabled"] = columnHeader.Current.IsEnabled,
                    ["BoundingRectangle"] = new BoundingRectangle
                    {
                        X = columnHeader.Current.BoundingRectangle.X,
                        Y = columnHeader.Current.BoundingRectangle.Y,
                        Width = columnHeader.Current.BoundingRectangle.Width,
                        Height = columnHeader.Current.BoundingRectangle.Height
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