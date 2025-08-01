using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Layout
{
    public class ExpandCollapseElementOperation : BaseUIAutomationOperation<ExpandCollapseElementRequest, ExpandCollapseResult>
    {
        public ExpandCollapseElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<ExpandCollapseElementOperation> logger) : base(elementFinderService, logger)
        {
        }

        protected override async Task<ExpandCollapseResult> ExecuteOperationAsync(ExpandCollapseElementRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
            }

            if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) || pattern is not ExpandCollapsePattern expandCollapsePattern)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support ExpandCollapsePattern");
            }

            var currentState = expandCollapsePattern.Current.ExpandCollapseState;
            var previousState = currentState.ToString();
            var action = request.Action ?? "toggle";
            
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
                    throw new ArgumentException($"Unsupported expand/collapse action: {action}");
            }

            // Small delay to allow UI to update
            await Task.Delay(100);

            var newState = expandCollapsePattern.Current.ExpandCollapseState;
            
            var result = new ExpandCollapseResult
            {
                ActionName = "ExpandCollapse",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                PreviousState = previousState,
                CurrentState = newState.ToString(),
                Details = $"Expand/Collapse action: {action}"
            };

            return result;
        }
    }
}

