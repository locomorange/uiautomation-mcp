using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Grid
{
    public class GetGridInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetGridInfoOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<UIAutomationMCP.Shared.Results.GridInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

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