using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Table
{
    public class GetRowHeadersOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetRowHeadersOperation(ElementFinderService elementFinderService)
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

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found", Data = result };

                if (!element.TryGetCurrentPattern(TablePattern.Pattern, out var pattern) || pattern is not TablePattern tablePattern)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "TablePattern not supported", Data = result };

                var rowHeaders = tablePattern.Current.GetRowHeaders();
                if (rowHeaders == null || rowHeaders.Length == 0)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "No row headers found", Data = result };

                foreach (var header in rowHeaders)
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

                result.SearchCriteria = new SearchCriteria
                {
                    PatternType = "TablePattern",
                    AdditionalCriteria = new Dictionary<string, object>
                    {
                        ["HeaderType"] = "Row"
                    }
                };

                return new OperationResult<ElementSearchResult> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<ElementSearchResult> { Success = false, Error = $"Error getting row headers: {ex.Message}", Data = result };
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