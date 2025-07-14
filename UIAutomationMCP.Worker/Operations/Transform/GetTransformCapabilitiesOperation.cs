using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class GetTransformCapabilitiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetTransformCapabilitiesOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<TransformCapabilitiesResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<TransformCapabilitiesResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new TransformCapabilitiesResult()
                });

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                return Task.FromResult(new OperationResult<TransformCapabilitiesResult> 
                { 
                    Success = false, 
                    Error = "TransformPattern not supported",
                    Data = new TransformCapabilitiesResult()
                });

            var result = new TransformCapabilitiesResult
            {
                CanMove = transformPattern.Current.CanMove,
                CanResize = transformPattern.Current.CanResize,
                CanRotate = transformPattern.Current.CanRotate,
                CurrentBounds = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                }
            };

            return Task.FromResult(new OperationResult<TransformCapabilitiesResult> 
            { 
                Success = true, 
                Data = result 
            });
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