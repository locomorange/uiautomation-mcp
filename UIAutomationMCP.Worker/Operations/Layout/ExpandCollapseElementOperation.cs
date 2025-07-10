using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ExpandCollapseElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ExpandCollapseElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var action = request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) || pattern is not ExpandCollapsePattern expandCollapsePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ExpandCollapsePattern" });

            var currentState = expandCollapsePattern.Current.ExpandCollapseState;
            
            switch (action.ToLowerInvariant())
            {
                case "expand":
                    expandCollapsePattern.Expand();
                    break;
                case "collapse":
                    expandCollapsePattern.Collapse();
                    break;
                case "toggle":
                    if (currentState == ExpandCollapseState.Expanded)
                        expandCollapsePattern.Collapse();
                    else
                        expandCollapsePattern.Expand();
                    break;
                default:
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Unsupported expand/collapse action: {action}" });
            }

            var newState = expandCollapsePattern.Current.ExpandCollapseState;
            return Task.FromResult(new OperationResult 
            { 
                Success = true, 
                Data = new { PreviousState = currentState.ToString(), NewState = newState.ToString() }
            });
        }
    }
}
