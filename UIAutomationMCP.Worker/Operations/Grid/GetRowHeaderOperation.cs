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
    public class GetRowHeaderOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetRowHeaderOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new ElementSearchResult();
            
            try
            {
                var typedRequest = request.GetTypedRequest<GetRowHeaderRequest>(_options);
                if (typedRequest == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Invalid request format", Data = result });
                
                var elementId = typedRequest.ElementId;
                var windowTitle = typedRequest.WindowTitle;
                var processId = typedRequest.ProcessId ?? 0;
                var row = typedRequest.Row;

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found", Data = result });

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "GridPattern not supported", Data = result });

                // Check if row is within bounds
                if (row >= gridPattern.Current.RowCount)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Row index out of range", Data = result });

                // Try to get the first item in the specified row (assuming header is at column 0)
                var headerElement = gridPattern.GetItem(row, 0);
                if (headerElement == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "No header element found at specified row", Data = result });

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

                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = $"Error getting row header: {ex.Message}", Data = result });
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