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

        public async Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = new ElementSearchResult();
            
            try
            {
                // Try to parse as typed request first
                var typedRequest = request.GetTypedRequest<GetColumnHeaderRequest>(_options);
                
                string elementId;
                string windowTitle;
                int processId;
                int column;
                
                if (typedRequest != null)
                {
                    elementId = typedRequest.ElementId;
                    windowTitle = typedRequest.WindowTitle;
                    processId = typedRequest.ProcessId ?? 0;
                    column = typedRequest.Column;
                }
                else
                {
                    // Fall back to legacy dictionary method
                    elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                    windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                    processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                        int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                    column = request.Parameters?.GetValueOrDefault("column")?.ToString() is string columnStr && 
                        int.TryParse(columnStr, out var parsedColumn) ? parsedColumn : 0;
                }

                var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
                if (element == null)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "Element not found", Data = result };

                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "GridPattern not supported", Data = result };

                // Check if column is within bounds
                if (column >= gridPattern.Current.ColumnCount)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "Column index out of range", Data = result };

                // Try to get the first item in the specified column (assuming header is at row 0)
                var headerElement = gridPattern.GetItem(0, column);
                if (headerElement == null)
                    return new OperationResult<ElementSearchResult> { Success = false, Error = "No header element found at specified column", Data = result };

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
                        ["Row"] = 0,
                        ["Column"] = column
                    }
                };

                return new OperationResult<ElementSearchResult> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                return new OperationResult<ElementSearchResult> { Success = false, Error = $"Error getting column header: {ex.Message}", Data = result };
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