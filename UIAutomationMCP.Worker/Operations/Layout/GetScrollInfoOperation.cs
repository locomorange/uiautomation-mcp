using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class GetScrollInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetScrollInfoOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ScrollInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ScrollInfoResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new ScrollInfoResult()
                });

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                return Task.FromResult(new OperationResult<ScrollInfoResult> 
                { 
                    Success = false, 
                    Error = "Element does not support ScrollPattern",
                    Data = new ScrollInfoResult()
                });

            try
            {
                var result = new ScrollInfoResult
                {
                    HorizontalScrollPercent = scrollPattern.Current.HorizontalScrollPercent,
                    VerticalScrollPercent = scrollPattern.Current.VerticalScrollPercent,
                    HorizontalViewSize = scrollPattern.Current.HorizontalViewSize,
                    VerticalViewSize = scrollPattern.Current.VerticalViewSize,
                    HorizontallyScrollable = scrollPattern.Current.HorizontallyScrollable,
                    VerticallyScrollable = scrollPattern.Current.VerticallyScrollable
                };

                return Task.FromResult(new OperationResult<ScrollInfoResult> 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ScrollInfoResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get scroll information: {ex.Message}",
                    Data = new ScrollInfoResult()
                });
            }
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