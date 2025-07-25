using System.Windows.Automation;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.UIAutomation.Helpers;
using UIAutomationMCP.Worker.Extensions;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Worker.Operations.Invoke
{
    public class InvokeElementOperation : BaseUIAutomationOperation<InvokeElementRequest, ActionResult>
    {
        public InvokeElementOperation(ElementFinderService elementFinderService, ILogger<InvokeElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<ActionResult> ExecuteOperationAsync(InvokeElementRequest request)
        {
            // パターン変換（リクエストから取得、デフォルトはInvokePattern）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? InvokePattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                processId: request.ProcessId,
                requiredPattern: requiredPattern);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("InvokeElement", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
            {
                throw new UIAutomationInvalidOperationException("InvokeElement", request.AutomationId, "InvokePattern not supported");
            }

            var elementInfo = _elementFinderService.GetElementBasicInfo(element);
            
            invokePattern.Invoke();
            
            return new ActionResult
            {
                ActionName = "Invoke",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Invoked element: {elementInfo.Name} (Type: {elementInfo.ControlType}, ID: {elementInfo.AutomationId})"
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(InvokeElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}