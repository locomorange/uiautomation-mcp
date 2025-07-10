using System.Windows.Automation;
using System.Windows.Automation.Text;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class TraverseTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public TraverseTextOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var direction = request.Parameters?.GetValueOrDefault("direction")?.ToString() ?? "";
            var count = request.Parameters?.GetValueOrDefault("count")?.ToString() is string countStr && 
                int.TryParse(countStr, out var parsedCount) ? parsedCount : 1;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support TextPattern" });

            var selectionRanges = textPattern.GetSelection();
            var results = new List<object>();

            foreach (var range in selectionRanges)
            {
                var workingRange = range.Clone();
                var (textUnit, moveDirection) = ParseTraversalDirection(direction);
                var actualCount = moveDirection * count;

                var moved = workingRange.Move(textUnit, actualCount);
                workingRange.Select();
                var newText = workingRange.GetText(-1);
                
                results.Add(new
                {
                    MovedUnits = moved,
                    Text = newText,
                    BoundingRectangle = workingRange.GetBoundingRectangles()
                });
            }

            return Task.FromResult(new OperationResult { Success = true, Data = results });
        }

        private (TextUnit textUnit, int direction) ParseTraversalDirection(string direction)
        {
            return direction.ToLower() switch
            {
                "character" or "char" => (TextUnit.Character, 1),
                "character-back" or "char-back" => (TextUnit.Character, -1),
                "word" => (TextUnit.Word, 1),
                "word-back" => (TextUnit.Word, -1),
                "line" => (TextUnit.Line, 1),
                "line-back" => (TextUnit.Line, -1),
                "paragraph" => (TextUnit.Paragraph, 1),
                "paragraph-back" => (TextUnit.Paragraph, -1),
                _ => (TextUnit.Character, 1)
            };
        }
    }
}
