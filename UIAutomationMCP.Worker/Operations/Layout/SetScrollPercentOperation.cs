using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class SetScrollPercentOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetScrollPercentOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            
            var horizontalPercentStr = request.Parameters?.GetValueOrDefault("horizontalPercent")?.ToString() ?? "";
            var verticalPercentStr = request.Parameters?.GetValueOrDefault("verticalPercent")?.ToString() ?? "";

            if (!double.TryParse(horizontalPercentStr, out var horizontalPercent))
                return Task.FromResult(new OperationResult { Success = false, Error = "Invalid horizontal percentage value" });

            if (!double.TryParse(verticalPercentStr, out var verticalPercent))
                return Task.FromResult(new OperationResult { Success = false, Error = "Invalid vertical percentage value" });

            // Validate percentage ranges (ScrollPattern uses -1 for NoScroll)
            if ((horizontalPercent < -1 || horizontalPercent > 100) && horizontalPercent != ScrollPattern.NoScroll)
                return Task.FromResult(new OperationResult { Success = false, Error = "Horizontal percentage must be between 0-100 or -1 for no change" });

            if ((verticalPercent < -1 || verticalPercent > 100) && verticalPercent != ScrollPattern.NoScroll)
                return Task.FromResult(new OperationResult { Success = false, Error = "Vertical percentage must be between 0-100 or -1 for no change" });

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support ScrollPattern" });

            try
            {
                // Use ScrollPattern.NoScroll (-1) to indicate no change for that axis
                var finalHorizontalPercent = horizontalPercent == -1 ? ScrollPattern.NoScroll : horizontalPercent;
                var finalVerticalPercent = verticalPercent == -1 ? ScrollPattern.NoScroll : verticalPercent;

                scrollPattern.SetScrollPercent(finalHorizontalPercent, finalVerticalPercent);

                // Get current scroll position after setting
                var currentInfo = new
                {
                    HorizontalScrollPercent = scrollPattern.Current.HorizontalScrollPercent,
                    VerticalScrollPercent = scrollPattern.Current.VerticalScrollPercent,
                    HorizontalViewSize = scrollPattern.Current.HorizontalViewSize,
                    VerticalViewSize = scrollPattern.Current.VerticalViewSize
                };

                return Task.FromResult(new OperationResult { Success = true, Data = currentInfo });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Scroll percentage out of range: {ex.Message}" });
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Scroll operation not supported: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Failed to set scroll percentage: {ex.Message}" });
            }
        }
    }
}