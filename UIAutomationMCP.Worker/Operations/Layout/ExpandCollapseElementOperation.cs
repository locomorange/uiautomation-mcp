using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ExpandCollapseElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ExpandCollapseElementOperation> _logger;

        public ExpandCollapseElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<ExpandCollapseElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ExpandCollapseElementRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new ExpandCollapseResult { ActionName = "ExpandCollapse" }
                    });
                }

                if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) || pattern is not ExpandCollapsePattern expandCollapsePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support ExpandCollapsePattern",
                        Data = new ExpandCollapseResult { ActionName = "ExpandCollapse" }
                    });
                }

                var currentState = expandCollapsePattern.Current.ExpandCollapseState;
                var previousState = currentState.ToString();
                var action = typedRequest.Action ?? "toggle";
                
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
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = false, 
                            Error = $"Unsupported expand/collapse action: {action}",
                            Data = new ExpandCollapseResult { ActionName = "ExpandCollapse" }
                        });
                }

                // Small delay to allow UI to update
                System.Threading.Thread.Sleep(100);

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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExpandCollapseElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to expand/collapse element: {ex.Message}",
                    Data = new ExpandCollapseResult 
                    { 
                        ActionName = "ExpandCollapse",
                        Completed = false
                    }
                });
            }
        }
    }
}