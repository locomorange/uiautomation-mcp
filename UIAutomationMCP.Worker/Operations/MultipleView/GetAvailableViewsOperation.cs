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
    public class GetAvailableViewsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetAvailableViewsOperation> _logger;

        public GetAvailableViewsOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetAvailableViewsOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetAvailableViewsRequest>(parametersJson)!;
                
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

                var supportedViews = multipleViewPattern.Current.GetSupportedViews();
                var viewDetails = new List<ViewInfo>();

                foreach (var viewId in supportedViews)
                {
                    try
                    {
                        var viewName = multipleViewPattern.GetViewName(viewId);
                        viewDetails.Add(new ViewInfo
                        {
                            ViewId = viewId,
                            ViewName = viewName ?? $"View {viewId}"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get name for view ID {ViewId}", viewId);
                        viewDetails.Add(new ViewInfo
                        {
                            ViewId = viewId,
                            ViewName = $"View {viewId}"
                        });
                    }
                }

                var viewNamesDict = new Dictionary<int, string>();
                foreach (var viewDetail in viewDetails)
                {
                    viewNamesDict[viewDetail.ViewId] = viewDetail.ViewName ?? "";
                }

                var result = new MultipleViewResult
                {
                    ElementId = typedRequest.AutomationId ?? typedRequest.Name ?? "",
                    CurrentView = multipleViewPattern.Current.CurrentView,
                    SupportedViews = supportedViews.ToList(),
                    ViewNames = viewNamesDict,
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
                _logger.LogError(ex, "GetAvailableViews operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get available views: {ex.Message}",
                    Data = new MultipleViewResult()
                });
            }
        }
    }
}