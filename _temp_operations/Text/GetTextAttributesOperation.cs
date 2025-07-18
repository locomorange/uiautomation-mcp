using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetTextAttributesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetTextAttributesOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<TextAttributesResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
            {
                return Task.FromResult(new OperationResult<TextAttributesResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new TextAttributesResult()
                });
            }

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
            {
                return Task.FromResult(new OperationResult<TextAttributesResult> 
                { 
                    Success = false, 
                    Error = "Element does not support TextPattern",
                    Data = new TextAttributesResult()
                });
            }

            try
            {
                var selectionRanges = textPattern.GetSelection();
                var textRanges = new List<TextRangeAttributes>();

                foreach (var range in selectionRanges)
                {
                    var attributes = new Dictionary<string, object>();
                    
                    // Get common text attributes
                    var attributesToCheck = new[]
                    {
                        TextPattern.FontNameAttribute,
                        TextPattern.FontSizeAttribute,
                        TextPattern.FontWeightAttribute,
                        TextPattern.ForegroundColorAttribute,
                        TextPattern.BackgroundColorAttribute,
                        TextPattern.IsItalicAttribute
                    };

                    foreach (var attr in attributesToCheck)
                    {
                        var value = range.GetAttributeValue(attr);
                        if (value != null && !value.Equals(TextPattern.MixedAttributeValue))
                        {
                            attributes[attr.ProgrammaticName] = value;
                        }
                        else
                        {
                            attributes[attr.ProgrammaticName] = "NotSupported";
                        }
                    }

                    var boundingRects = range.GetBoundingRectangles();
                    var boundingRectArray = boundingRects?.Length > 0 
                        ? new double[] { boundingRects[0].X, boundingRects[0].Y, boundingRects[0].Width, boundingRects[0].Height }
                        : Array.Empty<double>();
                    
                    textRanges.Add(new TextRangeAttributes
                    {
                        Text = range.GetText(-1),
                        Attributes = attributes,
                        BoundingRectangle = new UIAutomationMCP.Shared.BoundingRectangle
                        {
                            X = boundingRectArray.Length > 0 ? boundingRectArray[0] : 0,
                            Y = boundingRectArray.Length > 1 ? boundingRectArray[1] : 0,
                            Width = boundingRectArray.Length > 2 ? boundingRectArray[2] : 0,
                            Height = boundingRectArray.Length > 3 ? boundingRectArray[3] : 0
                        }
                    });
                }

                return Task.FromResult(new OperationResult<TextAttributesResult> 
                { 
                    Success = true, 
                    Data = new TextAttributesResult { TextRanges = textRanges }
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<TextAttributesResult> 
                { 
                    Success = false, 
                    Error = $"Failed to get text attributes: {ex.Message}",
                    Data = new TextAttributesResult()
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
