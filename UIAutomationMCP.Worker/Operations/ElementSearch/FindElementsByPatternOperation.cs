using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class FindElementsByPatternOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public FindElementsByPatternOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public async Task<OperationResult<PatternSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var result = await ExecuteInternalAsync(request);
            return result;
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = await ExecuteAsync(request);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
        }

        private async Task<OperationResult<PatternSearchResult>> ExecuteInternalAsync(WorkerRequest request)
        {
            var patternName = request.Parameters?.GetValueOrDefault("pattern")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var scope = request.Parameters?.GetValueOrDefault("scope")?.ToString() ?? "descendants";
            var maxResults = request.Parameters?.GetValueOrDefault("maxResults")?.ToString() is string maxResultsStr && 
                int.TryParse(maxResultsStr, out var parsedMaxResults) ? parsedMaxResults : 100;
            var includeDescendantPatterns = request.Parameters?.GetValueOrDefault("includeDescendantPatterns")?.ToString() == "true";
            var validateAvailability = request.Parameters?.GetValueOrDefault("validateAvailability")?.ToString() != "false"; // default true
            var timeoutMs = request.Parameters?.GetValueOrDefault("timeoutMs")?.ToString() is string timeoutStr && 
                int.TryParse(timeoutStr, out var parsedTimeout) ? parsedTimeout : 30000;

            if (string.IsNullOrEmpty(patternName))
            {
                return new OperationResult<PatternSearchResult>
                {
                    Success = false,
                    Error = "Pattern name is required",
                    Data = new PatternSearchResult()
                };
            }

            var patternId = GetPatternId(patternName);
            if (patternId == null)
            {
                return new OperationResult<PatternSearchResult>
                {
                    Success = false,
                    Error = $"Unknown pattern: {patternName}",
                    Data = new PatternSearchResult { PatternSearched = patternName }
                };
            }

            var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId);
            
            // Prevent searching from RootElement for performance reasons
            if (searchRoot == null && string.IsNullOrEmpty(windowTitle) && processId == 0)
            {
                return new OperationResult<PatternSearchResult>
                {
                    Success = false,
                    Error = "Search scope too broad. Please specify a windowTitle or processId to narrow the search.",
                    Data = new PatternSearchResult { PatternSearched = patternName }
                };
            }
            
            searchRoot ??= AutomationElement.RootElement;

            // Determine tree scope
            TreeScope treeScope = scope.ToLower() switch
            {
                "children" => TreeScope.Children,
                "element" => TreeScope.Element,
                "parent" => TreeScope.Parent,
                "ancestors" => TreeScope.Ancestors,
                "subtree" => TreeScope.Subtree,
                _ => TreeScope.Descendants
            };

            var elementList = new List<ElementInfo>();
            
            // Create cancellation token for timeout
            using var cts = new CancellationTokenSource(timeoutMs);
            
            try
            {
                return await Task.Run(() =>
                {
                    // Use TreeWalker for pattern-based search
                    var walker = TreeWalker.ControlViewWalker;
                    SearchForPattern(searchRoot, walker, patternId, treeScope, elementList, 
                        includeDescendantPatterns, validateAvailability, maxResults);
                    
                    var searchResult = new PatternSearchResult
                    {
                        Elements = elementList,
                        PatternSearched = patternName,
                        ValidationPerformed = validateAvailability,
                        SearchCriteria = new SearchCriteria
                        {
                            WindowTitle = windowTitle,
                            ProcessId = processId > 0 ? processId : null,
                            Scope = scope,
                            AdditionalCriteria = new Dictionary<string, object>
                            {
                                ["pattern"] = patternName,
                                ["includeDescendantPatterns"] = includeDescendantPatterns,
                                ["validateAvailability"] = validateAvailability
                            }
                        }
                    };
                    
                    return new OperationResult<PatternSearchResult>
                    {
                        Success = true,
                        Data = searchResult
                    };
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<PatternSearchResult>
                {
                    Success = false,
                    Error = $"Search operation timed out after {timeoutMs}ms",
                    Data = new PatternSearchResult { PatternSearched = patternName }
                };
            }
        }

        private void SearchForPattern(AutomationElement root, TreeWalker walker, AutomationPattern patternId,
            TreeScope scope, List<ElementInfo> results, bool includeDescendantPatterns,
            bool validateAvailability, int maxResults)
        {
            if (results.Count >= maxResults) return;

            try
            {
                // Check current element
                if (scope == TreeScope.Element || scope == TreeScope.Subtree || scope == TreeScope.Descendants)
                {
                    if (CheckElementForPattern(root, patternId, validateAvailability))
                    {
                        results.Add(CreatePatternElementInfo(root, patternId, validateAvailability));
                    }
                }

                // Check children/descendants
                if (scope == TreeScope.Children || scope == TreeScope.Descendants || scope == TreeScope.Subtree)
                {
                    var child = walker.GetFirstChild(root);
                    while (child != null && results.Count < maxResults)
                    {
                        if (scope == TreeScope.Children)
                        {
                            // Only check direct children
                            if (CheckElementForPattern(child, patternId, validateAvailability))
                            {
                                results.Add(CreatePatternElementInfo(child, patternId, validateAvailability));
                            }
                        }
                        else
                        {
                            // Recursive search for descendants
                            SearchForPattern(child, walker, patternId, TreeScope.Subtree, results, 
                                includeDescendantPatterns, validateAvailability, maxResults);
                        }
                        
                        child = walker.GetNextSibling(child);
                    }
                }

                // Check descendants for pattern support if requested
                if (includeDescendantPatterns && results.Count < maxResults)
                {
                    CheckDescendantsForPattern(root, walker, patternId, results, validateAvailability, maxResults);
                }
            }
            catch (ElementNotAvailableException)
            {
                // Element became unavailable during search
            }
        }

        private void CheckDescendantsForPattern(AutomationElement element, TreeWalker walker, 
            AutomationPattern patternId, List<ElementInfo> results, bool validateAvailability, int maxResults)
        {
            var child = walker.GetFirstChild(element);
            while (child != null && results.Count < maxResults)
            {
                try
                {
                    if (CheckElementForPattern(child, patternId, validateAvailability))
                    {
                        // Add parent element info with descendant pattern info
                        var parentInfo = results.FirstOrDefault(r => r.AutomationId == element.Current.AutomationId);
                        if (parentInfo == null)
                        {
                            parentInfo = CreatePatternElementInfo(element, patternId, false);
                            results.Add(parentInfo);
                        }
                    }
                    
                    CheckDescendantsForPattern(child, walker, patternId, results, validateAvailability, maxResults);
                    child = walker.GetNextSibling(child);
                }
                catch (ElementNotAvailableException)
                {
                    break;
                }
            }
        }

        private bool CheckElementForPattern(AutomationElement element, AutomationPattern patternId, bool validateAvailability)
        {
            try
            {
                var supportedPatterns = element.GetSupportedPatterns();
                bool hasPattern = supportedPatterns.Any(p => p.Id == patternId.Id);
                
                if (!hasPattern || !validateAvailability)
                {
                    return hasPattern;
                }

                // Validate that the pattern is actually available
                object? pattern = null;
                return element.TryGetCurrentPattern(patternId, out pattern) && pattern != null;
            }
            catch
            {
                return false;
            }
        }

        private ElementInfo CreatePatternElementInfo(AutomationElement element, AutomationPattern patternId, bool includePatternState)
        {
            var info = new ElementInfo
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ControlType = element.Current.ControlType.LocalizedControlType,
                ClassName = element.Current.ClassName,
                IsEnabled = element.Current.IsEnabled,
                ProcessId = element.Current.ProcessId,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                HelpText = $"Pattern: {GetPatternName(patternId)}",
                AvailableActions = GetPatternActions(element, patternId)
            };

            return info;
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

                return state.Count > 0 ? state : null;
            }
            catch
            {
                return null;
            }
        }

        private AutomationPattern? GetPatternId(string patternName)
        {
            return patternName.ToLower() switch
            {
                "invoke" or "invokepattern" => InvokePattern.Pattern,
                "value" or "valuepattern" => ValuePattern.Pattern,
                "toggle" or "togglepattern" => TogglePattern.Pattern,
                "selection" or "selectionpattern" => SelectionPattern.Pattern,
                "selectionitem" or "selectionitempattern" => SelectionItemPattern.Pattern,
                "expandcollapse" or "expandcollapsepattern" => ExpandCollapsePattern.Pattern,
                "grid" or "gridpattern" => GridPattern.Pattern,
                "griditem" or "griditempattern" => GridItemPattern.Pattern,
                "multipleview" or "multipleviewpattern" => MultipleViewPattern.Pattern,
                "window" or "windowpattern" => WindowPattern.Pattern,
                "scroll" or "scrollpattern" => ScrollPattern.Pattern,
                "scrollitem" or "scrollitempattern" => ScrollItemPattern.Pattern,
                "rangevalue" or "rangevaluepattern" => RangeValuePattern.Pattern,
                "table" or "tablepattern" => TablePattern.Pattern,
                "tableitem" or "tableitempattern" => TableItemPattern.Pattern,
                "text" or "textpattern" => TextPattern.Pattern,
                "transform" or "transformpattern" => TransformPattern.Pattern,
                "dock" or "dockpattern" => DockPattern.Pattern,
                _ => null
            };
        }

        private string GetPatternName(AutomationPattern pattern)
        {
            if (pattern == InvokePattern.Pattern) return "InvokePattern";
            if (pattern == ValuePattern.Pattern) return "ValuePattern";
            if (pattern == TogglePattern.Pattern) return "TogglePattern";
            if (pattern == SelectionPattern.Pattern) return "SelectionPattern";
            if (pattern == SelectionItemPattern.Pattern) return "SelectionItemPattern";
            if (pattern == ExpandCollapsePattern.Pattern) return "ExpandCollapsePattern";
            if (pattern == GridPattern.Pattern) return "GridPattern";
            if (pattern == GridItemPattern.Pattern) return "GridItemPattern";
            if (pattern == MultipleViewPattern.Pattern) return "MultipleViewPattern";
            if (pattern == WindowPattern.Pattern) return "WindowPattern";
            if (pattern == ScrollPattern.Pattern) return "ScrollPattern";
            if (pattern == ScrollItemPattern.Pattern) return "ScrollItemPattern";
            if (pattern == RangeValuePattern.Pattern) return "RangeValuePattern";
            if (pattern == TablePattern.Pattern) return "TablePattern";
            if (pattern == TableItemPattern.Pattern) return "TableItemPattern";
            if (pattern == TextPattern.Pattern) return "TextPattern";
            if (pattern == TransformPattern.Pattern) return "TransformPattern";
            if (pattern == DockPattern.Pattern) return "DockPattern";
            
            return "Unknown";
        }

        private Dictionary<string, string> GetPatternActions(AutomationElement element, AutomationPattern patternId)
        {
            var actions = new Dictionary<string, string>();
            
            if (patternId == InvokePattern.Pattern)
                actions["Invoke"] = "Click or activate the element";
            else if (patternId == ValuePattern.Pattern)
                actions["SetValue"] = "Set the element's value";
            else if (patternId == TogglePattern.Pattern)
                actions["Toggle"] = "Toggle the element's state";
            else if (patternId == SelectionItemPattern.Pattern)
                actions["Select"] = "Select this item";
            else if (patternId == ExpandCollapsePattern.Pattern)
            {
                actions["Expand"] = "Expand the element";
                actions["Collapse"] = "Collapse the element";
            }
            else if (patternId == ScrollPattern.Pattern)
                actions["Scroll"] = "Scroll the element";
            else if (patternId == WindowPattern.Pattern)
            {
                actions["Close"] = "Close the window";
                actions["Maximize"] = "Maximize the window";
                actions["Minimize"] = "Minimize the window";
            }
            
            return actions;
        }
    }
}