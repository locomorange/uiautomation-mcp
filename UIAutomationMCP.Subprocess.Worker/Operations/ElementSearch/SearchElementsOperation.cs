using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Core.Options;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Worker.Extensions;
using UIAutomationMCP.Subprocess.Worker.Helpers;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Subprocess.Worker.Operations.ElementSearch
{
    public class SearchElementsOperation : BaseUIAutomationOperation<SearchElementsRequest, SearchElementsResult>
    {
        private readonly IOptions<UIAutomationOptions> _options;

        public SearchElementsOperation(
            ElementFinderService elementFinderService,
            ILogger<SearchElementsOperation> logger,
            IOptions<UIAutomationOptions> options)
            : base(elementFinderService, logger)
        {
            _options = options;
        }
        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(SearchElementsRequest request)
        {
            if (request.MaxResults <= 0)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("MaxResults must be greater than 0");
            }

            // Allow empty search criteria as it's valid for global search
            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        protected override async Task<SearchElementsResult> ExecuteOperationAsync(SearchElementsRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Timeout handling is performed by SubprocessExecutor, so it is not handled directly here.
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
                _logger?.LogDebug("Starting PerformSearchAsync");

                // UI Automation availability check - basic check only
                try
                {
                    _logger?.LogDebug("Checking UI Automation availability");
                    var rootElement = AutomationElement.RootElement;
                    if (rootElement == null)
                    {
                        throw new InvalidOperationException("UI Automation root element is not available");
                    }
                    _logger?.LogDebug("UI Automation root element available");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "UI Automation availability check failed");
                    throw new InvalidOperationException($"UI Automation is not available: {ex.Message}");
                }

                // Cache bypass logic for real-time window detection
                if (request.BypassCache)
                {
                    _logger?.LogDebug("BypassCache enabled - forcing UIAutomation cache refresh");
                    try
                    {
                        // Force UIAutomation to refresh its cache by re-accessing root element
                        // This helps ensure we get real-time window state for window detection
                        var refreshRoot = AutomationElement.RootElement;
                        var refreshCheck = refreshRoot?.Current.Name; // Access property to trigger cache refresh
                        _logger?.LogDebug("UIAutomation cache refresh completed");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "UIAutomation cache refresh failed, continuing with normal search");
                    }
                }

                // Perform search using ElementFinderService with new criteria-based API
                _logger?.LogDebug("Starting FindElements with ControlType={ControlType}, WindowHandle={WindowHandle}", request.ControlType, request.WindowHandle);
                
                var searchCriteria = new ElementSearchCriteria
                {
                    AutomationId = request.AutomationId,
                    Name = request.Name,
                    ControlType = request.ControlType,
                    WindowTitle = request.WindowTitle,
                    Scope = request.Scope,
                    WindowHandle = request.WindowHandle,
                    // Use the explicit parameter from request, with fallback logic for backward compatibility
                    UseWindowHandleAsFilter = request.UseWindowHandleAsFilter ||
                        (request.WindowHandle.HasValue && !string.IsNullOrEmpty(request.WindowTitle))
                };
                
                bool useCacheOptimization = _options.Value.Performance.EnableCacheOptimization;
                var foundElementsList = FindElementsWithOptimalMethod(searchCriteria, useCacheOptimization);

                // Cross-property searchText / fuzzyMatch filtering.
                // UIA PropertyCondition only supports exact matches, so substring/fuzzy filtering
                // is applied here against the found elements. This runs before sorting and the
                // MaxResults/totalFound calculation so the metadata counts reflect the filtered set.
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    var beforeFilter = foundElementsList.Count;
                    foundElementsList = FilterBySearchText(
                        foundElementsList, request.SearchText!, request.FuzzyMatch, useCacheOptimization);
                    _logger?.LogDebug(
                        "SearchText filter '{SearchText}' (fuzzy={Fuzzy}) matched {Matched} of {Total} elements",
                        request.SearchText, request.FuzzyMatch, foundElementsList.Count, beforeFilter);
                }

                // Apply basic filtering and sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    foundElementsList = SortElementsBasic(foundElementsList, request.SortBy, useCacheOptimization);
                }

                // Apply result limits
                var totalFound = foundElementsList.Count;
                if (foundElementsList.Count > request.MaxResults)
                {
                    foundElementsList = foundElementsList.Take(request.MaxResults).ToList();
                }

                // Convert to ElementInfo array with includeDetails support
                // When cache optimization is enabled, use .Cached properties for bulk reads
                var elements = foundElementsList.Select(e => 
                    UIAutomationMCP.Subprocess.Core.Helpers.ElementInfoBuilder.CreateElementInfo(
                        e, request.IncludeDetails, _logger, useCached: useCacheOptimization)).ToArray();

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
        /// Selects the optimal search strategy based on the search root's characteristics.
        /// For list containers (List, DataGrid, Tree, ComboBox) with Children scope,
        /// uses ListSearchOptimizer to pick TreeWalker (Win32) or FindAll (WPF).
        /// Otherwise falls back to the standard ElementFinderService.FindElements path.
        /// </summary>
        private List<AutomationElement> FindElementsWithOptimalMethod(
            ElementSearchCriteria searchCriteria, bool useCacheOptimization)
        {
            // Check if we can use ListSearchOptimizer for this search.
            // Conditions: the search root is a list container AND scope is "children".
            var scope = searchCriteria.Scope?.ToLower();
            if (scope is "children" or null) // default scope resolves to Children
            {
                try
                {
                    var searchRoot = _elementFinderService.GetSearchRoot(searchCriteria);
                    if (searchRoot != null && ListSearchOptimizer.IsListContainer(searchRoot))
                    {
                        _logger?.LogDebug(
                            "Search root is a list container (FrameworkId={FrameworkId}), using ListSearchOptimizer",
                            searchRoot.Current.FrameworkId);

                        // Build the same condition that ElementFinderService would build
                        var condition = BuildSearchConditionForOptimizer(searchCriteria);
                        if (condition != null)
                        {
                            // Run outside CacheRequest scope — TreeWalker has its own COM pattern
                            var optimizedResults = ListSearchOptimizer.FindAllChildrenOptimized(searchRoot, condition);
                            _logger?.LogDebug("ListSearchOptimizer returned {Count} elements", optimizedResults.Count);
                            return optimizedResults.ToList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "ListSearchOptimizer check failed, falling back to standard search");
                }
            }

            // Standard path: FindAll via ElementFinderService (with optional CacheRequest)
            return FindElementsStandard(searchCriteria, useCacheOptimization);
        }

        /// <summary>
        /// Standard FindAll path with optional CacheRequest optimization
        /// </summary>
        private List<AutomationElement> FindElementsStandard(
            ElementSearchCriteria searchCriteria, bool useCacheOptimization)
        {
            AutomationElementCollection foundElementsCollection;

            if (useCacheOptimization)
            {
                var cacheRequest = CacheRequestHelper.CreateElementSearchCache();
                using (cacheRequest.Activate())
                {
                    foundElementsCollection = _elementFinderService.FindElements(searchCriteria);
                    _logger?.LogDebug("FindElements completed with cache optimization, found {Count} elements",
                        foundElementsCollection?.Count ?? 0);
                }
            }
            else
            {
                foundElementsCollection = _elementFinderService.FindElements(searchCriteria);
                _logger?.LogDebug("FindElements completed, found {Count} elements",
                    foundElementsCollection?.Count ?? 0);
            }

            var result = new List<AutomationElement>();
            if (foundElementsCollection != null)
            {
                foreach (AutomationElement element in foundElementsCollection)
                {
                    if (element != null)
                        result.Add(element);
                }
            }
            return result;
        }

        /// <summary>
        /// Builds a Condition matching the search criteria, for use with ListSearchOptimizer.
        /// Mirrors the condition-building logic in ElementFinderService.BuildSearchCondition.
        /// </summary>
        private Condition? BuildSearchConditionForOptimizer(ElementSearchCriteria criteria)
        {
            var conditions = new List<Condition>();

            if (!string.IsNullOrEmpty(criteria.AutomationId))
                conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, criteria.AutomationId));

            if (!string.IsNullOrEmpty(criteria.Name))
                conditions.Add(new PropertyCondition(AutomationElement.NameProperty, criteria.Name));

            if (!string.IsNullOrEmpty(criteria.ClassName))
                conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, criteria.ClassName));

            if (!string.IsNullOrEmpty(criteria.ControlType))
            {
                if (UIAutomationMCP.Subprocess.Core.Helpers.ControlTypeHelper.TryGetControlType(
                    criteria.ControlType, out var controlType))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));
                }
            }

            if (criteria.VisibleOnly)
                conditions.Add(new PropertyCondition(AutomationElement.IsOffscreenProperty, false));

            if (criteria.EnabledOnly)
                conditions.Add(new PropertyCondition(AutomationElement.IsEnabledProperty, true));

            if (conditions.Count == 0)
                return Condition.TrueCondition;

            return conditions.Count == 1
                ? conditions[0]
                : new AndCondition(conditions.ToArray());
        }


        /// <summary>
        /// Sort elements using basic criteria
        /// </summary>
        private List<AutomationElement> SortElementsBasic(List<AutomationElement> elements, string sortBy, bool useCached = false)
        {
            return sortBy?.ToLower() switch
            {
                "name" => elements.OrderBy(e => GetElementName(e, useCached)).ToList(),
                "controltype" => elements.OrderBy(e => GetElementControlType(e, useCached)).ToList(),
                "position" => elements.OrderBy(e => GetElementX(e, useCached))
                                   .ThenBy(e => GetElementY(e, useCached)).ToList(),
                _ => elements
            };
        }

        private string GetElementName(AutomationElement element, bool useCached = false)
        {
            try
            {
                var props = useCached ? element.Cached : element.Current;
                return props.Name ?? "";
            }
            catch { return ""; }
        }

        private string GetElementControlType(AutomationElement element, bool useCached = false)
        {
            try
            {
                var props = useCached ? element.Cached : element.Current;
                return props.ControlType.ProgrammaticName ?? "";
            }
            catch { return ""; }
        }

        private double GetElementX(AutomationElement element, bool useCached = false)
        {
            try
            {
                var props = useCached ? element.Cached : element.Current;
                return props.BoundingRectangle.X;
            }
            catch { return 0; }
        }

        private double GetElementY(AutomationElement element, bool useCached = false)
        {
            try
            {
                var props = useCached ? element.Cached : element.Current;
                return props.BoundingRectangle.Y;
            }
            catch { return 0; }
        }

        /// <summary>
        /// Filters elements by the cross-property searchText: keeps an element when its Name,
        /// AutomationId, or ClassName matches the search text (case-insensitive substring, or
        /// normalized fuzzy match when <paramref name="fuzzyMatch"/> is true). UIA cannot express
        /// substring conditions, so this is applied in-process to the already-found elements.
        /// </summary>
        private List<AutomationElement> FilterBySearchText(
            List<AutomationElement> elements, string searchText, bool fuzzyMatch, bool useCached)
        {
            var result = new List<AutomationElement>(elements.Count);
            foreach (var element in elements)
            {
                var name = GetSearchableProperty(element, AutomationElement.NameProperty, useCached);
                var automationId = GetSearchableProperty(element, AutomationElement.AutomationIdProperty, useCached);
                var className = GetSearchableProperty(element, AutomationElement.ClassNameProperty, useCached);

                if (SearchTextMatcher.IsMatchAny(searchText, fuzzyMatch, name, automationId, className))
                {
                    result.Add(element);
                }
            }
            return result;
        }

        /// <summary>
        /// Reads a string property for searchText matching. Prefers the cached value when cache
        /// optimization is active, falling back to the current value when the property was not
        /// cached for this element (e.g. elements returned by ListSearchOptimizer, which runs
        /// outside the CacheRequest scope).
        /// </summary>
        private static string GetSearchableProperty(AutomationElement element, AutomationProperty property, bool useCached)
        {
            if (useCached)
            {
                try
                {
                    if (element.GetCachedPropertyValue(property) is string cached)
                        return cached;
                }
                catch
                {
                    // Property not cached for this element; fall through to the current value.
                }
            }

            try
            {
                return element.GetCurrentPropertyValue(property) as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
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
            if (!string.IsNullOrEmpty(request.WindowTitle)) criteria.Add($"WindowTitle: {request.WindowTitle}");
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


    }
}

