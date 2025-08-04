using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Layout
{
    public class ScrollElementIntoViewOperation : BaseUIAutomationOperation<ScrollElementIntoViewRequest, ScrollActionResult>
    {
        public ScrollElementIntoViewOperation(
            ElementFinderService elementFinderService,
            ILogger<ScrollElementIntoViewOperation> logger) : base(elementFinderService, logger)
        {
        }

        protected override Task<ScrollActionResult> ExecuteOperationAsync(ScrollElementIntoViewRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);

            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
            }

            if (!element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) || pattern is not ScrollItemPattern scrollItemPattern)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support ScrollItemPattern");
            }

            scrollItemPattern.ScrollIntoView();

            var result = new ScrollActionResult
            {
                ActionName = "ScrollIntoView",
                Completed = true,
                ExecutedAt = DateTime.UtcNow
            };

            return Task.FromResult(result);
        }
    }
}

