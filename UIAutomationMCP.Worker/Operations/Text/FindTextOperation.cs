using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
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

        public Task<OperationResult<TextSearchResult>> ExecuteAsync(WorkerRequest request)
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
            {
                return Task.FromResult(new OperationResult<TextSearchResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new TextSearchResult { Found = false, Text = searchText }
                });
            }

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
            {
                return Task.FromResult(new OperationResult<TextSearchResult> 
                { 
                    Success = false, 
                    Error = "Element does not support TextPattern",
                    Data = new TextSearchResult { Found = false, Text = searchText }
                });
            }

            var documentRange = textPattern.DocumentRange;
            var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
            
            if (foundRange != null)
            {
                var foundText = foundRange.GetText(-1);
                var boundingRects = foundRange.GetBoundingRectangles();
                var boundingRect = boundingRects?.Length > 0 ? new BoundingRectangle
                {
                    X = boundingRects[0].X,
                    Y = boundingRects[0].Y,
                    Width = boundingRects[0].Width,
                    Height = boundingRects[0].Height
                } : null;

                return Task.FromResult(new OperationResult<TextSearchResult> 
                { 
                    Success = true, 
                    Data = new TextSearchResult 
                    { 
                        Found = true, 
                        Text = foundText,
                        BoundingRectangle = boundingRect,
                        StartIndex = 0, // Note: UI Automation doesn't provide exact index
                        Length = foundText.Length
                    }
                });
            }
            else
            {
                return Task.FromResult(new OperationResult<TextSearchResult> 
                { 
                    Success = true, 
                    Data = new TextSearchResult { Found = false, Text = searchText }
                });
            }
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}
