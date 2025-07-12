using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class IsSelectedOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public IsSelectedOperation(ElementFinderService elementFinderService)
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
            
            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObject))
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element does not support SelectionItemPattern: {elementId}" });
            }

            var selectionItemPattern = (SelectionItemPattern)patternObject;
            bool isSelected = selectionItemPattern.Current.IsSelected;
            
            return Task.FromResult(new OperationResult { Success = true, Data = new { IsSelected = isSelected } });
        }
    }
}