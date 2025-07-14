using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ExpandCollapseElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public ExpandCollapseElementOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ExpandCollapseResult>> ExecuteAsync(WorkerRequest request)
        {
            // Try to get typed request first, fall back to legacy dictionary method
            var typedRequest = request.GetTypedRequest<ExpandCollapseElementRequest>(_options);
            
            var elementId = typedRequest?.ElementId ?? request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = typedRequest?.WindowTitle ?? request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = typedRequest?.ProcessId ?? (request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0);
            var action = typedRequest?.Action ?? request.Parameters?.GetValueOrDefault("action")?.ToString() ?? _options.Value.Layout.DefaultExpandCollapseAction;

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
