using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class SetScrollPercentOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SetScrollPercentOperation> _logger;

        public SetScrollPercentOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetScrollPercentOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SetScrollPercentRequest>(parametersJson)!;
                
                var horizontalPercent = typedRequest.HorizontalPercent;
                var verticalPercent = typedRequest.VerticalPercent;

                // Validate percentage ranges (ScrollPattern uses -1 for NoScroll)
                if ((horizontalPercent < -1 || horizontalPercent > 100) && horizontalPercent != ScrollPattern.NoScroll)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Horizontal percentage must be between 0-100 or -1 for no change",
                        Data = new ScrollActionResult()
                    });
                }

                if ((verticalPercent < -1 || verticalPercent > 100) && verticalPercent != ScrollPattern.NoScroll)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Vertical percentage must be between 0-100 or -1 for no change",
                        Data = new ScrollActionResult()
                    });
                }

                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element '{typedRequest.ElementId}' not found",
                        Data = new ScrollActionResult()
                    });
                }

                if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support ScrollPattern",
                        Data = new ScrollActionResult()
                    });
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Scroll percentage out of range: {ex.Message}",
                    Data = new ScrollActionResult()
                });
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Scroll operation not supported: {ex.Message}",
                    Data = new ScrollActionResult()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetScrollPercent operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to set scroll percentage: {ex.Message}",
                    Data = new ScrollActionResult()
                });
            }
        }
    }
}