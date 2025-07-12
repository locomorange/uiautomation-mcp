using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class IsSelectionRequiredOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public IsSelectionRequiredOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var containerId = request.Parameters?.GetValueOrDefault("containerId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(containerId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });
            
            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var patternObject))
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element does not support SelectionPattern: {containerId}" });
            }

            var selectionPattern = (SelectionPattern)patternObject;
            bool isSelectionRequired = selectionPattern.Current.IsSelectionRequired;
            
            return Task.FromResult(new OperationResult { Success = true, Data = new { IsSelectionRequired = isSelectionRequired } });
        }
    }
}