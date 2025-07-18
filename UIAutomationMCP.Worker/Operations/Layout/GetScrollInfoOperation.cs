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
    public class GetScrollInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetScrollInfoOperation> _logger;

        public GetScrollInfoOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetScrollInfoOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetScrollInfoRequest>(parametersJson)!;
                
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
                        Data = new ScrollInfoResult()
                    });
                }

                if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support ScrollPattern",
                        Data = new ScrollInfoResult()
                    });
                }

                var result = new ScrollInfoResult
                {
                    HorizontalScrollPercent = scrollPattern.Current.HorizontalScrollPercent,
                    VerticalScrollPercent = scrollPattern.Current.VerticalScrollPercent,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetScrollInfo operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get scroll information: {ex.Message}",
                    Data = new ScrollInfoResult()
                });
            }
        }
    }
}