using System.Windows.Automation;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Worker.Extensions;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Subprocess.Worker.Operations.SynchronizedInput
{
    public class CancelSynchronizedInputOperation : BaseUIAutomationOperation<CancelSynchronizedInputRequest, ActionResult>
    {
        public CancelSynchronizedInputOperation(ElementFinderService elementFinderService, ILogger<CancelSynchronizedInputOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<ActionResult> ExecuteOperationAsync(CancelSynchronizedInputRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = SynchronizedInputPattern.Pattern.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);

            if (element == null)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationElementNotFoundException("CancelSynchronizedInput", elementIdentifier);
            }

            if (!element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var pattern) || pattern is not SynchronizedInputPattern synchronizedInputPattern)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationInvalidOperationException("CancelSynchronizedInput", elementIdentifier, "SynchronizedInputPattern not supported");
            }

            synchronizedInputPattern.Cancel();

            return Task.FromResult(new ActionResult
            {
                ActionName = "CancelSynchronizedInput",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Cancelled synchronized input listening on element: {element.Current.Name}"
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(CancelSynchronizedInputRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("AutomationId or Name is required for CancelSynchronizedInput operation");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
