using System.Windows.Automation;
using System.Text.RegularExpressions;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
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

        public async Task<OperationResult<ElementSearchResult>> ExecuteAsync(string parametersJson)
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

        private async Task<OperationResult<ElementSearchResult>> ExecuteInternalAsync(string parametersJson)
        {
            // Direct deserialization from pre-serialized JSON
            // Defaults are already applied at Server level
            var typedRequest = JsonSerializationHelper.Deserialize<FindElementsRequest>(parametersJson)!;
            
            if (typedRequest == null)
            {
                return new OperationResult<ElementSearchResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected FindElementsRequest.",
                    Data = new ElementSearchResult()
                };
            }
            
            var searchText = typedRequest.SearchText ?? "";
            var controlType = typedRequest.ControlType ?? "";
            var windowTitle = typedRequest.WindowTitle ?? "";
            var processId = typedRequest.ProcessId ?? 0;
            var scope = typedRequest.Scope;
            var maxResults = typedRequest.MaxResults; // Default already applied at server level
            var timeoutMs = 30000; // タイムアウト処理はworkerで行わない
            
            var className = typedRequest.ClassName ?? "";
            var helpText = "";
            var acceleratorKey = "";
            var accessKey = "";
            
            var patternType = "exact";
            var searchMethod = "findall";
            var conditionOperator = "and";
            var excludeText = "";
            var excludeControlType = "";
            
            var useCache = typedRequest.UseCache;
            var useRegex = typedRequest.UseRegex;
            var useWildcard = typedRequest.UseWildcard;
            var cacheTimeoutMinutes = 5;

            var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId);
            
            // Prevent searching from RootElement for performance reasons
            // Allow search if either windowTitle or processId is provided, even if GetSearchRoot returns null
            if (string.IsNullOrEmpty(windowTitle) && processId == 0)
            {
                var result = new ElementSearchResult();
                return new OperationResult<ElementSearchResult>
                {
                    Success = false,
                    Error = "Search scope too broad. Please specify windowTitle or processId to avoid performance issues. Full desktop search may take significant time and consume substantial system resources.",
                    Data = result
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
            cacheRequest.Add(AutomationElement.LabeledByProperty);
            cacheRequest.Add(AutomationElement.AccessKeyProperty);
            cacheRequest.Add(AutomationElement.AcceleratorKeyProperty);
            
            // Add patterns to cache
            cacheRequest.Add(ValuePattern.Pattern);
            cacheRequest.Add(TogglePattern.Pattern);
            cacheRequest.Add(RangeValuePattern.Pattern);
            cacheRequest.Add(WindowPattern.Pattern);
            cacheRequest.Add(SelectionPattern.Pattern);
            cacheRequest.Add(GridPattern.Pattern);
            cacheRequest.Add(ScrollPattern.Pattern);
            cacheRequest.Add(TextPattern.Pattern);
            cacheRequest.Add(TransformPattern.Pattern);
            cacheRequest.Add(ExpandCollapsePattern.Pattern);
            cacheRequest.Add(DockPattern.Pattern);
            cacheRequest.Add(MultipleViewPattern.Pattern);
            cacheRequest.Add(GridItemPattern.Pattern);
            cacheRequest.Add(TableItemPattern.Pattern);
            cacheRequest.Add(TablePattern.Pattern);
            cacheRequest.Add(SelectionItemPattern.Pattern);
            cacheRequest.Add(InvokePattern.Pattern);
            cacheRequest.Add(ScrollItemPattern.Pattern);
            cacheRequest.Add(VirtualizedItemPattern.Pattern);
            cacheRequest.Add(ItemContainerPattern.Pattern);
            cacheRequest.Add(SynchronizedInputPattern.Pattern);
            
            cacheRequest.AutomationElementMode = AutomationElementMode.None;
            cacheRequest.TreeFilter = Automation.RawViewCondition;
            
            // Check cache first if enabled
            if (useCache)
            {
                var cacheKey = _cacheService.GenerateCacheKey(CreateParametersDictionary(typedRequest));
                var cachedResult = _cacheService.GetFromCache(cacheKey);
                if (cachedResult != null)
                {
                    var cachedSearchResult = new ElementSearchResult
                    {
                        Elements = cachedResult,
                        SearchCriteria = CreateSearchCriteriaString(CreateParametersDictionary(typedRequest))
                    };
                    return new OperationResult<ElementSearchResult>
                    {
                        Success = true,
                        Data = cachedSearchResult
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
                        var cacheKey = _cacheService.GenerateCacheKey(CreateParametersDictionary(typedRequest));
                        _cacheService.AddToCache(cacheKey, elementList, TimeSpan.FromMinutes(cacheTimeoutMinutes));
                    }
                    
                    var searchResult = new ElementSearchResult
                    {
                        Elements = elementList,
                        SearchCriteria = CreateSearchCriteriaString(CreateParametersDictionary(typedRequest))
                    };
                    
                    return new OperationResult<ElementSearchResult>
                    {
                        Success = true,
                        Data = searchResult
                    };
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<ElementSearchResult>
                {
                    Success = false,
                    Error = $"Search operation timed out after {timeoutMs}ms",
                    Data = new ElementSearchResult()
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
            var info = new UIAutomationMCP.Shared.ElementInfo
            {
                AutomationId = element.Cached.AutomationId,
                Name = element.Cached.Name,
                ControlType = element.Cached.ControlType.LocalizedControlType,
                LocalizedControlType = element.Cached.ControlType.LocalizedControlType,
                IsEnabled = element.Cached.IsEnabled,
                IsVisible = !element.Cached.IsOffscreen,
                IsOffscreen = element.Cached.IsOffscreen,
                ProcessId = element.Cached.ProcessId,
                ClassName = element.Cached.ClassName,
                FrameworkId = element.Cached.FrameworkId,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Cached.BoundingRectangle.X,
                    Y = element.Cached.BoundingRectangle.Y,
                    Width = element.Cached.BoundingRectangle.Width,
                    Height = element.Cached.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element)
            };

            // 詳細情報を設定（既存動作を維持するため常に含める）
            info.Details = CreateElementDetails(element);
            
            return info;
        }

        private string[] GetSupportedPatternsArray(AutomationElement element)
        {
            try
            {
                var supportedPatterns = element.GetSupportedPatterns();
                return supportedPatterns.Select(p => p.ProgrammaticName).ToArray();
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private ElementDetails CreateElementDetails(AutomationElement element)
        {
            var details = new ElementDetails
            {
                HelpText = element.Cached.HelpText ?? "",
                HasKeyboardFocus = element.Cached.HasKeyboardFocus,
                IsKeyboardFocusable = element.Cached.IsKeyboardFocusable,
                IsPassword = element.Cached.IsPassword
            };

            // パターン情報を設定
            SetPatternInfoSafe(element, details);
            
            return details;
        }

        private void SetPatternInfoSafe(AutomationElement element, ElementDetails details)
        {
            try
            {
                // Only use basic, safe patterns that work reliably with caching

                // Value Pattern
                if (element.TryGetCachedPattern(ValuePattern.Pattern, out var valuePatternObj) && 
                    valuePatternObj is ValuePattern valuePattern)
                {
                    details.ValueInfo = new ValueInfo
                    {
                        Value = valuePattern.Cached.Value ?? "",
                        IsReadOnly = valuePattern.Cached.IsReadOnly
                    };
                }

                // Toggle Pattern
                if (element.TryGetCachedPattern(TogglePattern.Pattern, out var togglePatternObj) && 
                    togglePatternObj is TogglePattern togglePattern)
                {
                    var state = togglePattern.Cached.ToggleState;
                    details.Toggle = new ToggleInfo
                    {
                        State = state.ToString(),
                        IsToggled = state == ToggleState.On,
                        CanToggle = true
                    };
                }

                // Range Value Pattern
                if (element.TryGetCachedPattern(RangeValuePattern.Pattern, out var rangePatternObj) && 
                    rangePatternObj is RangeValuePattern rangePattern)
                {
                    details.Range = new RangeInfo
                    {
                        Value = rangePattern.Cached.Value,
                        Minimum = rangePattern.Cached.Minimum,
                        Maximum = rangePattern.Cached.Maximum,
                        SmallChange = rangePattern.Cached.SmallChange,
                        LargeChange = rangePattern.Cached.LargeChange,
                        IsReadOnly = rangePattern.Cached.IsReadOnly
                    };
                }

                // Invoke Pattern (just presence check)
                if (element.TryGetCachedPattern(InvokePattern.Pattern, out _))
                {
                    details.Invoke = new InvokeInfo
                    {
                        IsInvokable = true
                    };
                }

                // ScrollItem Pattern (just presence check)
                if (element.TryGetCachedPattern(ScrollItemPattern.Pattern, out _))
                {
                    details.ScrollItem = new ScrollItemInfo
                    {
                        IsScrollable = true
                    };
                }

                // Selection Pattern - basic properties only
                if (element.TryGetCachedPattern(SelectionPattern.Pattern, out var selectionPatternObj) && 
                    selectionPatternObj is SelectionPattern selectionPattern)
                {
                    details.Selection = new SelectionInfo
                    {
                        CanSelectMultiple = selectionPattern.Cached.CanSelectMultiple,
                        IsSelectionRequired = selectionPattern.Cached.IsSelectionRequired,
                        SelectedCount = 0, // Skip complex GetSelection operation
                        SelectedItems = new List<SelectionItemInfo>() // Skip complex operations
                    };
                }

                // SelectionItem Pattern - basic properties only
                if (element.TryGetCachedPattern(SelectionItemPattern.Pattern, out var selectionItemPatternObj) && 
                    selectionItemPatternObj is SelectionItemPattern selectionItemPattern)
                {
                    var selectionContainer = selectionItemPattern.Cached.SelectionContainer;
                    var selectionItemInfo = new SelectionItemInfo
                    {
                        AutomationId = element.Cached.AutomationId ?? "",
                        Name = element.Cached.Name ?? "",
                        ControlType = element.Cached.ControlType?.LocalizedControlType ?? "",
                        IsSelected = selectionItemPattern.Cached.IsSelected
                    };

                    if (selectionContainer != null)
                    {
                        selectionItemInfo.SelectionContainer = selectionContainer.Cached.AutomationId ?? "";
                    }

                    // Add to Selection info if not already present
                    if (details.Selection == null)
                    {
                        details.Selection = new SelectionInfo
                        {
                            SelectedItems = new List<SelectionItemInfo> { selectionItemInfo },
                            SelectedCount = selectionItemInfo.IsSelected ? 1 : 0
                        };
                    }
                }

                // Table Pattern - basic properties only
                if (element.TryGetCachedPattern(TablePattern.Pattern, out var tablePatternObj) && 
                    tablePatternObj is TablePattern tablePattern)
                {
                    details.Table = new TableInfo
                    {
                        RowCount = tablePattern.Cached.RowCount,
                        ColumnCount = tablePattern.Cached.ColumnCount,
                        RowOrColumnMajor = tablePattern.Cached.RowOrColumnMajor.ToString(),
                        ColumnHeaders = new List<HeaderInfo>(), // Skip complex header operations
                        RowHeaders = new List<HeaderInfo>() // Skip complex header operations
                    };
                }

                // Grid Pattern - basic properties only  
                if (element.TryGetCachedPattern(GridPattern.Pattern, out var gridPatternObj) && 
                    gridPatternObj is GridPattern gridPattern)
                {
                    bool canSelectMultiple = false;
                    if (element.TryGetCachedPattern(SelectionPattern.Pattern, out var selPatternObj) && 
                        selPatternObj is SelectionPattern selPattern)
                    {
                        canSelectMultiple = selPattern.Cached.CanSelectMultiple;
                    }

                    details.Grid = new GridInfo
                    {
                        RowCount = gridPattern.Cached.RowCount,
                        ColumnCount = gridPattern.Cached.ColumnCount,
                        CanSelectMultiple = canSelectMultiple
                    };
                }

                // Accessibility Information - basic cached properties only
                var accessibilityInfo = new AccessibilityInfo();
                bool hasAccessibilityInfo = false;

                // Basic accessibility properties from cached data
                var helpText = element.Cached.HelpText;
                if (!string.IsNullOrEmpty(helpText))
                {
                    accessibilityInfo.HelpText = helpText;
                    hasAccessibilityInfo = true;
                }

                var accessKey = element.Cached.AccessKey;
                if (!string.IsNullOrEmpty(accessKey))
                {
                    accessibilityInfo.AccessKey = accessKey;
                    hasAccessibilityInfo = true;
                }

                var acceleratorKey = element.Cached.AcceleratorKey;
                if (!string.IsNullOrEmpty(acceleratorKey))
                {
                    accessibilityInfo.AcceleratorKey = acceleratorKey;
                    hasAccessibilityInfo = true;
                }

                // LabeledBy information - only if cached
                try
                {
                    var labeledByProperty = element.GetCachedPropertyValue(AutomationElement.LabeledByProperty);
                    if (labeledByProperty is AutomationElement labeledByElement && labeledByElement != null)
                    {
                        accessibilityInfo.LabeledBy = new ElementReference
                        {
                            AutomationId = labeledByElement.Cached.AutomationId ?? "",
                            Name = labeledByElement.Cached.Name ?? "",
                            ControlType = labeledByElement.Cached.ControlType?.LocalizedControlType ?? ""
                        };
                        hasAccessibilityInfo = true;
                    }
                }
                catch
                {
                    // Skip LabeledBy if not available in cache
                }

                if (hasAccessibilityInfo)
                {
                    details.Accessibility = accessibilityInfo;
                }
            }
            catch
            {
                // Silent fail for pattern info - basic element info is still returned
                // No logging to avoid interfering with JSON communication
            }
        }

        private void SetPatternInfo(AutomationElement element, ElementDetails details)
        {
            try
            {
                // Value Pattern
                if (element.TryGetCachedPattern(ValuePattern.Pattern, out var valuePatternObj) && 
                    valuePatternObj is ValuePattern valuePattern)
                {
                    details.ValueInfo = new ValueInfo
                    {
                        Value = valuePattern.Cached.Value ?? "",
                        IsReadOnly = valuePattern.Cached.IsReadOnly
                    };
                }

                // Toggle Pattern
                if (element.TryGetCachedPattern(TogglePattern.Pattern, out var togglePatternObj) && 
                    togglePatternObj is TogglePattern togglePattern)
                {
                    var state = togglePattern.Cached.ToggleState;
                    details.Toggle = new ToggleInfo
                    {
                        State = state.ToString(),
                        IsToggled = state == ToggleState.On,
                        CanToggle = true
                    };
                }

                // Range Value Pattern
                if (element.TryGetCachedPattern(RangeValuePattern.Pattern, out var rangePatternObj) && 
                    rangePatternObj is RangeValuePattern rangePattern)
                {
                    details.Range = new RangeInfo
                    {
                        Value = rangePattern.Cached.Value,
                        Minimum = rangePattern.Cached.Minimum,
                        Maximum = rangePattern.Cached.Maximum,
                        SmallChange = rangePattern.Cached.SmallChange,
                        LargeChange = rangePattern.Cached.LargeChange,
                        IsReadOnly = rangePattern.Cached.IsReadOnly
                    };
                }

                // Window Pattern
                if (element.TryGetCachedPattern(WindowPattern.Pattern, out var windowPatternObj) && 
                    windowPatternObj is WindowPattern windowPattern)
                {
                    details.Window = new WindowPatternInfo
                    {
                        CanMaximize = windowPattern.Cached.CanMaximize,
                        CanMinimize = windowPattern.Cached.CanMinimize,
                        IsModal = windowPattern.Cached.IsModal,
                        IsTopmost = windowPattern.Cached.IsTopmost,
                        InteractionState = windowPattern.Cached.WindowInteractionState.ToString(),
                        VisualState = windowPattern.Cached.WindowVisualState.ToString()
                    };
                }

                // Selection Pattern - Simplified version without GetSelection
                if (element.TryGetCachedPattern(SelectionPattern.Pattern, out var selectionPatternObj) && 
                    selectionPatternObj is SelectionPattern selectionPattern)
                {
                    try
                    {
                        details.Selection = new SelectionInfo
                        {
                            CanSelectMultiple = selectionPattern.Cached.CanSelectMultiple,
                            IsSelectionRequired = selectionPattern.Cached.IsSelectionRequired,
                            SelectedCount = 0, // Skip complex GetSelection operation for now
                            SelectedItems = new List<SelectionItemInfo>() // Skip complex operations for now
                        };
                    }
                    catch
                    {
                        // If cached access fails, skip pattern info
                    }
                }

                // Grid Pattern
                if (element.TryGetCachedPattern(GridPattern.Pattern, out var gridPatternObj) && 
                    gridPatternObj is GridPattern gridPattern)
                {
                    bool canSelectMultiple = false;
                    if (element.TryGetCachedPattern(SelectionPattern.Pattern, out var selPatternObj) && 
                        selPatternObj is SelectionPattern selPattern)
                    {
                        canSelectMultiple = selPattern.Cached.CanSelectMultiple;
                    }

                    details.Grid = new GridInfo
                    {
                        RowCount = gridPattern.Cached.RowCount,
                        ColumnCount = gridPattern.Cached.ColumnCount,
                        CanSelectMultiple = canSelectMultiple
                    };
                }

                // Scroll Pattern
                if (element.TryGetCachedPattern(ScrollPattern.Pattern, out var scrollPatternObj) && 
                    scrollPatternObj is ScrollPattern scrollPattern)
                {
                    details.Scroll = new ScrollInfo
                    {
                        HorizontalPercent = scrollPattern.Cached.HorizontalScrollPercent,
                        VerticalPercent = scrollPattern.Cached.VerticalScrollPercent,
                        HorizontalViewSize = scrollPattern.Cached.HorizontalViewSize,
                        VerticalViewSize = scrollPattern.Cached.VerticalViewSize,
                        HorizontallyScrollable = scrollPattern.Cached.HorizontallyScrollable,
                        VerticallyScrollable = scrollPattern.Cached.VerticallyScrollable
                    };
                }

                // Text Pattern - Note: TextPattern operations require Current access, so we'll handle this differently
                if (element.TryGetCachedPattern(TextPattern.Pattern, out _))
                {
                    try
                    {
                        // Text pattern requires live access for operations
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPatternObj) && 
                            textPatternObj is TextPattern textPattern)
                        {
                            var documentRange = textPattern.DocumentRange;
                            var text = documentRange.GetText(-1);
                            
                            var selections = textPattern.GetSelection();
                            var selectedText = "";
                            var hasSelection = false;
                            
                            if (selections.Length > 0)
                            {
                                selectedText = selections[0].GetText(-1) ?? "";
                                hasSelection = !string.IsNullOrEmpty(selectedText);
                            }

                            details.Text = new TextInfo
                            {
                                Text = text ?? "",
                                Length = text?.Length ?? 0,
                                SelectedText = selectedText,
                                HasSelection = hasSelection
                            };
                        }
                    }
                    catch
                    {
                        // If Current access fails, skip text pattern info
                    }
                }

                // Transform Pattern
                if (element.TryGetCachedPattern(TransformPattern.Pattern, out var transformPatternObj) && 
                    transformPatternObj is TransformPattern transformPattern)
                {
                    var boundingRect = element.Cached.BoundingRectangle;
                    details.Transform = new TransformInfo
                    {
                        CanMove = transformPattern.Cached.CanMove,
                        CanResize = transformPattern.Cached.CanResize,
                        CanRotate = transformPattern.Cached.CanRotate,
                        CurrentX = boundingRect.X,
                        CurrentY = boundingRect.Y,
                        CurrentWidth = boundingRect.Width,
                        CurrentHeight = boundingRect.Height
                    };
                }

                // ExpandCollapse Pattern
                if (element.TryGetCachedPattern(ExpandCollapsePattern.Pattern, out var expandCollapsePatternObj) && 
                    expandCollapsePatternObj is ExpandCollapsePattern expandCollapsePattern)
                {
                    details.ExpandCollapse = new ExpandCollapseInfo
                    {
                        State = expandCollapsePattern.Cached.ExpandCollapseState.ToString()
                    };
                }

                // Dock Pattern
                if (element.TryGetCachedPattern(DockPattern.Pattern, out var dockPatternObj) && 
                    dockPatternObj is DockPattern dockPattern)
                {
                    details.Dock = new DockInfo
                    {
                        Position = dockPattern.Cached.DockPosition.ToString()
                    };
                }

                // MultipleView Pattern - Simplified version without GetViewName
                if (element.TryGetCachedPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) && 
                    multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                {
                    try
                    {
                        details.MultipleView = new MultipleViewInfo
                        {
                            CurrentView = multipleViewPattern.Cached.CurrentView,
                            AvailableViews = new List<PatternViewInfo>() // Skip complex operations for now
                        };
                    }
                    catch
                    {
                        // If cached access fails, skip pattern info
                    }
                }

                // GridItem Pattern
                if (element.TryGetCachedPattern(GridItemPattern.Pattern, out var gridItemPatternObj) && 
                    gridItemPatternObj is GridItemPattern gridItemPattern)
                {
                    var containingGrid = gridItemPattern.Cached.ContainingGrid;
                    details.GridItem = new GridItemInfo
                    {
                        Row = gridItemPattern.Cached.Row,
                        Column = gridItemPattern.Cached.Column,
                        RowSpan = gridItemPattern.Cached.RowSpan,
                        ColumnSpan = gridItemPattern.Cached.ColumnSpan,
                        ContainingGrid = containingGrid?.Cached.AutomationId ?? ""
                    };
                }

                // Table Pattern - Simplified version without header operations
                if (element.TryGetCachedPattern(TablePattern.Pattern, out var tablePatternObj) && 
                    tablePatternObj is TablePattern tablePattern)
                {
                    try
                    {
                        details.Table = new TableInfo
                        {
                            RowCount = tablePattern.Cached.RowCount,
                            ColumnCount = tablePattern.Cached.ColumnCount,
                            RowOrColumnMajor = tablePattern.Cached.RowOrColumnMajor.ToString(),
                            ColumnHeaders = new List<HeaderInfo>(), // Skip complex operations for now
                            RowHeaders = new List<HeaderInfo>() // Skip complex operations for now
                        };
                    }
                    catch
                    {
                        // If cached access fails, skip pattern info
                    }
                }

                // TableItem Pattern - Simplified version without header operations
                if (element.TryGetCachedPattern(TableItemPattern.Pattern, out var tableItemPatternObj) && 
                    tableItemPatternObj is TableItemPattern tableItemPattern)
                {
                    try
                    {
                        details.TableItem = new TableItemInfo
                        {
                            ColumnHeaders = new List<HeaderInfo>(), // Skip complex operations for now
                            RowHeaders = new List<HeaderInfo>() // Skip complex operations for now
                        };
                    }
                    catch
                    {
                        // If cached access fails, skip pattern info
                    }
                }

                // SelectionItem Pattern
                if (element.TryGetCachedPattern(SelectionItemPattern.Pattern, out var selectionItemPatternObj) && 
                    selectionItemPatternObj is SelectionItemPattern selectionItemPattern)
                {
                    var selectionContainer = selectionItemPattern.Cached.SelectionContainer;
                    var selectionItemInfo = new SelectionItemInfo
                    {
                        AutomationId = element.Cached.AutomationId ?? "",
                        Name = element.Cached.Name ?? "",
                        ControlType = element.Cached.ControlType?.LocalizedControlType ?? "",
                        IsSelected = selectionItemPattern.Cached.IsSelected
                    };

                    if (selectionContainer != null)
                    {
                        selectionItemInfo.SelectionContainer = selectionContainer.Cached.AutomationId ?? "";
                    }

                    // SelectionInfo にも含める
                    if (details.Selection == null)
                    {
                        details.Selection = new SelectionInfo
                        {
                            SelectedItems = new List<SelectionItemInfo> { selectionItemInfo },
                            SelectedCount = selectionItemInfo.IsSelected ? 1 : 0
                        };
                    }
                }

                // Invoke Pattern
                if (element.TryGetCachedPattern(InvokePattern.Pattern, out _))
                {
                    details.Invoke = new InvokeInfo
                    {
                        IsInvokable = true
                    };
                }

                // ScrollItem Pattern
                if (element.TryGetCachedPattern(ScrollItemPattern.Pattern, out _))
                {
                    details.ScrollItem = new ScrollItemInfo
                    {
                        IsScrollable = true
                    };
                }

                // VirtualizedItem Pattern
                if (element.TryGetCachedPattern(VirtualizedItemPattern.Pattern, out _))
                {
                    details.VirtualizedItem = new VirtualizedItemInfo
                    {
                        IsVirtualized = true
                    };
                }

                // ItemContainer Pattern
                if (element.TryGetCachedPattern(ItemContainerPattern.Pattern, out _))
                {
                    details.ItemContainer = new ItemContainerInfo
                    {
                        IsItemContainer = true
                    };
                }

                // SynchronizedInput Pattern
                if (element.TryGetCachedPattern(SynchronizedInputPattern.Pattern, out _))
                {
                    details.SynchronizedInput = new SynchronizedInputInfo
                    {
                        SupportsSynchronizedInput = true
                    };
                }

                // Accessibility情報
                var accessibilityInfo = new AccessibilityInfo();
                bool hasAccessibilityInfo = false;

                // LabeledBy情報
                try
                {
                    var labeledByProperty = element.GetCachedPropertyValue(AutomationElement.LabeledByProperty);
                    if (labeledByProperty is AutomationElement labeledByElement && labeledByElement != null)
                    {
                        accessibilityInfo.LabeledBy = new ElementReference
                        {
                            AutomationId = labeledByElement.Cached.AutomationId ?? "",
                            Name = labeledByElement.Cached.Name ?? "",
                            ControlType = labeledByElement.Cached.ControlType?.LocalizedControlType ?? ""
                        };
                        hasAccessibilityInfo = true;
                    }
                }
                catch { }

                // その他のアクセシビリティ情報
                try
                {
                    var helpText = element.Cached.HelpText;
                    if (!string.IsNullOrEmpty(helpText))
                    {
                        accessibilityInfo.HelpText = helpText;
                        hasAccessibilityInfo = true;
                    }

                    var accessKey = element.Cached.AccessKey;
                    if (!string.IsNullOrEmpty(accessKey))
                    {
                        accessibilityInfo.AccessKey = accessKey;
                        hasAccessibilityInfo = true;
                    }

                    var acceleratorKey = element.Cached.AcceleratorKey;
                    if (!string.IsNullOrEmpty(acceleratorKey))
                    {
                        accessibilityInfo.AcceleratorKey = acceleratorKey;
                        hasAccessibilityInfo = true;
                    }
                }
                catch { }

                if (hasAccessibilityInfo)
                {
                    details.Accessibility = accessibilityInfo;
                }
            }
            catch (Exception ex)
            {
                // パターン情報の取得でエラーが発生しても、基本的な要素情報は返す
                Console.WriteLine($"Error setting pattern info: {ex.Message}");
            }
        }

        private string CreateSearchCriteriaString(Dictionary<string, object>? parameters)
        {
            if (parameters == null) return "No criteria specified";
            
            var criteria = new List<string>();
            foreach (var param in parameters)
            {
                if (param.Value != null)
                    criteria.Add($"{param.Key}: {param.Value}");
            }
            return string.Join(", ", criteria);
        }

        private Dictionary<string, object> CreateParametersDictionary(FindElementsRequest request)
        {
            return new Dictionary<string, object>
            {
                ["searchText"] = request.SearchText ?? "",
                ["controlType"] = request.ControlType ?? "",
                ["windowTitle"] = request.WindowTitle ?? "",
                ["processId"] = request.ProcessId ?? 0,
                ["scope"] = request.Scope ?? "",
                ["className"] = request.ClassName ?? "",
                ["maxResults"] = request.MaxResults,
                ["useCache"] = request.UseCache,
                ["useRegex"] = request.UseRegex,
                ["useWildcard"] = request.UseWildcard
            };
        }

        private SearchCriteria CreateSearchCriteria(Dictionary<string, object>? parameters)
        {
            if (parameters == null) return new SearchCriteria();

            return new SearchCriteria
            {
                SearchText = parameters.GetValueOrDefault("searchText")?.ToString(),
                ControlType = parameters.GetValueOrDefault("controlType")?.ToString(),
                WindowTitle = parameters.GetValueOrDefault("windowTitle")?.ToString(),
                ProcessId = parameters.GetValueOrDefault("processId")?.ToString() is string pidStr && 
                    int.TryParse(pidStr, out var pid) ? pid : null,
                Scope = parameters.GetValueOrDefault("scope")?.ToString(),
                PatternType = parameters.GetValueOrDefault("patternType")?.ToString(),
                AdditionalCriteria = new Dictionary<string, object>
                {
                    ["className"] = parameters.GetValueOrDefault("className")?.ToString() ?? "",
                    ["helpText"] = parameters.GetValueOrDefault("helpText")?.ToString() ?? "",
                    ["acceleratorKey"] = parameters.GetValueOrDefault("acceleratorKey")?.ToString() ?? "",
                    ["accessKey"] = parameters.GetValueOrDefault("accessKey")?.ToString() ?? "",
                    ["searchMethod"] = parameters.GetValueOrDefault("searchMethod")?.ToString() ?? "",
                    ["conditionOperator"] = parameters.GetValueOrDefault("conditionOperator")?.ToString() ?? "",
                    ["excludeText"] = parameters.GetValueOrDefault("excludeText")?.ToString() ?? "",
                    ["excludeControlType"] = parameters.GetValueOrDefault("excludeControlType")?.ToString() ?? ""
                }
            };
        }

    }
}