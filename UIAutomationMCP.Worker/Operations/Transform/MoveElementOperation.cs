using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class MoveElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public MoveElementOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<TransformActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<MoveElementRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<TransformActionResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected MoveElementRequest.",
                    Data = new TransformActionResult
                    {
                        ActionName = "Move",
                        TransformType = "Move",
                        Completed = false,
                        ExecutedAt = DateTime.UtcNow
                    }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;
            var x = typedRequest.X;
            var y = typedRequest.Y;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Move",
                    TransformType = "Move",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = failureResult
                });
            }

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Move",
                    TransformType = "Move",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "TransformPattern not supported",
                    Data = failureResult
                });
            }

            if (!transformPattern.Current.CanMove)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Move",
                    TransformType = "Move",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "Element cannot be moved (CanMove = false)",
                    Data = failureResult
                });
            }

            try
            {
                transformPattern.Move(x, y);
                
                // Get updated bounds after move
                var newBounds = new BoundingRectangle
                {
                    X = x,
                    Y = y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                };

                var successResult = new TransformActionResult
                {
                    ActionName = "Move",
                    TransformType = "Move",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    NewBounds = newBounds,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetX", x },
                        { "TargetY", y },
                        { "ElementId", elementId }
                    }
                };

                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = true, 
                    Data = successResult
                });
            }
            catch (InvalidOperationException ex)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Move",
                    TransformType = "Move",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetX", x },
                        { "TargetY", y },
                        { "ElementId", elementId },
                        { "Exception", ex.Message }
                    }
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = $"Move operation failed: {ex.Message}",
                    Data = failureResult
                });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }

        private double GetDoubleParameter(Dictionary<string, object>? parameters, string key, double defaultValue = 0.0)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && double.TryParse(value, out var result))
                return result;
            return defaultValue;
        }
    }
}