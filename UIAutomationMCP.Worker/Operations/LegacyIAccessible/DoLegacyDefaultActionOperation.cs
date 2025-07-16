using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.LegacyIAccessible
{
    public class DoLegacyDefaultActionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public DoLegacyDefaultActionOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<DoLegacyDefaultActionRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected DoLegacyDefaultActionRequest.",
                    Data = new ActionResult { ActionName = "DoLegacyDefaultAction" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ActionResult { ActionName = "DoLegacyDefaultAction" }
                });

            if (!element.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out var pattern) || pattern is not LegacyIAccessiblePattern legacyPattern)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "LegacyIAccessiblePattern not supported",
                    Data = new ActionResult { ActionName = "DoLegacyDefaultAction" }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                var defaultAction = legacyPattern.Current.DefaultAction;
                
                legacyPattern.DoDefaultAction();
                
                var result = new ActionResult
                {
                    ActionName = "DoLegacyDefaultAction",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["ElementName"] = elementInfo.Name,
                        ["ElementType"] = elementInfo.ControlType,
                        ["ElementId"] = elementInfo.AutomationId,
                        ["DefaultAction"] = defaultAction ?? "Unknown",
                        ["Message"] = $"Performed default action: {defaultAction}"
                    }
                };
                
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = $"Failed to perform default action: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "DoLegacyDefaultAction",
                        Completed = false
                    }
                });
            }
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