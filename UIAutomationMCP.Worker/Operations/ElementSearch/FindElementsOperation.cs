using System.Windows.Automation;
using System.Text.RegularExpressions;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class FindElementsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly FindElementsCacheService _cacheService;

        public FindElementsOperation(ElementFinderService elementFinderService, FindElementsCacheService? cacheService = null)
        {
            _elementFinderService = elementFinderService;
            _cacheService = cacheService ?? new FindElementsCacheService();
        }

        public async Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var searchText = request.Parameters?.GetValueOrDefault("searchText")?.ToString() ?? "";
            var controlType = request.Parameters?.GetValueOrDefault("controlType")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var scope = request.Parameters?.GetValueOrDefault("scope")?.ToString() ?? "descendants";
            var maxResults = request.Parameters?.GetValueOrDefault("maxResults")?.ToString() is string maxResultsStr && 
                int.TryParse(maxResultsStr, out var parsedMaxResults) ? parsedMaxResults : 100;
            var timeoutMs = request.Parameters?.GetValueOrDefault("timeoutMs")?.ToString() is string timeoutStr && 
                int.TryParse(timeoutStr, out var parsedTimeout) ? parsedTimeout : 30000; // 30 seconds default
            
            // Additional search properties
            var className = request.Parameters?.GetValueOrDefault("className")?.ToString() ?? "";
            var helpText = request.Parameters?.GetValueOrDefault("helpText")?.ToString() ?? "";
            var acceleratorKey = request.Parameters?.GetValueOrDefault("acceleratorKey")?.ToString() ?? "";
            var accessKey = request.Parameters?.GetValueOrDefault("accessKey")?.ToString() ?? "";
            
            // Pattern matching parameters
            var patternType = request.Parameters?.GetValueOrDefault("patternType")?.ToString() ?? "exact";
            var searchMethod = request.Parameters?.GetValueOrDefault("searchMethod")?.ToString() ?? "findall"; // findall or treewalker
            
            // Custom condition parameters
            var conditionOperator = request.Parameters?.GetValueOrDefault("conditionOperator")?.ToString() ?? "and"; // and, or, not
            var excludeText = request.Parameters?.GetValueOrDefault("excludeText")?.ToString() ?? "";
            var excludeControlType = request.Parameters?.GetValueOrDefault("excludeControlType")?.ToString() ?? "";
            
            // Cache parameters
            var useCache = request.Parameters?.GetValueOrDefault("useCache")?.ToString() == "true";
            var cacheTimeoutMinutes = request.Parameters?.GetValueOrDefault("cacheTimeoutMinutes")?.ToString() is string cacheTimeoutStr && 
                int.TryParse(cacheTimeoutStr, out var parsedCacheTimeout) ? parsedCacheTimeout : 5;

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
            
            Condition condition = Condition.TrueCondition;
            
            // Build search condition
            var conditions = new List<Condition>();
            
            // Handle pattern-based search differently
            bool usePatternMatching = !string.IsNullOrEmpty(searchText) && 
                (patternType == "wildcard" || patternType == "regex");
            
            if (!string.IsNullOrEmpty(searchText) && !usePatternMatching)
            {
                // Exact match (original behavior)
                var nameCondition = new PropertyCondition(AutomationElement.NameProperty, searchText);
                var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, searchText);
                conditions.Add(new OrCondition(nameCondition, automationIdCondition));
            }
            
            if (!string.IsNullOrEmpty(controlType))
            {
                var controlTypeObj = ControlTypeHelper.GetControlType(controlType);
                if (controlTypeObj != null)
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeObj));
                }
            }
            
            // Add additional property conditions
            if (!string.IsNullOrEmpty(className))
            {
                conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, className));
            }
            
            if (!string.IsNullOrEmpty(helpText))
            {
                conditions.Add(new PropertyCondition(AutomationElement.HelpTextProperty, helpText));
            }
            
            if (!string.IsNullOrEmpty(acceleratorKey))
            {
                conditions.Add(new PropertyCondition(AutomationElement.AcceleratorKeyProperty, acceleratorKey));
            }
            
            if (!string.IsNullOrEmpty(accessKey))
            {
                conditions.Add(new PropertyCondition(AutomationElement.AccessKeyProperty, accessKey));
            }
            
            // Build exclude conditions
            var excludeConditions = new List<Condition>();
            
            if (!string.IsNullOrEmpty(excludeText))
            {
                var excludeNameCondition = new PropertyCondition(AutomationElement.NameProperty, excludeText);
                var excludeIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, excludeText);
                excludeConditions.Add(new OrCondition(excludeNameCondition, excludeIdCondition));
            }
            
            if (!string.IsNullOrEmpty(excludeControlType))
            {
                var excludeControlTypeObj = ControlTypeHelper.GetControlType(excludeControlType);
                if (excludeControlTypeObj != null)
                {
                    excludeConditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, excludeControlTypeObj));
                }
            }
            
            // Combine conditions based on operator
            if (conditions.Count > 0 || excludeConditions.Count > 0)
            {
                if (excludeConditions.Count > 0)
                {
                    var excludeCondition = excludeConditions.Count == 1 ? excludeConditions[0] : 
                        new OrCondition(excludeConditions.ToArray());
                    var notCondition = new NotCondition(excludeCondition);
                    
                    if (conditions.Count > 0)
                    {
                        var includeCondition = conditions.Count == 1 ? conditions[0] :
                            (conditionOperator == "or" ? 
                                (Condition)new OrCondition(conditions.ToArray()) : 
                                new AndCondition(conditions.ToArray()));
                        
                        condition = new AndCondition(includeCondition, notCondition);
                    }
                    else
                    {
                        condition = notCondition;
                    }
                }
                else if (conditions.Count > 0)
                {
                    condition = conditions.Count == 1 ? conditions[0] :
                        (conditionOperator == "or" ? 
                            (Condition)new OrCondition(conditions.ToArray()) : 
                            new AndCondition(conditions.ToArray()));
                }
            }
            
            // Determine tree scope based on parameter
            TreeScope treeScope = scope.ToLower() switch
            {
                "children" => TreeScope.Children,
                "element" => TreeScope.Element,
                "parent" => TreeScope.Parent,
                "ancestors" => TreeScope.Ancestors,
                "subtree" => TreeScope.Subtree,
                _ => TreeScope.Descendants
            };
            
            // Use CacheRequest for better performance
            var cacheRequest = new CacheRequest();
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.IsEnabledProperty);
            cacheRequest.Add(AutomationElement.ProcessIdProperty);
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);
            cacheRequest.Add(AutomationElement.ClassNameProperty);
            cacheRequest.Add(AutomationElement.HelpTextProperty);
            cacheRequest.AutomationElementMode = AutomationElementMode.None;
            cacheRequest.TreeFilter = Automation.RawViewCondition;
            
            // Check cache first if enabled
            if (useCache)
            {
                var cacheKey = _cacheService.GenerateCacheKey(request.Parameters);
                var cachedResult = _cacheService.GetFromCache(cacheKey);
                if (cachedResult != null)
                {
                    return new OperationResult
                    {
                        Success = true,
                        Data = cachedResult
                    };
                }
            }
            
            var elementList = new List<ElementInfo>();
            
            // Create cancellation token for timeout
            using var cts = new CancellationTokenSource(timeoutMs);
            
            try
            {
                return await Task.Run(() =>
                {
                    using (cacheRequest.Activate())
                    {
                        if (searchMethod == "treewalker")
                        {
                            SearchWithTreeWalker(searchRoot, treeScope, condition, elementList, 
                                searchText, patternType, usePatternMatching, maxResults);
                        }
                        else
                        {
                            SearchWithFindAll(searchRoot, treeScope, condition, elementList,
                                searchText, patternType, usePatternMatching, maxResults);
                        }
                    }
                    
                    // Cache the result if enabled
                    if (useCache && elementList.Count > 0)
                    {
                        var cacheKey = _cacheService.GenerateCacheKey(request.Parameters);
                        _cacheService.AddToCache(cacheKey, elementList, TimeSpan.FromMinutes(cacheTimeoutMinutes));
                    }
                    
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

        private void SearchWithFindAll(AutomationElement searchRoot, TreeScope treeScope, 
            Condition condition, List<ElementInfo> elementList, string searchText, 
            string patternType, bool usePatternMatching, int maxResults)
        {
            var elements = searchRoot.FindAll(treeScope, condition);
            
            foreach (AutomationElement element in elements)
            {
                if (element != null)
                {
                    if (elementList.Count >= maxResults)
                    {
                        break;
                    }
                    
                    try
                    {
                        // Check pattern matching if needed
                        if (usePatternMatching)
                        {
                            var name = element.Cached.Name;
                            var automationId = element.Cached.AutomationId;
                            
                            if (!MatchesPattern(name, searchText, patternType) && 
                                !MatchesPattern(automationId, searchText, patternType))
                            {
                                continue;
                            }
                        }
                        
                        elementList.Add(CreateElementInfo(element));
                    }
                    catch (ElementNotAvailableException)
                    {
                        continue;
                    }
                }
            }
        }

        private void SearchWithTreeWalker(AutomationElement searchRoot, TreeScope treeScope,
            Condition condition, List<ElementInfo> elementList, string searchText,
            string patternType, bool usePatternMatching, int maxResults)
        {
            var walker = new TreeWalker(condition);
            
            if (treeScope == TreeScope.Children)
            {
                SearchChildrenWithWalker(searchRoot, walker, elementList, searchText, 
                    patternType, usePatternMatching, maxResults);
            }
            else if (treeScope == TreeScope.Descendants || treeScope == TreeScope.Subtree)
            {
                SearchDescendantsWithWalker(searchRoot, walker, elementList, searchText,
                    patternType, usePatternMatching, maxResults, treeScope == TreeScope.Subtree);
            }
            else
            {
                // For other scopes, fall back to FindAll
                SearchWithFindAll(searchRoot, treeScope, condition, elementList, 
                    searchText, patternType, usePatternMatching, maxResults);
            }
        }

        private void SearchChildrenWithWalker(AutomationElement parent, TreeWalker walker,
            List<ElementInfo> elementList, string searchText, string patternType,
            bool usePatternMatching, int maxResults)
        {
            var child = walker.GetFirstChild(parent);
            
            while (child != null && elementList.Count < maxResults)
            {
                try
                {
                    if (usePatternMatching)
                    {
                        var name = child.Cached.Name;
                        var automationId = child.Cached.AutomationId;
                        
                        if (MatchesPattern(name, searchText, patternType) || 
                            MatchesPattern(automationId, searchText, patternType))
                        {
                            elementList.Add(CreateElementInfo(child));
                        }
                    }
                    else
                    {
                        elementList.Add(CreateElementInfo(child));
                    }
                    
                    child = walker.GetNextSibling(child);
                }
                catch (ElementNotAvailableException)
                {
                    break;
                }
            }
        }

        private void SearchDescendantsWithWalker(AutomationElement root, TreeWalker walker,
            List<ElementInfo> elementList, string searchText, string patternType,
            bool usePatternMatching, int maxResults, bool includeRoot)
        {
            if (includeRoot && elementList.Count < maxResults)
            {
                try
                {
                    if (usePatternMatching)
                    {
                        var name = root.Cached.Name;
                        var automationId = root.Cached.AutomationId;
                        
                        if (MatchesPattern(name, searchText, patternType) || 
                            MatchesPattern(automationId, searchText, patternType))
                        {
                            elementList.Add(CreateElementInfo(root));
                        }
                    }
                    else
                    {
                        elementList.Add(CreateElementInfo(root));
                    }
                }
                catch (ElementNotAvailableException)
                {
                    // Skip
                }
            }
            
            if (elementList.Count >= maxResults) return;
            
            var child = walker.GetFirstChild(root);
            while (child != null && elementList.Count < maxResults)
            {
                SearchDescendantsWithWalker(child, walker, elementList, searchText,
                    patternType, usePatternMatching, maxResults, true);
                    
                child = walker.GetNextSibling(child);
            }
        }

        private bool MatchesPattern(string text, string pattern, string patternType)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            switch (patternType)
            {
                case "wildcard":
                    return MatchesWildcard(text, pattern);
                case "regex":
                    return MatchesRegex(text, pattern);
                default:
                    return text.Equals(pattern, StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool MatchesWildcard(string text, string pattern)
        {
            // Convert wildcard pattern to regex
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            
            return Regex.IsMatch(text, regexPattern, RegexOptions.IgnoreCase);
        }

        private bool MatchesRegex(string text, string pattern)
        {
            try
            {
                return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                // Invalid regex pattern
                return false;
            }
        }

        private ElementInfo CreateElementInfo(AutomationElement element)
        {
            return new ElementInfo
            {
                AutomationId = element.Cached.AutomationId,
                Name = element.Cached.Name,
                ControlType = element.Cached.ControlType.LocalizedControlType,
                IsEnabled = element.Cached.IsEnabled,
                ProcessId = element.Cached.ProcessId,
                ClassName = element.Cached.ClassName,
                HelpText = element.Cached.HelpText,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Cached.BoundingRectangle.X,
                    Y = element.Cached.BoundingRectangle.Y,
                    Width = element.Cached.BoundingRectangle.Width,
                    Height = element.Cached.BoundingRectangle.Height
                }
            };
        }

    }
}