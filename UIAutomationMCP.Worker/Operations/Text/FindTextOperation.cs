using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class FindTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public FindTextOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var searchText = request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "";
            var backward = request.Parameters?.GetValueOrDefault("backward")?.ToString() is string backwardStr && 
                bool.TryParse(backwardStr, out var parsedBackward) ? parsedBackward : false;
            var ignoreCase = request.Parameters?.GetValueOrDefault("ignoreCase")?.ToString() is string ignoreCaseStr && 
                bool.TryParse(ignoreCaseStr, out var parsedIgnoreCase) ? parsedIgnoreCase : true;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });

            var documentRange = textPattern.DocumentRange;
            var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
            
            if (foundRange != null)
            {
                var foundText = foundRange.GetText(-1);
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new { Found = true, Text = foundText, BoundingRectangle = foundRange.GetBoundingRectangles() }
                });
            }
            else
            {
                return Task.FromResult(new OperationResult { Success = true, Data = new { Found = false } });
            }
        }
    }
}
