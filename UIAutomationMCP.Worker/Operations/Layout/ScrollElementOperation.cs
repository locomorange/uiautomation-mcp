using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
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
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ScrollPattern" });

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
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Unsupported scroll direction: {direction}" });
            }

            return Task.FromResult(new OperationResult { Success = true, Data = $"Element scrolled {direction} successfully" });
        }
    }
}
