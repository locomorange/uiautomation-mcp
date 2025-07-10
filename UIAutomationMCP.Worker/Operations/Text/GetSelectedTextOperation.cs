using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetSelectedTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetSelectedTextOperation(ElementFinderService elementFinderService)
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
            var selectedTexts = new List<object>();

            foreach (var range in selectionRanges)
            {
                selectedTexts.Add(new
                {
                    Text = range.GetText(-1),
                    BoundingRectangle = range.GetBoundingRectangles()
                });
            }

            if (selectedTexts.Count == 0)
            {
                return Task.FromResult(new OperationResult { Success = true, Data = new { SelectedText = "", HasSelection = false } });
            }
            else if (selectedTexts.Count == 1)
            {
                return Task.FromResult(new OperationResult { Success = true, Data = new { SelectedText = selectedTexts[0], HasSelection = true } });
            }
            else
            {
                return Task.FromResult(new OperationResult { Success = true, Data = new { SelectedTexts = selectedTexts, HasSelection = true, MultipleSelections = true } });
            }
        }
    }
}
