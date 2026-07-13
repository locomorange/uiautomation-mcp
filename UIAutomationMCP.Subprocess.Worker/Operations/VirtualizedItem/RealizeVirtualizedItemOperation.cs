using System.Windows.Automation;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Worker.Extensions;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Subprocess.Worker.Operations.VirtualizedItem
{
    public class RealizeVirtualizedItemOperation : BaseUIAutomationOperation<RealizeVirtualizedItemRequest, ActionResult>
    {
        public RealizeVirtualizedItemOperation(ElementFinderService elementFinderService, ILogger<RealizeVirtualizedItemOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<ActionResult> ExecuteOperationAsync(RealizeVirtualizedItemRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = VirtualizedItemPattern.Pattern.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);

            if (element == null)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationElementNotFoundException("RealizeVirtualizedItem", elementIdentifier);
            }

            if (!element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out var pattern) || pattern is not VirtualizedItemPattern virtualizedItemPattern)
            {
                var elementIdentifier = !string.IsNullOrWhiteSpace(request.AutomationId) ? request.AutomationId : request.Name ?? "unknown";
                throw new UIAutomationInvalidOperationException("RealizeVirtualizedItem", elementIdentifier, "VirtualizedItemPattern not supported");
            }

            virtualizedItemPattern.Realize();

            return Task.FromResult(new ActionResult
            {
                ActionName = "RealizeVirtualizedItem",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Realized virtualized item: {element.Current.Name}"
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(RealizeVirtualizedItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("AutomationId or Name is required for RealizeVirtualizedItem operation");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
