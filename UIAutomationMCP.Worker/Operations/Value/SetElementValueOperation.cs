using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Value
{
    public class SetElementValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetElementValueOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var value = request.Parameters?.GetValueOrDefault("value")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "ValuePattern not supported" });

            valuePattern.SetValue(value);
            return Task.FromResult(new OperationResult { Success = true });
        }
    }
}