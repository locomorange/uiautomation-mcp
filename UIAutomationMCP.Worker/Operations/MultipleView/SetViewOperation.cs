using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.MultipleView
{
    public class SetViewOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetViewOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var viewId = request.Parameters?.GetValueOrDefault("viewId")?.ToString() is string viewIdStr && 
                int.TryParse(viewIdStr, out var parsedViewId) ? parsedViewId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "MultipleViewPattern not supported" });

            try
            {
                multipleViewPattern.SetCurrentView(viewId);
                return Task.FromResult(new OperationResult { Success = true });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error setting view: {ex.Message}" });
            }
        }
    }
}