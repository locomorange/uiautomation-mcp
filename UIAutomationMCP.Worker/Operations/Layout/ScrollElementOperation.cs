using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class ScrollElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ScrollElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ScrollActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var direction = request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? "";
            var amount = request.Parameters?.GetValueOrDefault("amount")?.ToString() is string amountStr && 
                double.TryParse(amountStr, out var parsedAmount) ? parsedAmount : 1.0;

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
                    Details = new Dictionary<string, object>
                    {
                        ["Direction"] = direction,
                        ["Amount"] = amount
                    }
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
