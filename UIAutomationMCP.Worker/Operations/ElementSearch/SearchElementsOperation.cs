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

                // タイムアウト処理はSubprocessExecutorで行うため、直接実行
                SearchElementsResult result = await PerformSearchAsync(request);

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
            
            // Add patterns themselves for supportedPatterns detection
            cacheRequest.Add(ValuePattern.Pattern);
            cacheRequest.Add(TogglePattern.Pattern);
            cacheRequest.Add(SelectionPattern.Pattern);
            cacheRequest.Add(RangeValuePattern.Pattern);
            cacheRequest.Add(GridPattern.Pattern);
            cacheRequest.Add(TablePattern.Pattern);
            cacheRequest.Add(ScrollPattern.Pattern);
            cacheRequest.Add(TransformPattern.Pattern);
            cacheRequest.Add(WindowPattern.Pattern);
            cacheRequest.Add(ExpandCollapsePattern.Pattern);
            cacheRequest.Add(DockPattern.Pattern);
            cacheRequest.Add(MultipleViewPattern.Pattern);
            cacheRequest.Add(TextPattern.Pattern);
            cacheRequest.Add(GridItemPattern.Pattern);
            cacheRequest.Add(TableItemPattern.Pattern);
            cacheRequest.Add(InvokePattern.Pattern);
            cacheRequest.Add(ScrollItemPattern.Pattern);
            cacheRequest.Add(VirtualizedItemPattern.Pattern);
            cacheRequest.Add(ItemContainerPattern.Pattern);
            cacheRequest.Add(SynchronizedInputPattern.Pattern);
            
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
                        var elementInfo = ElementInfoBuilder.CreateElementInfoFromCached(element, request.IncludeDetails, _logger);
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


        // Note: Element info creation has been moved to ElementInfoBuilder helper class

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