using System.Windows.Automation;
using System.Windows.Automation.Text;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
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

        public Task<OperationResult<TextTraversalResult>> ExecuteAsync(WorkerRequest request)
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
                return Task.FromResult(new OperationResult<TextTraversalResult> { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                return Task.FromResult(new OperationResult<TextTraversalResult> { Success = false, Error = "Element does not support TextPattern" });

            var selectionRanges = textPattern.GetSelection();
            var moveResults = new List<TextMoveInfo>();

            foreach (var range in selectionRanges)
            {
                var workingRange = range.Clone();
                var (textUnit, moveDirection) = ParseTraversalDirection(direction);
                var actualCount = moveDirection * count;

                var moved = workingRange.Move(textUnit, actualCount);
                workingRange.Select();
                var newText = workingRange.GetText(-1);
                
                var boundingRects = workingRange.GetBoundingRectangles();
                var boundingRectArray = boundingRects?.Length > 0 
                    ? new double[] { boundingRects[0].X, boundingRects[0].Y, boundingRects[0].Width, boundingRects[0].Height }
                    : Array.Empty<double>();
                    
                moveResults.Add(new TextMoveInfo
                {
                    MovedUnits = moved,
                    Text = newText,
                    BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle
                    {
                        X = boundingRectArray.Length > 0 ? boundingRectArray[0] : 0,
                        Y = boundingRectArray.Length > 1 ? boundingRectArray[1] : 0,
                        Width = boundingRectArray.Length > 2 ? boundingRectArray[2] : 0,
                        Height = boundingRectArray.Length > 3 ? boundingRectArray[3] : 0
                    }
                });
            }

            var result = new TextTraversalResult
            {
                MoveResults = moveResults
            };

            return Task.FromResult(new OperationResult<TextTraversalResult> { Success = true, Data = result });
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

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(string parametersJson)
        {
            var typedResult = await ExecuteAsync(parametersJson);
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
