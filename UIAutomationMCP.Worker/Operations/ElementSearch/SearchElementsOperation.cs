using System.Diagnostics;
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
    public class SearchElementsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SearchElementsOperation>? _logger;

        public SearchElementsOperation(ElementFinderService elementFinderService, ILogger<SearchElementsOperation>? logger = null)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }
        public async Task<OperationResult<SearchElementsResult>> ExecuteAsync(string parametersJson)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var request = JsonSerializationHelper.Deserialize<SearchElementsRequest>(parametersJson);
                if (request == null)
                {
                    return new OperationResult<SearchElementsResult>
                    {
                        Success = false,
                        Error = "Failed to deserialize SearchElementsRequest"
                    };
                }

                // 既存のUIAutomationEnvironment.ExecuteWithTimeoutAsyncを使用（非同期操作用）
                SearchElementsResult result;
                try
                {
                    result = await UIAutomationEnvironment.ExecuteWithTimeoutAsync(
                        () => PerformSearchAsync(request), 
                        "SearchElements", 
                        request.TimeoutSeconds);
                }
                catch (TimeoutException ex)
                {
                    return new OperationResult<SearchElementsResult>
                    {
                        Success = false,
                        Error = ex.Message,
                        ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                    };
                }

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                
                return new OperationResult<SearchElementsResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new OperationResult<SearchElementsResult>
                {
                    Success = false,
                    Error = $"Operation failed: {ex.Message}",
                    ExecutionSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
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

        private Task<SearchElementsResult> PerformSearchAsync(SearchElementsRequest request)
        {
            var searchStopwatch = Stopwatch.StartNew();
            
            try
            {
                // UI Automation availability check
                if (!UIAutomationEnvironment.IsAvailable)
                {
                    throw new InvalidOperationException($"UI Automation is not available: {UIAutomationEnvironment.UnavailabilityReason}");
                }

                // Convert request to advanced search parameters
                var searchParams = ConvertToAdvancedSearchParameters(request);

                // Perform advanced search using ElementFinderService
                var foundElementsCollection = _elementFinderService.FindElementsAdvanced(searchParams);

                // Convert to list for further processing
                var foundElementsList = new List<AutomationElement>();
                foreach (AutomationElement element in foundElementsCollection)
                {
                    if (element != null)
                        foundElementsList.Add(element);
                }

                // Apply fuzzy matching if needed
                if (request.FuzzyMatch)
                {
                    foundElementsList = _elementFinderService.ApplyFuzzyFilter(foundElementsCollection, searchParams);
                }

                // Apply pattern filtering
                foundElementsList = _elementFinderService.ApplyPatternFilter(foundElementsList, searchParams);

                // Apply sorting if requested
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    foundElementsList = _elementFinderService.SortElements(foundElementsList, request.SortBy);
                }

                // Apply result limits
                var totalFound = foundElementsList.Count;
                if (foundElementsList.Count > request.MaxResults)
                {
                    foundElementsList = foundElementsList.Take(request.MaxResults).ToList();
                }

                // Convert to ElementInfo array with optional details
                var elements = ConvertToElementInfoArray(foundElementsList, request);

                searchStopwatch.Stop();

                return Task.FromResult(new SearchElementsResult
                {
                    Success = true,
                    OperationName = "SearchElements",
                    Elements = elements,
                    Metadata = new SearchMetadata
                    {
                        TotalFound = totalFound,
                        Returned = elements.Length,
                        SearchDuration = searchStopwatch.Elapsed,
                        SearchCriteria = BuildSearchCriteria(request),
                        WasTruncated = totalFound > request.MaxResults,
                        SuggestedRefinements = GenerateSuggestedRefinements(request, totalFound),
                        ExecutedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                searchStopwatch.Stop();
                _logger?.LogError(ex, "SearchElements operation failed");
                
                throw new InvalidOperationException($"Search operation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// SearchElementsRequestをAdvancedSearchParametersに変換
        /// </summary>
        private AdvancedSearchParameters ConvertToAdvancedSearchParameters(SearchElementsRequest request)
        {
            var searchParams = new AdvancedSearchParameters
            {
                SearchText = request.SearchText,
                AutomationId = request.AutomationId,
                Name = request.Name,
                ClassName = request.ClassName,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                ProcessId = request.ProcessId,
                Scope = request.Scope,
                VisibleOnly = request.VisibleOnly,
                FuzzyMatch = request.FuzzyMatch,
                EnabledOnly = request.EnabledOnly,
                SortBy = request.SortBy,
                TimeoutMs = request.TimeoutSeconds * 1000,
                CacheRequest = CreateCacheRequest()
            };

            // Parse RequiredPatterns
            if (!string.IsNullOrEmpty(request.RequiredPatterns))
            {
                searchParams.RequiredPatterns = request.RequiredPatterns
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();
            }

            // Parse AnyOfPatterns
            if (!string.IsNullOrEmpty(request.AnyOfPatterns))
            {
                searchParams.AnyOfPatterns = request.AnyOfPatterns
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();
            }

            return searchParams;
        }

        private CacheRequest CreateCacheRequest()
        {
            var cacheRequest = new CacheRequest();
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.ClassNameProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.IsEnabledProperty);
            cacheRequest.Add(AutomationElement.ProcessIdProperty);
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);
            cacheRequest.Add(AutomationElement.IsOffscreenProperty);
            cacheRequest.Add(AutomationElement.FrameworkIdProperty);
            cacheRequest.Add(AutomationElement.HelpTextProperty);
            cacheRequest.Add(AutomationElement.HasKeyboardFocusProperty);
            cacheRequest.Add(AutomationElement.IsKeyboardFocusableProperty);
            cacheRequest.Add(AutomationElement.IsPasswordProperty);
            
            // Add accessibility properties
            cacheRequest.Add(AutomationElement.AcceleratorKeyProperty);
            cacheRequest.Add(AutomationElement.AccessKeyProperty);
            cacheRequest.Add(AutomationElement.ItemTypeProperty);
            cacheRequest.Add(AutomationElement.ItemStatusProperty);
            cacheRequest.Add(AutomationElement.CultureProperty);
            cacheRequest.Add(AutomationElement.OrientationProperty);
            
            // Add pattern properties
            cacheRequest.Add(ValuePattern.ValueProperty);
            cacheRequest.Add(ValuePattern.IsReadOnlyProperty);
            cacheRequest.Add(TogglePattern.ToggleStateProperty);
            cacheRequest.Add(SelectionPattern.CanSelectMultipleProperty);
            cacheRequest.Add(SelectionPattern.IsSelectionRequiredProperty);
            cacheRequest.Add(RangeValuePattern.ValueProperty);
            cacheRequest.Add(RangeValuePattern.MinimumProperty);
            cacheRequest.Add(RangeValuePattern.MaximumProperty);
            cacheRequest.Add(RangeValuePattern.SmallChangeProperty);
            cacheRequest.Add(RangeValuePattern.LargeChangeProperty);
            cacheRequest.Add(RangeValuePattern.IsReadOnlyProperty);
            cacheRequest.Add(GridPattern.RowCountProperty);
            cacheRequest.Add(GridPattern.ColumnCountProperty);
            cacheRequest.Add(TablePattern.RowCountProperty);
            cacheRequest.Add(TablePattern.ColumnCountProperty);
            cacheRequest.Add(TablePattern.RowOrColumnMajorProperty);
            cacheRequest.Add(ScrollPattern.HorizontalScrollPercentProperty);
            cacheRequest.Add(ScrollPattern.VerticalScrollPercentProperty);
            cacheRequest.Add(ScrollPattern.HorizontalViewSizeProperty);
            cacheRequest.Add(ScrollPattern.VerticalViewSizeProperty);
            cacheRequest.Add(ScrollPattern.HorizontallyScrollableProperty);
            cacheRequest.Add(ScrollPattern.VerticallyScrollableProperty);
            cacheRequest.Add(TransformPattern.CanMoveProperty);
            cacheRequest.Add(TransformPattern.CanResizeProperty);
            cacheRequest.Add(TransformPattern.CanRotateProperty);
            cacheRequest.Add(WindowPattern.WindowVisualStateProperty);
            cacheRequest.Add(WindowPattern.WindowInteractionStateProperty);
            cacheRequest.Add(WindowPattern.IsModalProperty);
            cacheRequest.Add(WindowPattern.IsTopmostProperty);
            cacheRequest.Add(WindowPattern.CanMaximizeProperty);
            cacheRequest.Add(WindowPattern.CanMinimizeProperty);
            cacheRequest.Add(ExpandCollapsePattern.ExpandCollapseStateProperty);
            cacheRequest.Add(DockPattern.DockPositionProperty);
            cacheRequest.Add(MultipleViewPattern.CurrentViewProperty);
            cacheRequest.Add(GridItemPattern.RowProperty);
            cacheRequest.Add(GridItemPattern.ColumnProperty);
            cacheRequest.Add(GridItemPattern.RowSpanProperty);
            cacheRequest.Add(GridItemPattern.ColumnSpanProperty);
            cacheRequest.Add(GridItemPattern.ContainingGridProperty);
            cacheRequest.Add(TableItemPattern.RowProperty);
            cacheRequest.Add(TableItemPattern.ColumnProperty);
            cacheRequest.Add(TableItemPattern.RowSpanProperty);
            cacheRequest.Add(TableItemPattern.ColumnSpanProperty);
            
            cacheRequest.AutomationElementMode = AutomationElementMode.None;
            cacheRequest.TreeFilter = Automation.RawViewCondition;
            return cacheRequest;
        }

        private ElementInfo[] ConvertToElementInfoArray(List<AutomationElement> elements, SearchElementsRequest request)
        {
            var result = new List<ElementInfo>();

            foreach (var element in elements)
            {
                if (element != null)
                {
                    try
                    {
                        var elementInfo = CreateElementInfoFromAutomationElement(element, request.IncludeDetails);
                        result.Add(elementInfo);
                    }
                    catch (ElementNotAvailableException)
                    {
                        // Element is no longer available, skip it
                        continue;
                    }
                }
            }

            return result.ToArray();
        }

        private ElementInfo CreateElementInfoFromAutomationElement(AutomationElement element, bool includeDetails)
        {
            var elementInfo = new ElementInfo
            {
                AutomationId = element.Cached.AutomationId ?? "",
                Name = element.Cached.Name ?? "",
                ControlType = element.Cached.ControlType.LocalizedControlType ?? "",
                LocalizedControlType = element.Cached.ControlType.LocalizedControlType ?? "",
                IsEnabled = element.Cached.IsEnabled,
                IsVisible = !element.Cached.IsOffscreen,
                IsOffscreen = element.Cached.IsOffscreen,
                ProcessId = element.Cached.ProcessId,
                ClassName = element.Cached.ClassName ?? "",
                FrameworkId = element.Cached.FrameworkId ?? "",
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Cached.BoundingRectangle.X,
                    Y = element.Cached.BoundingRectangle.Y,
                    Width = element.Cached.BoundingRectangle.Width,
                    Height = element.Cached.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element)
            };

            // Include details if requested
            if (includeDetails)
            {
                elementInfo.Details = CreateElementDetails(element);
            }

            return elementInfo;
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

            // Set pattern information safely
            SetPatternInfo(element, details);
            
            return details;
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
                    details.Toggle = new ToggleInfo
                    {
                        State = togglePattern.Cached.ToggleState.ToString(),
                        CanToggle = true
                    };
                }

                // Selection Pattern
                if (element.TryGetCachedPattern(SelectionPattern.Pattern, out var selectionPatternObj) && 
                    selectionPatternObj is SelectionPattern selectionPattern)
                {
                    details.Selection = new SelectionInfo
                    {
                        CanSelectMultiple = selectionPattern.Cached.CanSelectMultiple,
                        IsSelectionRequired = selectionPattern.Cached.IsSelectionRequired
                    };
                }

                // Range Pattern
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

                // Grid Pattern
                if (element.TryGetCachedPattern(GridPattern.Pattern, out var gridPatternObj) && 
                    gridPatternObj is GridPattern gridPattern)
                {
                    details.Grid = new GridInfo
                    {
                        RowCount = gridPattern.Cached.RowCount,
                        ColumnCount = gridPattern.Cached.ColumnCount
                    };
                }

                // Table Pattern
                if (element.TryGetCachedPattern(TablePattern.Pattern, out var tablePatternObj) && 
                    tablePatternObj is TablePattern tablePattern)
                {
                    var rowHeaders = new List<HeaderInfo>();
                    var columnHeaders = new List<HeaderInfo>();
                    
                    try
                    {
                        var rowHeaderElements = tablePattern.Cached.GetRowHeaders();
                        foreach (var header in rowHeaderElements)
                        {
                            rowHeaders.Add(new HeaderInfo
                            {
                                AutomationId = header.Cached.AutomationId ?? "",
                                Name = header.Cached.Name ?? "",
                                ControlType = header.Cached.ControlType.LocalizedControlType ?? ""
                            });
                        }
                        
                        var columnHeaderElements = tablePattern.Cached.GetColumnHeaders();
                        foreach (var header in columnHeaderElements)
                        {
                            columnHeaders.Add(new HeaderInfo
                            {
                                AutomationId = header.Cached.AutomationId ?? "",
                                Name = header.Cached.Name ?? "",
                                ControlType = header.Cached.ControlType.LocalizedControlType ?? ""
                            });
                        }
                    }
                    catch { }

                    details.Table = new TableInfo
                    {
                        RowCount = tablePattern.Cached.RowCount,
                        ColumnCount = tablePattern.Cached.ColumnCount,
                        RowOrColumnMajor = tablePattern.Cached.RowOrColumnMajor.ToString(),
                        RowHeaders = rowHeaders,
                        ColumnHeaders = columnHeaders
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

                // Transform Pattern
                if (element.TryGetCachedPattern(TransformPattern.Pattern, out var transformPatternObj) && 
                    transformPatternObj is TransformPattern transformPattern)
                {
                    details.Transform = new TransformInfo
                    {
                        CanMove = transformPattern.Cached.CanMove,
                        CanResize = transformPattern.Cached.CanResize,
                        CanRotate = transformPattern.Cached.CanRotate
                    };
                }

                // Window Pattern
                if (element.TryGetCachedPattern(WindowPattern.Pattern, out var windowPatternObj) && 
                    windowPatternObj is WindowPattern windowPattern)
                {
                    details.Window = new WindowPatternInfo
                    {
                        VisualState = windowPattern.Cached.WindowVisualState.ToString(),
                        InteractionState = windowPattern.Cached.WindowInteractionState.ToString(),
                        IsModal = windowPattern.Cached.IsModal,
                        IsTopmost = windowPattern.Cached.IsTopmost,
                        CanMaximize = windowPattern.Cached.CanMaximize,
                        CanMinimize = windowPattern.Cached.CanMinimize
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

                // MultipleView Pattern
                if (element.TryGetCachedPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) && 
                    multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                {
                    var availableViews = new List<PatternViewInfo>();
                    try
                    {
                        var viewIds = multipleViewPattern.Cached.GetSupportedViews();
                        foreach (var viewId in viewIds)
                        {
                            string viewName = "";
                            try
                            {
                                viewName = multipleViewPattern.GetViewName(viewId);
                            }
                            catch { }
                            
                            availableViews.Add(new PatternViewInfo
                            {
                                ViewId = viewId,
                                ViewName = viewName
                            });
                        }
                    }
                    catch { }

                    details.MultipleView = new MultipleViewInfo
                    {
                        CurrentView = multipleViewPattern.Cached.CurrentView,
                        AvailableViews = availableViews
                    };
                }

                // Text Pattern
                if (element.TryGetCachedPattern(TextPattern.Pattern, out var textPatternObj) && 
                    textPatternObj is TextPattern textPattern)
                {
                    string textValue = "";
                    string selectedText = "";
                    try
                    {
                        textValue = textPattern.DocumentRange.GetText(-1);
                        var selections = textPattern.GetSelection();
                        if (selections.Length > 0)
                        {
                            selectedText = selections[0].GetText(-1);
                        }
                    }
                    catch { }

                    details.Text = new TextInfo
                    {
                        Text = textValue,
                        Length = textValue.Length,
                        SelectedText = selectedText,
                        HasSelection = !string.IsNullOrEmpty(selectedText)
                    };
                }

                // GridItem Pattern
                if (element.TryGetCachedPattern(GridItemPattern.Pattern, out var gridItemPatternObj) && 
                    gridItemPatternObj is GridItemPattern gridItemPattern)
                {
                    details.GridItem = new GridItemInfo
                    {
                        Row = gridItemPattern.Cached.Row,
                        Column = gridItemPattern.Cached.Column,
                        RowSpan = gridItemPattern.Cached.RowSpan,
                        ColumnSpan = gridItemPattern.Cached.ColumnSpan,
                        ContainingGrid = gridItemPattern.Cached.ContainingGrid?.Cached.Name ?? ""
                    };
                }

                // TableItem Pattern
                if (element.TryGetCachedPattern(TableItemPattern.Pattern, out var tableItemPatternObj) && 
                    tableItemPatternObj is TableItemPattern tableItemPattern)
                {
                    var rowHeaderItems = new List<HeaderInfo>();
                    var columnHeaderItems = new List<HeaderInfo>();
                    
                    try
                    {
                        var rowHeaders = tableItemPattern.Cached.GetRowHeaderItems();
                        foreach (var header in rowHeaders)
                        {
                            rowHeaderItems.Add(new HeaderInfo
                            {
                                AutomationId = header.Cached.AutomationId ?? "",
                                Name = header.Cached.Name ?? "",
                                ControlType = header.Cached.ControlType.LocalizedControlType ?? ""
                            });
                        }
                        
                        var columnHeaders = tableItemPattern.Cached.GetColumnHeaderItems();
                        foreach (var header in columnHeaders)
                        {
                            columnHeaderItems.Add(new HeaderInfo
                            {
                                AutomationId = header.Cached.AutomationId ?? "",
                                Name = header.Cached.Name ?? "",
                                ControlType = header.Cached.ControlType.LocalizedControlType ?? ""
                            });
                        }
                    }
                    catch { }

                    details.TableItem = new TableItemInfo
                    {
                        RowHeaders = rowHeaderItems,
                        ColumnHeaders = columnHeaderItems
                    };
                }

                // Invoke Pattern
                if (element.GetSupportedPatterns().Contains(InvokePattern.Pattern))
                {
                    details.Invoke = new InvokeInfo
                    {
                        IsInvokable = true
                    };
                }

                // ScrollItem Pattern  
                if (element.GetSupportedPatterns().Contains(ScrollItemPattern.Pattern))
                {
                    details.ScrollItem = new ScrollItemInfo
                    {
                        IsScrollable = true
                    };
                }

                // VirtualizedItem Pattern
                if (element.GetSupportedPatterns().Contains(VirtualizedItemPattern.Pattern))
                {
                    details.VirtualizedItem = new VirtualizedItemInfo
                    {
                        IsVirtualized = true
                    };
                }

                // ItemContainer Pattern
                if (element.GetSupportedPatterns().Contains(ItemContainerPattern.Pattern))
                {
                    details.ItemContainer = new ItemContainerInfo
                    {
                        IsItemContainer = true
                    };
                }

                // SynchronizedInput Pattern
                if (element.GetSupportedPatterns().Contains(SynchronizedInputPattern.Pattern))
                {
                    details.SynchronizedInput = new SynchronizedInputInfo
                    {
                        SupportsSynchronizedInput = true
                    };
                }

                // Accessibility Information
                details.Accessibility = new AccessibilityInfo
                {
                    AcceleratorKey = element.Cached.AcceleratorKey ?? "",
                    AccessKey = element.Cached.AccessKey ?? "",
                    HelpText = element.Cached.HelpText ?? ""
                };

            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to retrieve pattern information for element");
            }
        }

        private string[] GenerateSuggestedRefinements(SearchElementsRequest request, int totalFound)
        {
            var suggestions = new List<string>();

            if (totalFound == 0)
            {
                suggestions.Add("Try broadening search criteria");
                
                if (!string.IsNullOrEmpty(request.ControlType))
                {
                    suggestions.Add("Remove ControlType filter");
                }
                
                if (request.VisibleOnly)
                {
                    suggestions.Add("Include hidden elements (VisibleOnly=false)");
                }
                
                if (request.EnabledOnly)
                {
                    suggestions.Add("Include disabled elements (EnabledOnly=false)");
                }
                
                if (!string.IsNullOrEmpty(request.RequiredPatterns))
                {
                    suggestions.Add("Remove RequiredPatterns filter");
                }
                
                if (!request.FuzzyMatch && !string.IsNullOrEmpty(request.SearchText))
                {
                    suggestions.Add("Try FuzzyMatch=true for partial matching");
                }
                
                if (request.Scope == "children")
                {
                    suggestions.Add("Expand scope to 'descendants' or 'subtree'");
                }
            }
            else if (totalFound > request.MaxResults)
            {
                suggestions.Add($"Consider increasing MaxResults (current: {request.MaxResults})");
                suggestions.Add("Add more specific search criteria");
                
                if (string.IsNullOrEmpty(request.ControlType))
                {
                    suggestions.Add("Add ControlType filter");
                }
                
                if (string.IsNullOrEmpty(request.RequiredPatterns))
                {
                    suggestions.Add("Add RequiredPatterns filter");
                }
                
                if (!request.VisibleOnly)
                {
                    suggestions.Add("Filter to visible elements only (VisibleOnly=true)");
                }
            }
            else
            {
                if (!request.IncludeDetails)
                {
                    suggestions.Add("Use IncludeDetails=true for more information");
                }
                
                if (string.IsNullOrEmpty(request.SortBy))
                {
                    suggestions.Add("Try sorting by 'name', 'controltype', 'position', or 'size'");
                }
            }

            return suggestions.ToArray();
        }

        private string BuildSearchCriteria(SearchElementsRequest request)
        {
            var criteria = new List<string>();
            
            if (!string.IsNullOrEmpty(request.SearchText))
                criteria.Add($"SearchText='{request.SearchText}'");
            if (!string.IsNullOrEmpty(request.AutomationId))
                criteria.Add($"AutomationId='{request.AutomationId}'");
            if (!string.IsNullOrEmpty(request.Name))
                criteria.Add($"Name='{request.Name}'");
            if (!string.IsNullOrEmpty(request.ClassName))
                criteria.Add($"ClassName='{request.ClassName}'");
            if (!string.IsNullOrEmpty(request.ControlType))
                criteria.Add($"ControlType='{request.ControlType}'");
            if (!string.IsNullOrEmpty(request.Scope))
                criteria.Add($"Scope='{request.Scope}'");
            if (!string.IsNullOrEmpty(request.RequiredPatterns))
                criteria.Add($"RequiredPatterns='{request.RequiredPatterns}'");
            if (!string.IsNullOrEmpty(request.AnyOfPatterns))
                criteria.Add($"AnyOfPatterns='{request.AnyOfPatterns}'");
            if (request.VisibleOnly)
                criteria.Add("VisibleOnly=true");
            if (request.FuzzyMatch)
                criteria.Add("FuzzyMatch=true");
            if (request.EnabledOnly)
                criteria.Add("EnabledOnly=true");
            if (!string.IsNullOrEmpty(request.SortBy))
                criteria.Add($"SortBy='{request.SortBy}'");
            if (request.IncludeDetails)
                criteria.Add("IncludeDetails=true");
            
            return string.Join(", ", criteria);
        }
    }
}