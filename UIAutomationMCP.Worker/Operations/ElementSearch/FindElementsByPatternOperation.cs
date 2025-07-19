using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class FindElementsByPatternOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<FindElementsByPatternOperation> _logger;

        public FindElementsByPatternOperation(
            ElementFinderService elementFinderService,
            ILogger<FindElementsByPatternOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<FindElementsByPatternRequest>(parametersJson)!;
                
                if (string.IsNullOrEmpty(typedRequest.PatternName))
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Pattern name is required",
                        Data = new PatternSearchResult()
                    });
                }

                var patternId = GetPatternId(typedRequest.PatternName);
                if (patternId == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Unknown pattern: {typedRequest.PatternName}",
                        Data = new PatternSearchResult { PatternSearched = typedRequest.PatternName }
                    });
                }

                var searchRoot = _elementFinderService.GetSearchRoot(typedRequest.WindowTitle ?? "", typedRequest.ProcessId ?? 0);
                
                // Prevent searching from RootElement for performance reasons
                if (searchRoot == null && string.IsNullOrEmpty(typedRequest.WindowTitle) && (typedRequest.ProcessId ?? 0) == 0)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Search scope too broad. Please specify a windowTitle or processId to narrow the search.",
                        Data = new PatternSearchResult { PatternSearched = typedRequest.PatternName }
                    });
                }
                
                searchRoot ??= AutomationElement.RootElement;

                // Determine tree scope
                TreeScope treeScope = (typedRequest.Scope?.ToLower()) switch
                {
                    "children" => TreeScope.Children,
                    "element" => TreeScope.Element,
                    "parent" => TreeScope.Parent,
                    "ancestors" => TreeScope.Ancestors,
                    "subtree" => TreeScope.Subtree,
                    _ => TreeScope.Descendants
                };

                var elementList = new List<ElementInfo>();
                var maxResults = 100; // Default max results
                var validateAvailability = true; // Default validation
                
                // Use TreeWalker for pattern-based search
                var walker = TreeWalker.ControlViewWalker;
                SearchForPattern(searchRoot, walker, patternId, treeScope, elementList, validateAvailability, maxResults);
                
                var searchResult = new PatternSearchResult
                {
                    Elements = elementList,
                    PatternSearched = typedRequest.PatternName,
                    ValidationPerformed = validateAvailability,
                    SearchCriteria = $"Pattern search: {typedRequest.PatternName}"
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = searchResult 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindElementsByPattern operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to find elements by pattern: {ex.Message}",
                    Data = new PatternSearchResult()
                });
            }
        }

        private void SearchForPattern(AutomationElement root, TreeWalker walker, AutomationPattern patternId,
            TreeScope scope, List<ElementInfo> results, bool validateAvailability, int maxResults)
        {
            if (results.Count >= maxResults) return;

            try
            {
                // Check current element
                if (scope == TreeScope.Element || scope == TreeScope.Subtree || scope == TreeScope.Descendants)
                {
                    if (CheckElementForPattern(root, patternId, validateAvailability))
                    {
                        results.Add(CreatePatternElementInfo(root, patternId));
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
                                results.Add(CreatePatternElementInfo(child, patternId));
                            }
                        }
                        else
                        {
                            // Recursive search for descendants
                            SearchForPattern(child, walker, patternId, TreeScope.Subtree, results, validateAvailability, maxResults);
                        }
                        
                        child = walker.GetNextSibling(child);
                    }
                }
            }
            catch (ElementNotAvailableException)
            {
                // Element became unavailable during search
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

        private ElementInfo CreatePatternElementInfo(AutomationElement element, AutomationPattern patternId)
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
                SupportedPatterns = new List<string> { GetPatternName(patternId) }
            };

            return info;
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
}