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
    public class GetViewNameOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetViewNameOperation> _logger;

        public GetViewNameOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetViewNameOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetViewNameRequest>(parametersJson)!;
                
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

                var viewName = "";
                try
                {
                    viewName = multipleViewPattern.GetViewName(typedRequest.ViewId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get name for view ID {ViewId}", typedRequest.ViewId);
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"View ID {typedRequest.ViewId} not found",
                        Data = new MultipleViewResult()
                    });
                }

                var viewNamesDict = new Dictionary<int, string>
                {
                    [typedRequest.ViewId] = viewName
                };

                var result = new MultipleViewResult
                {
                    ElementId = typedRequest.ElementId,
                    CurrentView = typedRequest.ViewId,
                    SupportedViews = new List<int> { typedRequest.ViewId },
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
                _logger.LogError(ex, "GetViewName operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get view name: {ex.Message}",
                    Data = new MultipleViewResult()
                });
            }
        }
    }
}