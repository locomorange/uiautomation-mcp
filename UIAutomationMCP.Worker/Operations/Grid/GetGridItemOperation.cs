using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetGridItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetGridItemOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<GridItemResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new GridItemResult();
            
            try
            {
                var typedRequest = request.GetTypedRequest<GetGridItemRequest>(_options);
                if (typedRequest == null)
                    return Task.FromResult(new OperationResult<GridItemResult> { Success = false, Error = "Invalid request format", Data = result });
                
                var elementId = typedRequest.ElementId;
                var windowTitle = typedRequest.WindowTitle;
                var processId = typedRequest.ProcessId ?? 0;
                var row = typedRequest.Row;
                var column = typedRequest.Column;

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return Task.FromResult(new OperationResult<GridItemResult> { Success = false, Error = "Element not found", Data = result });

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                    return Task.FromResult(new OperationResult<GridItemResult> { Success = false, Error = "GridPattern not supported", Data = result });

                var gridItem = gridPattern.GetItem(row, column);
                if (gridItem == null)
                    return Task.FromResult(new OperationResult<GridItemResult> { Success = false, Error = "Grid item not found", Data = result });

                result.Row = row;
                result.Column = column;
                result.Element = new ElementInfo
                {
                    AutomationId = gridItem.Current.AutomationId,
                    Name = gridItem.Current.Name,
                    ControlType = gridItem.Current.ControlType.LocalizedControlType,
                    IsEnabled = gridItem.Current.IsEnabled,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = gridItem.Current.BoundingRectangle.X,
                        Y = gridItem.Current.BoundingRectangle.Y,
                        Width = gridItem.Current.BoundingRectangle.Width,
                        Height = gridItem.Current.BoundingRectangle.Height
                    }
                };

                return Task.FromResult(new OperationResult<GridItemResult> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<GridItemResult> { Success = false, Error = $"Error getting grid item: {ex.Message}", Data = result });
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