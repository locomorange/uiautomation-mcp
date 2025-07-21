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
    public class ScrollElementIntoViewOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ScrollElementIntoViewOperation> _logger;

        public ScrollElementIntoViewOperation(
            ElementFinderService elementFinderService, 
            ILogger<ScrollElementIntoViewOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ScrollElementIntoViewRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new ScrollActionResult()
                    });
                }

                if (!element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) || pattern is not ScrollItemPattern scrollItemPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support ScrollItemPattern",
                        Data = new ScrollActionResult()
                    });
                }

                scrollItemPattern.ScrollIntoView();
                
                var result = new ScrollActionResult
                {
                    ActionName = "ScrollIntoView",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScrollElementIntoView operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to scroll element into view: {ex.Message}",
                    Data = new ScrollActionResult()
                });
            }
        }
    }
}