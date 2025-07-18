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
    public class GetGridInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetGridInfoOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<UIAutomationMCP.Shared.Results.GridInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetGridInfoRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.GridInfoResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new UIAutomationMCP.Shared.Results.GridInfoResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.GridInfoResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new UIAutomationMCP.Shared.Results.GridInfoResult()
                });

            if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var pattern) || pattern is not GridPattern gridPattern)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.GridInfoResult> 
                { 
                    Success = false, 
                    Error = "GridPattern not supported",
                    Data = new UIAutomationMCP.Shared.Results.GridInfoResult()
                });

            var result = new UIAutomationMCP.Shared.Results.GridInfoResult
            {
                RowCount = gridPattern.Current.RowCount,
                ColumnCount = gridPattern.Current.ColumnCount
            };

            // Check if selection is supported
            if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selPattern) && selPattern is SelectionPattern selectionPattern)
            {
                result.CanSelectMultiple = selectionPattern.Current.CanSelectMultiple;
            }

            return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.GridInfoResult> 
            { 
                Success = true, 
                Data = result 
            });
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}