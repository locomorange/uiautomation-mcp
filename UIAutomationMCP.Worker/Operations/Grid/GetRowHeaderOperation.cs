using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
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

        public async Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new ElementSearchResult();
            
            try
            {
                var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                var row = request.Parameters?.GetValueOrDefault("row")?.ToString() is string rowStr && 
                    int.TryParse(rowStr, out var parsedRow) ? parsedRow : 0;

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found", Data = result };

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "GridPattern not supported", Data = result };

                // Check if row is within bounds
                if (row >= gridPattern.Current.RowCount)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "Row index out of range", Data = result };

                // Try to get the first item in the specified row (assuming header is at column 0)
                var headerElement = gridPattern.GetItem(row, 0);
                if (headerElement == null)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "No header element found at specified row", Data = result };

                var headerInfo = new ElementInfo
                {
                    AutomationId = headerElement.Current.AutomationId,
                    Name = headerElement.Current.Name,
                    ControlType = headerElement.Current.ControlType.LocalizedControlType,
                    IsEnabled = headerElement.Current.IsEnabled,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = headerElement.Current.BoundingRectangle.X,
                        Y = headerElement.Current.BoundingRectangle.Y,
                        Width = headerElement.Current.BoundingRectangle.Width,
                        Height = headerElement.Current.BoundingRectangle.Height
                    }
                };

                result.Elements.Add(headerInfo);
                result.SearchCriteria = new SearchCriteria
                {
                    AdditionalCriteria = new Dictionary<string, object>
                    {
                        ["Row"] = row,
                        ["Column"] = 0
                    }
                };

                return new OperationResult<ElementSearchResult> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<ElementSearchResult> { Success = false, Error = $"Error getting row header: {ex.Message}", Data = result };
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }
    }
}