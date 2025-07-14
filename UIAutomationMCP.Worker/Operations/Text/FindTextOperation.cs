using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class FindTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public FindTextOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<TextSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            // 型安全なリクエストを試行し、失敗した場合は従来の方法にフォールバック
            var typedRequest = request.GetTypedRequest<FindTextRequest>(_options);
            
            string elementId, windowTitle, searchText;
            int processId;
            bool backward, ignoreCase;
            
            if (typedRequest != null)
            {
                // 型安全なパラメータアクセス
                elementId = typedRequest.ElementId;
                windowTitle = typedRequest.WindowTitle;
                processId = typedRequest.ProcessId ?? 0;
                searchText = typedRequest.SearchText;
                backward = typedRequest.Backward;
                ignoreCase = typedRequest.IgnoreCase;
            }
            else
            {
                // 従来の方法（後方互換性のため）
                elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                searchText = request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "";
                backward = request.Parameters?.GetValueOrDefault("backward")?.ToString() is string backwardStr && 
                    bool.TryParse(backwardStr, out var parsedBackward) ? parsedBackward : false;
                ignoreCase = request.Parameters?.GetValueOrDefault("ignoreCase")?.ToString() is string ignoreCaseStr && 
                    bool.TryParse(ignoreCaseStr, out var parsedIgnoreCase) ? parsedIgnoreCase : true;
            }

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
