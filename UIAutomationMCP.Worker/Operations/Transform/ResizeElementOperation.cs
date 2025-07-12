using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var width = GetDoubleParameter(request.Parameters, "width", 0);
            var height = GetDoubleParameter(request.Parameters, "height", 0);
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            if (width <= 0 || height <= 0)
                return Task.FromResult(new OperationResult { Success = false, Error = "Width and height must be greater than 0" });

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "TransformPattern not supported" });

            if (!transformPattern.Current.CanResize)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element cannot be resized (CanResize = false)" });

            try
            {
                transformPattern.Resize(width, height);
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = $"Element resized to ({width}, {height}) successfully" 
                });
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Resize operation failed: {ex.Message}" });
            }
        }

        private double GetDoubleParameter(Dictionary<string, object>? parameters, string key, double defaultValue = 0.0)
        {
            if (parameters?.GetValueOrDefault(key)?.ToString() is string value && double.TryParse(value, out var result))
                return result;
            return defaultValue;
        }
    }
}