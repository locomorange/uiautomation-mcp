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
    public class GetColumnHeaderOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetColumnHeaderOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new ElementSearchResult();
            
            try
            {
                var typedRequest = request.GetTypedRequest<GetColumnHeaderRequest>(_options);
                if (typedRequest == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Invalid request format", Data = result });
                
                var elementId = typedRequest.ElementId;
                var windowTitle = typedRequest.WindowTitle;
                var processId = typedRequest.ProcessId ?? 0;
                var column = typedRequest.Column;

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found", Data = result });

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "GridPattern not supported", Data = result });

                // Check if column is within bounds
                if (column >= gridPattern.Current.ColumnCount)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Column index out of range", Data = result });

                // Try to get the first item in the specified column (assuming header is at row 0)
                var headerElement = gridPattern.GetItem(0, column);
                if (headerElement == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "No header element found at specified column", Data = result });

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
                result.SearchCriteria = "Grid column header search";
                result.SearchDuration = TimeSpan.FromMilliseconds(0);
                
                return Task.FromResult(new OperationResult<ElementSearchResult>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ElementSearchResult>
                {
                    Success = false,
                    Error = $"Failed to get column header: {ex.Message}"
                });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(string parametersJson)
        {
            var typedResult = await ExecuteAsync(parametersJson);
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