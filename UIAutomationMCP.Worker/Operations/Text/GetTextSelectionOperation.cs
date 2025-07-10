using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetTextSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetTextSelectionOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });

            var selectionRanges = textPattern.GetSelection();
            var selectionInfo = new List<object>();

            foreach (var range in selectionRanges)
            {
                selectionInfo.Add(new
                {
                    Text = range.GetText(-1),
                    BoundingRectangle = range.GetBoundingRectangles()
                });
            }

            return Task.FromResult(new OperationResult { Success = true, Data = selectionInfo });
        }
    }
}
