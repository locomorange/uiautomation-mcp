using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ScrollElementIntoViewOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ScrollElementIntoViewOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ScrollActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) || pattern is not ScrollItemPattern scrollItemPattern)
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = "Element does not support ScrollItemPattern" });

            scrollItemPattern.ScrollIntoView();
            
            var result = new ScrollActionResult
            {
                ActionName = "ScrollIntoView",
                Completed = true,
                ExecutedAt = DateTime.UtcNow
            };
            
            return Task.FromResult(new OperationResult<ScrollActionResult> { Success = true, Data = result });
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
