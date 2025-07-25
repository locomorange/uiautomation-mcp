using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ScrollElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ScrollElementOperation> _logger;

        public ScrollElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<ScrollElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ScrollElementRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new ScrollActionResult { ActionName = "Scroll" }
                    });
                }

                if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support ScrollPattern",
                        Data = new ScrollActionResult { ActionName = "Scroll" }
                    });
                }

                var direction = typedRequest.Direction ?? "down";
                var amount = typedRequest.Amount;

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
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = false, 
                            Error = $"Unsupported scroll direction: {direction}",
                            Data = new ScrollActionResult { ActionName = "Scroll" }
                        });
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScrollElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to scroll element: {ex.Message}",
                    Data = new ScrollActionResult 
                    { 
                        ActionName = "Scroll",
                        Completed = false
                    }
                });
            }
        }
    }
}