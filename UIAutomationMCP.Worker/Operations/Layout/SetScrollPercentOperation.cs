using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class SetScrollPercentOperation : BaseUIAutomationOperation<SetScrollPercentRequest, ScrollActionResult>
    {
        public SetScrollPercentOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetScrollPercentOperation> logger) : base(elementFinderService, logger)
        {
        }

        protected override Task<ScrollActionResult> ExecuteOperationAsync(SetScrollPercentRequest request)
        {
            var horizontalPercent = request.HorizontalPercent;
            var verticalPercent = request.VerticalPercent;

            // Validate percentage ranges (ScrollPattern uses -1 for NoScroll)
            if ((horizontalPercent < -1 || horizontalPercent > 100) && horizontalPercent != ScrollPattern.NoScroll)
            {
                throw new ArgumentException("Horizontal percentage must be between 0-100 or -1 for no change");
            }

            if ((verticalPercent < -1 || verticalPercent > 100) && verticalPercent != ScrollPattern.NoScroll)
            {
                throw new ArgumentException("Vertical percentage must be between 0-100 or -1 for no change");
            }

            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                ProcessId = request.ProcessId
            };
            var element = _elementFinderService.FindElement(searchCriteria);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
            }

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support ScrollPattern");
            }

            // Use ScrollPattern.NoScroll (-1) to indicate no change for that axis
            var finalHorizontalPercent = horizontalPercent == -1 ? ScrollPattern.NoScroll : horizontalPercent;
            var finalVerticalPercent = verticalPercent == -1 ? ScrollPattern.NoScroll : verticalPercent;

            scrollPattern.SetScrollPercent(finalHorizontalPercent, finalVerticalPercent);

            // Get current scroll position after setting
            var result = new ScrollActionResult
            {
                ActionName = "SetScrollPercent",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                HorizontalPercent = scrollPattern.Current.HorizontalScrollPercent,
                VerticalPercent = scrollPattern.Current.VerticalScrollPercent,
                HorizontalViewSize = scrollPattern.Current.HorizontalViewSize,
                VerticalViewSize = scrollPattern.Current.VerticalViewSize,
                HorizontallyScrollable = scrollPattern.Current.HorizontallyScrollable,
                VerticallyScrollable = scrollPattern.Current.VerticallyScrollable
            };

            return Task.FromResult(result);
        }
    }
}