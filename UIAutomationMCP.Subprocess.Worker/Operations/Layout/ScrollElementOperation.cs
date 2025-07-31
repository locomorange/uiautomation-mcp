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
    public class ScrollElementOperation : BaseUIAutomationOperation<ScrollElementRequest, ScrollActionResult>
    {
        public ScrollElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<ScrollElementOperation> logger) : base(elementFinderService, logger)
        {
        }

        protected override Task<ScrollActionResult> ExecuteOperationAsync(ScrollElementRequest request)
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

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support ScrollPattern");
            }

            var direction = request.Direction ?? "down";
            var amount = request.Amount;

            switch (direction.ToLowerInvariant())
            {
                case "up":
                    scrollPattern.ScrollVertical(ScrollAmount.SmallDecrement);
                    break;
                case "down":
                    scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
                    break;
                case "left":
                    scrollPattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                    break;
                case "right":
                    scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                    break;
                case "pageup":
                    scrollPattern.ScrollVertical(ScrollAmount.LargeDecrement);
                    break;
                case "pagedown":
                    scrollPattern.ScrollVertical(ScrollAmount.LargeIncrement);
                    break;
                case "pageleft":
                    scrollPattern.ScrollHorizontal(ScrollAmount.LargeDecrement);
                    break;
                case "pageright":
                    scrollPattern.ScrollHorizontal(ScrollAmount.LargeIncrement);
                    break;
                default:
                    throw new ArgumentException($"Unsupported scroll direction: {direction}");
            }

            // Get updated scroll position
            var result = new ScrollActionResult
            {
                ActionName = "Scroll",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                HorizontalPercent = scrollPattern.Current.HorizontalScrollPercent,
                VerticalPercent = scrollPattern.Current.VerticalScrollPercent,
                HorizontalViewSize = scrollPattern.Current.HorizontalViewSize,
                VerticalViewSize = scrollPattern.Current.VerticalViewSize,
                HorizontallyScrollable = scrollPattern.Current.HorizontallyScrollable,
                VerticallyScrollable = scrollPattern.Current.VerticallyScrollable,
                Details = $"Scrolled {direction} by {amount}"
            };

            return Task.FromResult(result);
        }
    }
}

