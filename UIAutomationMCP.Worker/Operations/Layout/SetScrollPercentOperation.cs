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
    public class SetScrollPercentOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SetScrollPercentOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ScrollActionResult>> ExecuteAsync(WorkerRequest request)
        {
            // Try to get typed request first, fall back to legacy dictionary method
            var typedRequest = request.GetTypedRequest<SetScrollPercentRequest>(_options);
            
            var elementId = typedRequest?.ElementId ?? request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = typedRequest?.WindowTitle ?? request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = typedRequest?.ProcessId ?? (request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0);
            
            var horizontalPercent = typedRequest?.HorizontalPercent ?? 
                (request.Parameters?.GetValueOrDefault("horizontalPercent")?.ToString() is string horizontalPercentStr && 
                double.TryParse(horizontalPercentStr, out var parsedHorizontalPercent) ? parsedHorizontalPercent : _options.Value.Layout.DefaultHorizontalScrollPercent);
            
            var verticalPercent = typedRequest?.VerticalPercent ?? 
                (request.Parameters?.GetValueOrDefault("verticalPercent")?.ToString() is string verticalPercentStr && 
                double.TryParse(verticalPercentStr, out var parsedVerticalPercent) ? parsedVerticalPercent : _options.Value.Layout.DefaultVerticalScrollPercent);

            if (typedRequest == null)
            {
                // Legacy validation only when not using typed request
                if (request.Parameters?.GetValueOrDefault("horizontalPercent")?.ToString() is string legacyHorizontalPercentStr && 
                    !double.TryParse(legacyHorizontalPercentStr, out _))
                    return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = "Invalid horizontal percentage value" });

                if (request.Parameters?.GetValueOrDefault("verticalPercent")?.ToString() is string legacyVerticalPercentStr && 
                    !double.TryParse(legacyVerticalPercentStr, out _))
                    return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = "Invalid vertical percentage value" });
            }

            // Validate percentage ranges (ScrollPattern uses -1 for NoScroll)
            if ((horizontalPercent < -1 || horizontalPercent > 100) && horizontalPercent != ScrollPattern.NoScroll)
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = "Horizontal percentage must be between 0-100 or -1 for no change" });

            if ((verticalPercent < -1 || verticalPercent > 100) && verticalPercent != ScrollPattern.NoScroll)
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = "Vertical percentage must be between 0-100 or -1 for no change" });

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = "Element does not support ScrollPattern" });

            try
            {
                // Use ScrollPattern.NoScroll (-1) to indicate no change for that axis
                var finalHorizontalPercent = horizontalPercent == -1 ? ScrollPattern.NoScroll : horizontalPercent;
                var finalVerticalPercent = verticalPercent == -1 ? ScrollPattern.NoScroll : verticalPercent;

                scrollPattern.SetScrollPercent(finalHorizontalPercent, finalVerticalPercent);

                // Get current scroll position after setting
                var result = new ScrollActionResult
                {
                    ActionName = "SetScrollPercent",
                    Completed = true,
                    HorizontalPercent = scrollPattern.Current.HorizontalScrollPercent,
                    VerticalPercent = scrollPattern.Current.VerticalScrollPercent,
                    HorizontalViewSize = scrollPattern.Current.HorizontalViewSize,
                    VerticalViewSize = scrollPattern.Current.VerticalViewSize,
                    HorizontallyScrollable = scrollPattern.Current.HorizontallyScrollable,
                    VerticallyScrollable = scrollPattern.Current.VerticallyScrollable
                };

                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = true, Data = result });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = $"Scroll percentage out of range: {ex.Message}" });
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = $"Scroll operation not supported: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ScrollActionResult> { Success = false, Error = $"Failed to set scroll percentage: {ex.Message}" });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }
    }
}