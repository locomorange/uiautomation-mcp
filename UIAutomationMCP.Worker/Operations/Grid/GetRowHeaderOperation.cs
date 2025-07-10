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

            if (!element.TryGetCurrentPattern(GridItemPattern.Pattern, out var pattern) || pattern is not GridItemPattern gridItemPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "GridItemPattern not supported" });

            try
            {
                var rowHeaders = gridItemPattern.Current.ContainingGrid.GetRowHeaders();
                if (rowHeaders == null || rowHeaders.Length == 0)
                    return Task.FromResult(new OperationResult { Success = false, Error = "No row headers found" });

                if (row >= rowHeaders.Length)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Row index out of range" });

                var rowHeader = rowHeaders[row];
                var headerInfo = new Dictionary<string, object>
                {
                    ["AutomationId"] = rowHeader.Current.AutomationId,
                    ["Name"] = rowHeader.Current.Name,
                    ["ControlType"] = rowHeader.Current.ControlType.LocalizedControlType,
                    ["IsEnabled"] = rowHeader.Current.IsEnabled,
                    ["BoundingRectangle"] = new BoundingRectangle
                    {
                        X = rowHeader.Current.BoundingRectangle.X,
                        Y = rowHeader.Current.BoundingRectangle.Y,
                        Width = rowHeader.Current.BoundingRectangle.Width,
                        Height = rowHeader.Current.BoundingRectangle.Height
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