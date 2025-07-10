using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class ClearSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ClearSelectionOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var containerElementId = request.Parameters?.GetValueOrDefault("containerElementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(containerElementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Container element not found" });

            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "SelectionPattern not supported" });

            var selection = selectionPattern.Current.GetSelection();
            foreach (AutomationElement selectedElement in selection)
            {
                if (selectedElement != null && 
                    selectedElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && 
                    itemPattern is SelectionItemPattern itemSelectionPattern)
                {
                    itemSelectionPattern.RemoveFromSelection();
                }
            }

            return Task.FromResult(new OperationResult { Success = true, Data = "Selection cleared successfully" });
        }
    }
}
