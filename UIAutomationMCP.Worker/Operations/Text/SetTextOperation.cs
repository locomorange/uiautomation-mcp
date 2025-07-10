using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SetTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetTextOperation(ElementFinderService elementFinderService)
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

            // Primary method: Use ValuePattern for text input controls
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                if (!vp.Current.IsReadOnly)
                {
                    vp.SetValue(text);
                    return Task.FromResult(new OperationResult { Success = true, Data = "Text set successfully" });
                }
                else
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element is read-only" });
                }
            }
            
            return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support text modification" });
        }
    }
}
