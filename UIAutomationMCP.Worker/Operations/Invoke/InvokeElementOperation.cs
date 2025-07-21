using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
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

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<InvokeElementRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    processId: typedRequest.ProcessId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult
                    {
                        Success = false,
                        Error = "Element not found",
                        Data = new ActionResult { ActionName = "Invoke" }
                    });
                }

                if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
                {
                    return Task.FromResult(new OperationResult
                    {
                        Success = false,
                        Error = "InvokePattern not supported",
                        Data = new ActionResult { ActionName = "Invoke" }
                    });
                }

                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                invokePattern.Invoke();
                
                var result = new ActionResult
                {
                    ActionName = "Invoke",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Invoked element: {elementInfo.Name} (Type: {elementInfo.ControlType}, ID: {elementInfo.AutomationId})"
                };
                
                return Task.FromResult(new OperationResult
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult
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
    }
}