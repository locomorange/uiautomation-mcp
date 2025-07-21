using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SelectTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SelectTextOperation> _logger;

        public SelectTextOperation(
            ElementFinderService elementFinderService,
            ILogger<SelectTextOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SelectTextRequest>(parametersJson)!;
                
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
                        Data = new ActionResult 
                        { 
                            ActionName = "SelectText", 
                            Completed = false, 
                            ExecutedAt = DateTime.UtcNow 
                        }
                    });
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TextPattern",
                        Data = new ActionResult 
                        { 
                            ActionName = "SelectText", 
                            Completed = false, 
                            ExecutedAt = DateTime.UtcNow 
                        }
                    });
                }

                var documentRange = textPattern.DocumentRange;
                var fullText = documentRange.GetText(-1);
                
                if (typedRequest.StartIndex < 0 || typedRequest.StartIndex >= fullText.Length)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Start index is out of range",
                        Data = new ActionResult 
                        { 
                            ActionName = "SelectText", 
                            Completed = false, 
                            ExecutedAt = DateTime.UtcNow 
                        }
                    });
                }
                
                var length = typedRequest.Length;
                if (typedRequest.StartIndex + length > fullText.Length)
                    length = fullText.Length - typedRequest.StartIndex;

                var textRange = documentRange.Clone();
                textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, typedRequest.StartIndex);
                textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, typedRequest.StartIndex);
                textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);
                textRange.Select();

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = new ActionResult 
                    { 
                        ActionName = "SelectText", 
                        Completed = true, 
                        ExecutedAt = DateTime.UtcNow,
                        Details = $"Selected text from index {typedRequest.StartIndex}, length {length}: '{fullText.Substring(typedRequest.StartIndex, length)}'"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectText operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to select text: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "SelectText", 
                        Completed = false, 
                        ExecutedAt = DateTime.UtcNow 
                    }
                });
            }
        }
    }
}