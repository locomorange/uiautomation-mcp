using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetRowHeaderItemsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetRowHeaderItemsOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TableItemPattern.Pattern, out var pattern) || pattern is not TableItemPattern tableItemPattern)
                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "TableItemPattern not supported" });

            try
            {
                var rowHeaderItems = tableItemPattern.Current.GetRowHeaderItems();
                if (rowHeaderItems == null || rowHeaderItems.Length == 0)
                    return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = "No row header items found" });

                var elements = new List<ElementInfo>();
                foreach (var header in rowHeaderItems)
                {
                    elements.Add(new ElementInfo
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
                    });
                }

                var result = new ElementSearchResult
                {
                    Elements = elements
                };

                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ElementSearchResult> { Success = false, Error = $"Error getting row header items: {ex.Message}" });
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