using System.Windows.Automation;
using System.Windows.Automation.Text;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SelectTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SelectTextOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var startIndex = request.Parameters?.GetValueOrDefault("startIndex")?.ToString() is string startIndexStr && 
                int.TryParse(startIndexStr, out var parsedStartIndex) ? parsedStartIndex : 0;
            var length = request.Parameters?.GetValueOrDefault("length")?.ToString() is string lengthStr && 
                int.TryParse(lengthStr, out var parsedLength) ? parsedLength : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
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
                return Task.FromResult(new OperationResult<ActionResult> 
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
            
            if (startIndex < 0 || startIndex >= fullText.Length)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
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
            
            if (startIndex + length > fullText.Length)
                length = fullText.Length - startIndex;

            try
            {
                var textRange = documentRange.Clone();
                textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, startIndex);
                textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, startIndex);
                textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);
                textRange.Select();

                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = true, 
                    Data = new ActionResult 
                    { 
                        ActionName = "SelectText", 
                        Completed = true, 
                        ExecutedAt = DateTime.UtcNow,
                        Details = new Dictionary<string, object>
                        {
                            ["StartIndex"] = startIndex,
                            ["Length"] = length,
                            ["SelectedText"] = fullText.Substring(startIndex, length)
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
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
