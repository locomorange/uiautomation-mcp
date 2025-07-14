using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.MultipleView
{
    public class GetCurrentViewOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetCurrentViewOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ViewResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ViewResult> { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return Task.FromResult(new OperationResult<ViewResult> { Success = false, Error = "MultipleViewPattern not supported" });

            try
            {
                var currentViewId = multipleViewPattern.Current.CurrentView;
                var currentViewName = multipleViewPattern.GetViewName(currentViewId);
                
                var viewInfo = new ViewResult
                {
                    ViewId = currentViewId,
                    ViewName = currentViewName
                };

                return Task.FromResult(new OperationResult<ViewResult> { Success = true, Data = viewInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ViewResult> { Success = false, Error = $"Error getting current view: {ex.Message}" });
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