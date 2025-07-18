using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementInspection
{
    public class GetElementPatternsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetElementPatternsOperation> _logger;

        public GetElementPatternsOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetElementPatternsOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetElementPatternsRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new PatternsInfoResult()
                    });
                }

                var result = new PatternsInfoResult();
                var patternIds = element.GetSupportedPatterns();

                foreach (var patternId in patternIds)
                {
                    var patternInfo = new PatternInfoResult
                    {
                        PatternName = patternId.ProgrammaticName,
                        IsAvailable = true
                    };

                    // Try to get current state for known patterns
                    patternInfo.CurrentState = GetPatternState(element, patternId) ?? new Dictionary<string, object>();
                    
                    result.Patterns.Add(patternInfo);
                }

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetElementPatterns operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get element patterns: {ex.Message}",
                    Data = new PatternsInfoResult()
                });
            }
        }

        private Dictionary<string, object>? GetPatternState(AutomationElement element, AutomationPattern patternId)
        {
            try
            {
                var state = new Dictionary<string, object>();

                if (patternId == ValuePattern.Pattern && element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
                {
                    var vp = (ValuePattern)valuePattern;
                    state["Value"] = vp.Current.Value;
                    state["IsReadOnly"] = vp.Current.IsReadOnly;
                }
                else if (patternId == TogglePattern.Pattern && element.TryGetCurrentPattern(TogglePattern.Pattern, out object? togglePattern))
                {
                    var tp = (TogglePattern)togglePattern;
                    state["ToggleState"] = tp.Current.ToggleState.ToString();
                }
                else if (patternId == SelectionItemPattern.Pattern && element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object? selectionPattern))
                {
                    var sp = (SelectionItemPattern)selectionPattern;
                    state["IsSelected"] = sp.Current.IsSelected;
                }
                else if (patternId == ExpandCollapsePattern.Pattern && element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object? expandPattern))
                {
                    var ep = (ExpandCollapsePattern)expandPattern;
                    state["ExpandCollapseState"] = ep.Current.ExpandCollapseState.ToString();
                }
                else if (patternId == RangeValuePattern.Pattern && element.TryGetCurrentPattern(RangeValuePattern.Pattern, out object? rangePattern))
                {
                    var rp = (RangeValuePattern)rangePattern;
                    state["Value"] = rp.Current.Value;
                    state["Minimum"] = rp.Current.Minimum;
                    state["Maximum"] = rp.Current.Maximum;
                    state["IsReadOnly"] = rp.Current.IsReadOnly;
                }
                else if (patternId == WindowPattern.Pattern && element.TryGetCurrentPattern(WindowPattern.Pattern, out object? windowPattern))
                {
                    var wp = (WindowPattern)windowPattern;
                    state["WindowVisualState"] = wp.Current.WindowVisualState.ToString();
                    state["WindowInteractionState"] = wp.Current.WindowInteractionState.ToString();
                    state["CanMaximize"] = wp.Current.CanMaximize;
                    state["CanMinimize"] = wp.Current.CanMinimize;
                    state["IsModal"] = wp.Current.IsModal;
                    state["IsTopmost"] = wp.Current.IsTopmost;
                }

                return state.Count > 0 ? state : null;
            }
            catch
            {
                return null;
            }
        }
    }
}