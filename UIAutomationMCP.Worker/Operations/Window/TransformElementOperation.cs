using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class TransformElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public TransformElementOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<TransformActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<TransformElementRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<TransformActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format"
                });
            
            var elementId = typedRequest.ElementId;
            var action = typedRequest.Action;
            var x = typedRequest.X;
            var y = typedRequest.Y;
            var width = typedRequest.Width;
            var height = typedRequest.Height;
            var windowTitle = typedRequest.WindowTitle ?? "";
            var processId = typedRequest.ProcessId ?? 0;

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