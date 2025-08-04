using System.Windows.Automation;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Worker.Extensions;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Invoke
{
    public class InvokeElementOperation : BaseUIAutomationOperation<InvokeElementRequest, ActionResult>
    {
        public InvokeElementOperation(ElementFinderService elementFinderService, ILogger<InvokeElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<ActionResult> ExecuteOperationAsync(InvokeElementRequest request)
        {
            // Pattern conversion (get from request, default to InvokePattern)
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? InvokePattern.Pattern;

            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
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

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(InvokeElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required for invoke operation");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}

