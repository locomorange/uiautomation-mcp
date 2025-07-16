using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Styles
{
    public class GetStyleNameOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetStyleNameOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<StyleInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetStyleNameRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<StyleInfoResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected GetStyleNameRequest.",
                    Data = new StyleInfoResult { IsPatternAvailable = false }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<StyleInfoResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new StyleInfoResult { IsPatternAvailable = false }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                if (!element.TryGetCurrentPattern(StylesPattern.Pattern, out var pattern) || pattern is not StylesPattern stylesPattern)
                    return Task.FromResult(new OperationResult<StyleInfoResult> 
                    { 
                        Success = false, 
                        Error = "StylesPattern not supported",
                        Data = new StyleInfoResult 
                        { 
                            IsPatternAvailable = false,
                            ElementInfo = new Dictionary<string, object>
                            {
                                ["ElementName"] = elementInfo.Name,
                                ["ElementType"] = elementInfo.ControlType,
                                ["ElementId"] = elementInfo.AutomationId
                            }
                        }
                    });
                
                var styleName = stylesPattern.Current.StyleName;
                
                var result = new StyleInfoResult
                {
                    StyleName = styleName,
                    IsPatternAvailable = true,
                    ElementInfo = new Dictionary<string, object>
                    {
                        ["ElementName"] = elementInfo.Name,
                        ["ElementType"] = elementInfo.ControlType,
                        ["ElementId"] = elementInfo.AutomationId
                    }
                };
                
                return Task.FromResult(new OperationResult<StyleInfoResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<StyleInfoResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get style name: {ex.Message}",
                    Data = new StyleInfoResult { IsPatternAvailable = false }
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