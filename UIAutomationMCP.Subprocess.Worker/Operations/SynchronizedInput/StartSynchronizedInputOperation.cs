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
    public class StartSynchronizedInputOperation : BaseUIAutomationOperation<StartSynchronizedInputRequest, ActionResult>
    {
        public StartSynchronizedInputOperation(ElementFinderService elementFinderService, ILogger<StartSynchronizedInputOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<ActionResult> ExecuteOperationAsync(StartSynchronizedInputRequest request)
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
                throw new UIAutomationElementNotFoundException("StartSynchronizedInput", elementIdentifier);
            }

            if (!element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var pattern) || pattern is not SynchronizedInputPattern synchronizedInputPattern)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationInvalidOperationException("StartSynchronizedInput", elementIdentifier, "SynchronizedInputPattern not supported");
            }

            if (!Enum.TryParse<SynchronizedInputType>(request.InputType, true, out var inputType))
            {
                throw new UIAutomationInvalidOperationException("StartSynchronizedInput", element.Current.Name, $"Invalid input type: {request.InputType}");
            }

            synchronizedInputPattern.StartListening(inputType);

            return Task.FromResult(new ActionResult
            {
                ActionName = "StartSynchronizedInput",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Started listening for {inputType} on element: {element.Current.Name}"
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(StartSynchronizedInputRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("AutomationId or Name is required for StartSynchronizedInput operation");
            }

            if (string.IsNullOrWhiteSpace(request.InputType))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("InputType is required for StartSynchronizedInput operation");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
