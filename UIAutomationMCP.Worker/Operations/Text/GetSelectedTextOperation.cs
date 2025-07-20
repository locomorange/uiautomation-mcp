using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Windows.Automation;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetSelectedTextOperation : IUIAutomationOperation
    {
        private readonly ILogger<GetSelectedTextOperation> _logger;
        private readonly ElementFinderService _elementFinderService;

        public GetSelectedTextOperation(ILogger<GetSelectedTextOperation> logger, ElementFinderService elementFinderService)
        {
            _logger = logger;
            _elementFinderService = elementFinderService;
        }

        public string Name => "GetSelectedText";

        public async Task<OperationResult<TextInfoResult>> ExecuteAsync(string parametersJson)
        {
            var result = await ExecuteInternalAsync(parametersJson);
            return result;
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

        private async Task<OperationResult<TextInfoResult>> ExecuteInternalAsync(string parametersJson)
        {
            var request = JsonSerializationHelper.Deserialize<GetSelectedTextRequest>(parametersJson)!;
            
            if (request == null)
            {
                return new OperationResult<TextInfoResult>
                {
                    Success = false,
                    Error = "Failed to deserialize request parameters",
                    ExecutionSeconds = 0
                };
            }

            var result = await GetSelectedTextAsync(request);
            return result;
        }

        private Task<OperationResult<TextInfoResult>> GetSelectedTextAsync(GetSelectedTextRequest request)
        {
            try
            {
                var element = _elementFinderService.FindElementById(request.ElementId, request.WindowTitle ?? "", request.ProcessId ?? 0);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult<TextInfoResult>
                    {
                        Success = false,
                        Error = "Element not found",
                        ExecutionSeconds = 0
                    });
                }

                var result = new TextInfoResult
                {
                    Success = true,
                    ElementId = request.ElementId,
                    ElementName = element.Current.Name,
                    ElementAutomationId = element.Current.AutomationId,
                    ElementControlType = element.Current.ControlType.LocalizedControlType,
                    WindowTitle = request.WindowTitle ?? "",
                    ProcessId = request.ProcessId ?? 0,
                    Pattern = "TextPattern"
                };

                // Try to get selected text from TextPattern
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out object textPatternObj) &&
                    textPatternObj is TextPattern textPattern)
                {
                    var selections = textPattern.GetSelection();
                    if (selections.Length > 0)
                    {
                        result.HasSelection = true;
                        result.SelectedText = selections[0].GetText(-1);
                        result.SelectionStart = 0; // TextPattern doesn't provide exact indices
                        result.SelectionEnd = result.SelectedText?.Length ?? 0;
                        result.SelectionLength = result.SelectedText?.Length ?? 0;
                        
                        // Get the full text too
                        var documentRange = textPattern.DocumentRange;
                        result.Text = documentRange.GetText(-1);
                        result.Length = result.Text?.Length ?? 0;
                    }
                    else
                    {
                        result.HasSelection = false;
                        result.SelectedText = "";
                        result.SelectionStart = 0;
                        result.SelectionEnd = 0;
                        result.SelectionLength = 0;
                        
                        // Still get the full text
                        var documentRange = textPattern.DocumentRange;
                        result.Text = documentRange.GetText(-1);
                        result.Length = result.Text?.Length ?? 0;
                    }

                    result.IsReadOnly = !textPattern.SupportedTextSelection.HasFlag(SupportedTextSelection.Single);
                    result.CanSelectText = textPattern.SupportedTextSelection != SupportedTextSelection.None;
                }
                // Fallback to ValuePattern
                else if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object valuePatternObj) &&
                         valuePatternObj is ValuePattern valuePattern)
                {
                    result.Text = valuePattern.Current.Value;
                    result.Length = result.Text?.Length ?? 0;
                    result.IsReadOnly = valuePattern.Current.IsReadOnly;
                    result.HasSelection = false;
                    result.SelectedText = "";
                    result.SelectionStart = 0;
                    result.SelectionEnd = 0;
                    result.SelectionLength = 0;
                    result.CanSelectText = false;
                    result.Pattern = "ValuePattern";
                }
                else
                {
                    // No text pattern available
                    result.Text = element.Current.Name;
                    result.Length = result.Text?.Length ?? 0;
                    result.IsReadOnly = true;
                    result.HasSelection = false;
                    result.SelectedText = "";
                    result.SelectionStart = 0;
                    result.SelectionEnd = 0;
                    result.SelectionLength = 0;
                    result.CanSelectText = false;
                    result.Pattern = "Name";
                }

                return Task.FromResult(new OperationResult<TextInfoResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionSeconds = 0.05
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting selected text from element");
                return Task.FromResult(new OperationResult<TextInfoResult>
                {
                    Success = false,
                    Error = ex.Message,
                    ExecutionSeconds = 0
                });
            }
        }
    }

    public class GetSelectedTextRequest
    {
        public string ElementId { get; set; } = string.Empty;
        public string? WindowTitle { get; set; }
        public int? ProcessId { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}