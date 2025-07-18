using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ScrollElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public ScrollElementOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ScrollActionResult>> ExecuteAsync(WorkerRequest request)
        {
            // Try to get typed request first, fall back to legacy dictionary method
            var typedRequest = request.GetTypedRequest<ScrollElementRequest>(_options);
            
            var elementId = typedRequest?.ElementId ?? request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = typedRequest?.WindowTitle ?? request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = typedRequest?.ProcessId ?? (request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0);
            var direction = typedRequest?.Direction ?? request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? _options.Value.Layout.DefaultScrollDirection;
            var amount = typedRequest?.Amount ?? (request.Parameters?.GetValueOrDefault("amount")?.ToString() is string amountStr && 
                double.TryParse(amountStr, out var parsedAmount) ? parsedAmount : _options.Value.Layout.DefaultScrollAmount);

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ScrollActionResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new ScrollActionResult { ActionName = "Scroll" }
                });

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                return Task.FromResult(new OperationResult<ScrollActionResult> 
                { 
                    Success = false, 
                    Error = "Element does not support ScrollPattern",
                    Data = new ScrollActionResult { ActionName = "Scroll" }
                });

            try
            {
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
                        return Task.FromResult(new OperationResult<ScrollActionResult> 
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

                return Task.FromResult(new OperationResult<ScrollActionResult> 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ScrollActionResult> 
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

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}
