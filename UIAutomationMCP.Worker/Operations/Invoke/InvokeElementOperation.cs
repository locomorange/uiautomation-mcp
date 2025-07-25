using System.Windows.Automation;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Shared.ErrorHandling;
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
            var typedRequest = JsonSerializationHelper.Deserialize<InvokeElementRequest>(parametersJson)!;
            
            return Task.FromResult(ErrorHandlerRegistry.Handle(() =>
            {
                // Validate element ID
                var validationError = ErrorHandlerRegistry.ValidateElementId(typedRequest.AutomationId, "InvokeElement");
                if (validationError != null)
                {
                    throw new UIAutomationValidationException("InvokeElement", "Element ID is required");
                }
                
                // パターン変換（リクエストから取得、デフォルトはInvokePattern）
                var requiredPattern = AutomationPatternHelper.GetAutomationPattern(typedRequest.RequiredPattern) ?? InvokePattern.Pattern;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId,
                    requiredPattern: requiredPattern);
                
                if (element == null)
                {
                    throw new UIAutomationElementNotFoundException("InvokeElement", typedRequest.AutomationId);
                }

                if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
                {
                    throw new UIAutomationInvalidOperationException("InvokeElement", typedRequest.AutomationId, "InvokePattern not supported");
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
                
                return new OperationResult
                {
                    Success = true,
                    Data = result
                };
            }, "InvokeElement", typedRequest.AutomationId));
        }
    }
}