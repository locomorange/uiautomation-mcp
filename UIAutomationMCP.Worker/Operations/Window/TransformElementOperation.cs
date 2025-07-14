using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class TransformElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public TransformElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<TransformActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var action = request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "";
            var x = GetDoubleParameter(request.Parameters, "x", 0);
            var y = GetDoubleParameter(request.Parameters, "y", 0);
            var width = GetDoubleParameter(request.Parameters, "width", 0);
            var height = GetDoubleParameter(request.Parameters, "height", 0);
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "TransformPattern not supported" });

            try
            {
                switch (action.ToLowerInvariant())
                {
                    case "move":
                        if (!transformPattern.Current.CanMove)
                            return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "Element cannot be moved (CanMove = false)" });
                        if (x == 0 && y == 0)
                            return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "Move action requires x and y coordinates" });
                        transformPattern.Move(x, y);
                        break;
                    case "resize":
                        if (!transformPattern.Current.CanResize)
                            return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "Element cannot be resized (CanResize = false)" });
                        if (width <= 0 || height <= 0)
                            return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "Resize action requires positive width and height values" });
                        transformPattern.Resize(width, height);
                        break;
                    case "rotate":
                        if (!transformPattern.Current.CanRotate)
                            return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = "Element cannot be rotated (CanRotate = false)" });
                        transformPattern.Rotate(x); // Use x as rotation degrees
                        break;
                    default:
                        return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = $"Unsupported transform action: {action}" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult<TransformActionResult> { Success = false, Error = $"Transform operation failed: {ex.Message}" });
            }

            var newBounds = new BoundingRectangle
            {
                X = element.Current.BoundingRectangle.X,
                Y = element.Current.BoundingRectangle.Y,
                Width = element.Current.BoundingRectangle.Width,
                Height = element.Current.BoundingRectangle.Height
            };

            var result = new TransformActionResult
            {
                ActionName = action,
                Completed = true,
                TransformType = action,
                NewBounds = newBounds,
                RotationAngle = action == "rotate" ? x : null
            };

            return Task.FromResult(new OperationResult<TransformActionResult> { Success = true, Data = result });
        }

        private double GetDoubleParameter(Dictionary<string, object>? parameters, string key, double defaultValue = 0.0)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && double.TryParse(value, out var result))
                return result;
            return defaultValue;
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
    }
}