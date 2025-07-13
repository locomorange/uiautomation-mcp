using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public async Task<OperationResult> ExecuteAsync(WorkerRequest request)
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
                return new OperationResult
                {
                    Success = false,
                    Error = "Pattern name is required"
                };
            }

            var patternId = GetPatternId(patternName);
            if (patternId == null)
            {
                return new OperationResult
                {
                    Success = false,
                    Error = $"Unknown pattern: {patternName}"
                };
            }

            var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId);
            
            // Prevent searching from RootElement for performance reasons
            if (searchRoot == null && string.IsNullOrEmpty(windowTitle) && processId == 0)
            {
                return new OperationResult
                {
                    Success = false,
                    Error = "Search scope too broad. Please specify a windowTitle or processId to narrow the search."
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

            var elementList = new List<PatternElementInfo>();
            
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
                    
                    return new OperationResult
                    {
                        Success = true,
                        Data = elementList
                    };
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new OperationResult
                {
                    Success = false,
                    Error = $"Search operation timed out after {timeoutMs}ms"
                };
            }
        }

        private void SearchForPattern(AutomationElement root, TreeWalker walker, AutomationPattern patternId,
            TreeScope scope, List<PatternElementInfo> results, bool includeDescendantPatterns,
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
            AutomationPattern patternId, List<PatternElementInfo> results, bool validateAvailability, int maxResults)
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
                            parentInfo.HasDescendantWithPattern = true;
                            parentInfo.DescendantPatternInfo = $"{child.Current.Name} ({child.Current.ControlType.LocalizedControlType})";
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

        private PatternElementInfo CreatePatternElementInfo(AutomationElement element, AutomationPattern patternId, bool includePatternState)
        {
            var info = new PatternElementInfo
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
                PatternName = GetPatternName(patternId),
                PatternAvailable = true
            };

            // Get all supported patterns
            var supportedPatterns = element.GetSupportedPatterns();
            info.SupportedPatterns = supportedPatterns.Select(p => GetPatternName(p)).ToList();

            // Get pattern-specific state if requested
            if (includePatternState)
            {
                info.PatternState = GetPatternState(element, patternId);
            }

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
    }

    public class PatternElementInfo : ElementInfo
    {
        public string PatternName { get; set; } = "";
        public bool PatternAvailable { get; set; }
        public List<string> SupportedPatterns { get; set; } = new();
        public Dictionary<string, object>? PatternState { get; set; }
        public bool HasDescendantWithPattern { get; set; }
        public string? DescendantPatternInfo { get; set; }
    }
}