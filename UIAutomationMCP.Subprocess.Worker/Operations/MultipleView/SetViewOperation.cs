using System.Windows.Automation;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Worker.Extensions;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Subprocess.Worker.Operations.MultipleView
{
    public class SetViewOperation : BaseUIAutomationOperation<SetViewRequest, ActionResult>
    {
        public SetViewOperation(ElementFinderService elementFinderService, ILogger<SetViewOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<ActionResult> ExecuteOperationAsync(SetViewRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = MultipleViewPattern.Pattern.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);

            if (element == null)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationElementNotFoundException("SetView", elementIdentifier);
            }

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern viewPattern)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationInvalidOperationException("SetView", elementIdentifier, "MultipleViewPattern not supported");
            }

            viewPattern.SetCurrentView(request.ViewId);

            return Task.FromResult(new ActionResult
            {
                ActionName = "SetView",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Set current view to {request.ViewId} on element: {element.Current.Name}"
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(SetViewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name) && request.WindowHandle == null)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("AutomationId, Name, or WindowHandle is required for SetView operation");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
