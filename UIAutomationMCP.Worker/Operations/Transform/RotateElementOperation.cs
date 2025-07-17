using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class RotateElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public RotateElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<TransformActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var degrees = GetDoubleParameter(request.Parameters, "degrees", 0);
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Rotate",
                    TransformType = "Rotate",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Rotation operation for element {elementId} by {degrees} degrees"
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
                    ActionName = "Rotate",
                    TransformType = "Rotate",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Rotation operation for element {elementId} by {degrees} degrees"
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "TransformPattern not supported",
                    Data = failureResult
                });
            }

            if (!transformPattern.Current.CanRotate)
            {
                var failureResult = new TransformActionResult
                {
                    ActionName = "Rotate",
                    TransformType = "Rotate",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Rotation operation for element {elementId} by {degrees} degrees"
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "Element cannot be rotated (CanRotate = false)",
                    Data = failureResult
                });
            }

            try
            {
                transformPattern.Rotate(degrees);
                
                // Get current bounds (rotation may affect positioning)
                var currentRect = element.Current.BoundingRectangle;
                var newBounds = new BoundingRectangle
                {
                    X = currentRect.X,
                    Y = currentRect.Y,
                    Width = currentRect.Width,
                    Height = currentRect.Height
                };

                var successResult = new TransformActionResult
                {
                    ActionName = "Rotate",
                    TransformType = "Rotate",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    NewBounds = newBounds,
                    RotationAngle = degrees,
                    Details = $"Rotation operation for element {elementId} by {degrees} degrees"
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
                    ActionName = "Rotate",
                    TransformType = "Rotate",
                    Completed = false,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Failed to rotate element {elementId} by {degrees} degrees: {ex.Message}"
                };
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = $"Rotate operation failed: {ex.Message}",
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