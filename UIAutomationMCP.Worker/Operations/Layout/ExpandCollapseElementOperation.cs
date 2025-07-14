using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ExpandCollapseElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ExpandCollapseElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ExpandCollapseResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var action = request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ExpandCollapseResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new ExpandCollapseResult { ActionName = "ExpandCollapse" }
                });

            if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) || pattern is not ExpandCollapsePattern expandCollapsePattern)
                return Task.FromResult(new OperationResult<ExpandCollapseResult> 
                { 
                    Success = false, 
                    Error = "Element does not support ExpandCollapsePattern",
                    Data = new ExpandCollapseResult { ActionName = "ExpandCollapse" }
                });

            var currentState = expandCollapsePattern.Current.ExpandCollapseState;
            var previousState = currentState.ToString();
            
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
                    return Task.FromResult(new OperationResult<ExpandCollapseResult> 
                    { 
                        Success = false, 
                        Error = $"Unsupported expand/collapse action: {action}",
                        Data = new ExpandCollapseResult { ActionName = "ExpandCollapse" }
                    });
            }

            var newState = expandCollapsePattern.Current.ExpandCollapseState;
            
            var result = new ExpandCollapseResult
            {
                ActionName = "ExpandCollapse",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                PreviousState = previousState,
                CurrentState = newState.ToString(),
                Details = new Dictionary<string, object>
                {
                    ["Action"] = action
                }
            };

            return Task.FromResult(new OperationResult<ExpandCollapseResult> 
            { 
                Success = true, 
                Data = result
            });
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}
