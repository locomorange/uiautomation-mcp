using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Annotation
{
    public class GetAnnotationTargetOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetAnnotationTargetOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<AnnotationTargetResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetAnnotationTargetRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<AnnotationTargetResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected GetAnnotationTargetRequest.",
                    Data = new AnnotationTargetResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<AnnotationTargetResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new AnnotationTargetResult()
                });

            if (!element.TryGetCurrentPattern(AnnotationPattern.Pattern, out var pattern) || pattern is not AnnotationPattern annotationPattern)
                return Task.FromResult(new OperationResult<AnnotationTargetResult> 
                { 
                    Success = false, 
                    Error = "AnnotationPattern not supported",
                    Data = new AnnotationTargetResult()
                });

            try
            {
                var targetElement = annotationPattern.Current.Target;
                
                var result = new AnnotationTargetResult();
                
                if (targetElement != null)
                {
                    var targetInfo = _elementFinderService.GetElementBasicInfo(targetElement);
                    var bounds = targetElement.Current.BoundingRectangle;
                    
                    result.TargetElementId = targetInfo.AutomationId;
                    result.TargetElementName = targetInfo.Name;
                    result.TargetElementType = targetInfo.ControlType;
                    result.TargetElementBounds = bounds.IsEmpty ? null : new Dictionary<string, double>
                    {
                        ["left"] = bounds.Left,
                        ["top"] = bounds.Top,
                        ["width"] = bounds.Width,
                        ["height"] = bounds.Height
                    };
                }
                else
                {
                    result.TargetElementId = "";
                    result.TargetElementName = "No target element found";
                    result.TargetElementType = "";
                }
                
                return Task.FromResult(new OperationResult<AnnotationTargetResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<AnnotationTargetResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get annotation target: {ex.Message}",
                    Data = new AnnotationTargetResult()
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