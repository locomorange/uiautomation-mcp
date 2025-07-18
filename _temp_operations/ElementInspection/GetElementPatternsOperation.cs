using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementInspection
{
    public class GetElementPatternsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetElementPatternsOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<UIAutomationMCP.Shared.Results.PatternsInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetElementPatternsRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.PatternsInfoResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new UIAutomationMCP.Shared.Results.PatternsInfoResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.PatternsInfoResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new UIAutomationMCP.Shared.Results.PatternsInfoResult()
                });

            var result = new UIAutomationMCP.Shared.Results.PatternsInfoResult();
            var patternIds = element.GetSupportedPatterns();

            foreach (var patternId in patternIds)
            {
                var patternInfo = new UIAutomationMCP.Shared.Results.PatternInfoResult
                {
                    PatternName = patternId.ProgrammaticName,
                    IsAvailable = true
                };

                // Try to get current state for known patterns
                patternInfo.CurrentState = GetPatternState(element, patternId) ?? new Dictionary<string, object>();
                
                result.Patterns.Add(patternInfo);
            }

            return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.PatternsInfoResult> 
            { 
                Success = true, 
                Data = result 
            });
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