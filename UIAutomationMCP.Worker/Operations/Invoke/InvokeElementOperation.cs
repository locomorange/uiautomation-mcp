using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Invoke
{
    public class InvokeElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public InvokeElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "InvokePattern not supported" });

            invokePattern.Invoke();
            return Task.FromResult(new OperationResult { Success = true });
        }
    }
}