using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetColumnHeaderItemsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetColumnHeaderItemsOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new ElementSearchResult();
            
            try
            {
                // Try to get typed request first, fall back to legacy parameter extraction
                var typedRequest = request.GetTypedRequest<GetColumnHeaderItemsRequest>(_options);
                
                var elementId = typedRequest?.ElementId ?? request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                var windowTitle = typedRequest?.WindowTitle ?? request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                var processId = typedRequest?.ProcessId ?? (request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0);

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found", Data = result });

                TableItemPattern tableItemPattern;
                try
                {
                    tableItemPattern = (TableItemPattern)element.GetCurrentPattern(TableItemPattern.Pattern);
                }
                catch (InvalidOperationException)
                {
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "TableItemPattern not supported", Data = result });
                }

                var columnHeaderItems = tableItemPattern.Current.GetColumnHeaderItems();
                if (columnHeaderItems == null || columnHeaderItems.Length == 0)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "No column header items found", Data = result });

                foreach (var header in columnHeaderItems)
                {
                    var headerInfo = new ElementInfo
                    {
                        AutomationId = header.Current.AutomationId,
                        Name = header.Current.Name,
                        ControlType = header.Current.ControlType.LocalizedControlType,
                        IsEnabled = header.Current.IsEnabled,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = header.Current.BoundingRectangle.X,
                            Y = header.Current.BoundingRectangle.Y,
                            Width = header.Current.BoundingRectangle.Width,
                            Height = header.Current.BoundingRectangle.Height
                        }
                    };
                    result.Elements.Add(headerInfo);
                }

                result.SearchCriteria = "Table column header items search (TableItemPattern)";

                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = $"Error getting column header items: {ex.Message}", Data = result });
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