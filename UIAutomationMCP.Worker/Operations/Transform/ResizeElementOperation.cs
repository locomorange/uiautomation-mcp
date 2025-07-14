using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class ResizeElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ResizeElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<TransformActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var width = GetDoubleParameter(request.Parameters, "width", 0);
            var height = GetDoubleParameter(request.Parameters, "height", 0);
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            if (width <= 0 || height <= 0)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetWidth", width },
                        { "TargetHeight", height },
                        { "ElementId", elementId }
                    }
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "Width and height must be greater than 0",
                    Data = failureResult
                });
            }

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetWidth", width },
                        { "TargetHeight", height },
                        { "ElementId", elementId }
                    }
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
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetWidth", width },
                        { "TargetHeight", height },
                        { "ElementId", elementId }
                    }
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "TransformPattern not supported",
                    Data = failureResult
                });
            }

            if (!transformPattern.Current.CanResize)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetWidth", width },
                        { "TargetHeight", height },
                        { "ElementId", elementId }
                    }
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "Element cannot be resized (CanResize = false)",
                    Data = failureResult
                });
            }

            try
            {
                transformPattern.Resize(width, height);
                
                // Get updated bounds after resize
                var currentRect = element.Current.BoundingRectangle;
                var newBounds = new BoundingRectangle
                {
                    X = currentRect.X,
                    Y = currentRect.Y,
                    Width = width,
                    Height = height
                };

                var successResult = new TransformActionResult
                {
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    NewBounds = newBounds,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetWidth", width },
                        { "TargetHeight", height },
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
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = new Dictionary<string, object>
                    {
                        { "TargetWidth", width },
                        { "TargetHeight", height },
                        { "ElementId", elementId },
                        { "Exception", ex.Message }
                    }
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = $"Resize operation failed: {ex.Message}",
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