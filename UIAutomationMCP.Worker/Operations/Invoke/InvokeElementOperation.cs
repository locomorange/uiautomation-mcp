using System.Windows.Automation;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Common.Helpers;
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
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                ProcessId = request.ProcessId,
                RequiredPattern = requiredPattern?.ProgrammaticName
            };
            var element = _elementFinderService.FindElement(searchCriteria);
            
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