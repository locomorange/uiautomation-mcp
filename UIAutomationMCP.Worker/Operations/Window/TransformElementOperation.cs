using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
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
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "TransformPattern not supported" });

            try
            {
                switch (action.ToLowerInvariant())
                {
                    case "move":
                        if (!transformPattern.Current.CanMove)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Element cannot be moved (CanMove = false)" });
                        if (x == 0 && y == 0)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Move action requires x and y coordinates" });
                        transformPattern.Move(x, y);
                        break;
                    case "resize":
                        if (!transformPattern.Current.CanResize)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Element cannot be resized (CanResize = false)" });
                        if (width <= 0 || height <= 0)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Resize action requires positive width and height values" });
                        transformPattern.Resize(width, height);
                        break;
                    case "rotate":
                        if (!transformPattern.Current.CanRotate)
                            return Task.FromResult(new OperationResult { Success = false, Error = "Element cannot be rotated (CanRotate = false)" });
                        transformPattern.Rotate(x); // Use x as rotation degrees
                        break;
                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unsupported transform action: {action}" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Transform operation failed: {ex.Message}" });
            }

            return Task.FromResult(new OperationResult { Success = true, Data = $"Element transformed with action '{action}' successfully" });
        }

        private double GetDoubleParameter(Dictionary<string, object>? parameters, string key, double defaultValue = 0.0)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && double.TryParse(value, out var result))
                return result;
            return defaultValue;
        }
    }
}