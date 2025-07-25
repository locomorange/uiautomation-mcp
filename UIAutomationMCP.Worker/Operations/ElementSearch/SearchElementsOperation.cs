using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.UIAutomation.Helpers;
using UIAutomationMCP.Worker.Extensions;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class SearchElementsOperation : BaseUIAutomationOperation<SearchElementsRequest, SearchElementsResult>
    {
        public SearchElementsOperation(ElementFinderService elementFinderService, ILogger<SearchElementsOperation> logger)
            : base(elementFinderService, logger)
        {
        }
        protected override Core.Validation.ValidationResult ValidateRequest(SearchElementsRequest request)
        {
            if (request.MaxResults <= 0)
            {
                return Core.Validation.ValidationResult.Failure("MaxResults must be greater than 0");
            }

            // Allow empty search criteria as it's valid for global search
            return Core.Validation.ValidationResult.Success;
        }

        protected override async Task<SearchElementsResult> ExecuteOperationAsync(SearchElementsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // タイムアウト処理はSubprocessExecutorで行うため、直接実行
                SearchElementsResult result = await PerformSearchAsync(request);

                stopwatch.Stop();
                result.ExecutionTime = stopwatch.Elapsed;
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "SearchElements operation failed");
                throw new UIAutomationElementNotFoundException("Operation", null, $"Operation failed: {ex.Message}");
            }
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
        /// Convert AutomationElements to BasicElementInfo array
        /// </summary>
        private BasicElementInfo[] ConvertToElementInfoArray(List<AutomationElement> elements, SearchElementsRequest request)
        {
            return elements.Select(e => _elementFinderService.GetElementBasicInfo(e)).ToArray();
        }

        /// <summary>
        /// Build search criteria string for metadata
        /// </summary>
        private string BuildSearchCriteria(SearchElementsRequest request)
        {
            var criteria = new List<string>();
            if (!string.IsNullOrEmpty(request.SearchText)) criteria.Add($"SearchText: {request.SearchText}");
            if (!string.IsNullOrEmpty(request.AutomationId)) criteria.Add($"AutomationId: {request.AutomationId}");
            if (!string.IsNullOrEmpty(request.Name)) criteria.Add($"Name: {request.Name}");
            if (!string.IsNullOrEmpty(request.ControlType)) criteria.Add($"ControlType: {request.ControlType}");
            return string.Join(", ", criteria);
        }

        /// <summary>
        /// Generate suggested refinements based on search results
        /// </summary>
        private string[] GenerateSuggestedRefinements(SearchElementsRequest request, int totalFound)
        {
            var suggestions = new List<string>();
            if (totalFound > 100)
            {
                suggestions.Add("Add more specific search criteria to narrow results");
                if (string.IsNullOrEmpty(request.ControlType))
                    suggestions.Add("Specify a ControlType to filter by element type");
            }
            return suggestions.ToArray();
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
                CacheRequest = CreateCacheRequest()?.ToString() ?? string.Empty
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

    }
}