using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class FindTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<FindTextOperation> _logger;

        public FindTextOperation(
            ElementFinderService elementFinderService,
            ILogger<FindTextOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<FindTextRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element with AutomationId '{typedRequest.AutomationId}' and Name '{typedRequest.Name}' not found",
                        Data = new TextSearchResult { Found = false, Text = typedRequest.SearchText }
                    });
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TextPattern",
                        Data = new TextSearchResult { Found = false, Text = typedRequest.SearchText }
                    });
                }

                var documentRange = textPattern.DocumentRange;
                var foundRange = documentRange.FindText(typedRequest.SearchText, typedRequest.Backward, typedRequest.IgnoreCase);
                
                if (foundRange != null)
                {
                    var foundText = foundRange.GetText(-1) ?? "";
                    var boundingRects = foundRange.GetBoundingRectangles();
                    var boundingRect = boundingRects?.Length > 0 ? new BoundingRectangle
                    {
                        X = boundingRects[0].X,
                        Y = boundingRects[0].Y,
                        Width = boundingRects[0].Width,
                        Height = boundingRects[0].Height
                    } : new BoundingRectangle();

                    return Task.FromResult(new OperationResult 
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
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true, 
                        Data = new TextSearchResult { Found = false, Text = typedRequest.SearchText }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindText operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to find text: {ex.Message}",
                    Data = new TextSearchResult { Found = false, Text = "" }
                });
            }
        }
    }
}