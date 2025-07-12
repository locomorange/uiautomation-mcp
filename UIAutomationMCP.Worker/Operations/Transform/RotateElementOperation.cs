using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var degrees = GetDoubleParameter(request.Parameters, "degrees", 0);
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "TransformPattern not supported" });

            if (!transformPattern.Current.CanRotate)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element cannot be rotated (CanRotate = false)" });

            try
            {
                transformPattern.Rotate(degrees);
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = $"Element rotated by {degrees} degrees successfully" 
                });
            }
            catch (InvalidOperationException ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Rotate operation failed: {ex.Message}" });
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