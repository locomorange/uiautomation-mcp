using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Invoke
{
    public class InvokeElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public InvokeElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ActionResult { ActionName = "Invoke" }
                });

            if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "InvokePattern not supported",
                    Data = new ActionResult { ActionName = "Invoke" }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                invokePattern.Invoke();
                
                var result = new ActionResult
                {
                    ActionName = "Invoke",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        ["ElementName"] = elementInfo.Name,
                        ["ElementType"] = elementInfo.ControlType,
                        ["ElementId"] = elementInfo.AutomationId
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
                    Error = $"Failed to invoke element: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "Invoke",
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