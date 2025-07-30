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

        protected override Task<ActionResult> ExecuteOperationAsync(InvokeElementRequest request)
        {
            // パターン変換（リクエストから取得、デフォルトはInvokePattern）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? InvokePattern.Pattern;
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                ProcessId = request.ProcessId,
                RequiredPattern = requiredPattern?.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationElementNotFoundException("InvokeElement", elementIdentifier);
            }

            if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationInvalidOperationException("InvokeElement", elementIdentifier, "InvokePattern not supported");
            }

            var elementInfo = _elementFinderService.GetElementBasicInfo(element);
            
            invokePattern.Invoke();
            
            return Task.FromResult(new ActionResult
            {
                ActionName = "Invoke",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Invoked element: {elementInfo.Name} (Type: {elementInfo.ControlType}, ID: {elementInfo.AutomationId})"
            });
        }

        protected override Core.Validation.ValidationResult ValidateRequest(InvokeElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required for invoke operation");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}