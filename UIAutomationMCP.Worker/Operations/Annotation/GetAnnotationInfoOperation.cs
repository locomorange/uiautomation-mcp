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
    public class GetAnnotationInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetAnnotationInfoOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<AnnotationInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetAnnotationInfoRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<AnnotationInfoResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected GetAnnotationInfoRequest.",
                    Data = new AnnotationInfoResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<AnnotationInfoResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new AnnotationInfoResult()
                });

            if (!element.TryGetCurrentPattern(AnnotationPattern.Pattern, out var pattern) || pattern is not AnnotationPattern annotationPattern)
                return Task.FromResult(new OperationResult<AnnotationInfoResult> 
                { 
                    Success = false, 
                    Error = "AnnotationPattern not supported",
                    Data = new AnnotationInfoResult()
                });

            try
            {
                var current = annotationPattern.Current;
                
                var result = new AnnotationInfoResult
                {
                    AnnotationType = current.AnnotationTypeId,
                    AnnotationTypeName = current.AnnotationTypeName ?? "",
                    Author = current.Author ?? "",
                    DateTime = current.DateTime ?? "",
                    Subject = element.Current.Name ?? ""
                };
                
                return Task.FromResult(new OperationResult<AnnotationInfoResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<AnnotationInfoResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get annotation info: {ex.Message}",
                    Data = new AnnotationInfoResult()
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