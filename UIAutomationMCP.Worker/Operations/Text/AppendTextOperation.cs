using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class AppendTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public AppendTextOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var text = request.Parameters?.GetValueOrDefault("text")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            // Try ValuePattern first
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                if (!vp.Current.IsReadOnly)
                {
                    var currentValue = vp.Current.Value;
                    var newValue = currentValue + text;
                    vp.SetValue(newValue);
                    return Task.FromResult(new OperationResult { Success = true, Data = "Text appended successfully" });
                }
                else
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element is read-only" });
                }
            }
            else
            {
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support text modification" });
            }
        }
    }
}
