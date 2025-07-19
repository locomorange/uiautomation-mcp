using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.MultipleView
{
    public class GetCurrentViewOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetCurrentViewOperation> _logger;

        public GetCurrentViewOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetCurrentViewOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetCurrentViewRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle ?? "", 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new MultipleViewResult()
                    });
                }

                if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "MultipleViewPattern not supported",
                        Data = new MultipleViewResult()
                    });
                }

                var currentViewId = multipleViewPattern.Current.CurrentView;
                var currentViewName = "";
                
                try
                {
                    currentViewName = multipleViewPattern.GetViewName(currentViewId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get name for current view ID {ViewId}", currentViewId);
                    currentViewName = $"View {currentViewId}";
                }

                var viewNamesDict = new Dictionary<int, string>
                {
                    [currentViewId] = currentViewName
                };

                var result = new MultipleViewResult
                {
                    ElementId = typedRequest.ElementId,
                    CurrentView = currentViewId,
                    SupportedViews = new List<int> { currentViewId },
                    ViewNames = viewNamesDict,
                    WindowTitle = typedRequest.WindowTitle,
                    ProcessId = typedRequest.ProcessId ?? 0
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCurrentView operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get current view: {ex.Message}",
                    Data = new MultipleViewResult()
                });
            }
        }
    }
}