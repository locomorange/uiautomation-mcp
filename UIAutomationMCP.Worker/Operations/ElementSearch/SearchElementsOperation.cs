using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Common.Helpers;
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
                // タイムアウト�E琁E�E�ESubprocessExecutorで行うため、直接実衁E
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
                var foundElementsCollection = _elementFinderService.FindElements(searchCriteria);
                _logger?.LogDebug("FindElements completed, found {Count} elements", foundElementsCollection?.Count ?? 0);

                // Convert to list for further processing
                var foundElementsList = new List<AutomationElement>();
                if (foundElementsCollection != null)
                {
                    foreach (AutomationElement element in foundElementsCollection)
                    {
                        if (element != null)
                            foundElementsList.Add(element);
                    }
                }

                // Apply basic filtering and sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    foundElementsList = SortElementsBasic(foundElementsList, request.SortBy);
                }

                // Apply result limits
                var totalFound = foundElementsList.Count;
                if (foundElementsList.Count > request.MaxResults)
                {
                    foundElementsList = foundElementsList.Take(request.MaxResults).ToList();
                }

                // Convert to ElementInfo array
                var elements = foundElementsList.Select(e => _elementFinderService.GetElementBasicInfo(e)).ToArray();

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
        /// Sort elements using basic criteria
        /// </summary>
        private List<AutomationElement> SortElementsBasic(List<AutomationElement> elements, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "name" => elements.OrderBy(e => GetElementName(e)).ToList(),
                "controltype" => elements.OrderBy(e => GetElementControlType(e)).ToList(),
                "position" => elements.OrderBy(e => GetElementX(e))
                                   .ThenBy(e => GetElementY(e)).ToList(),
                _ => elements
            };
        }
        
        private string GetElementName(AutomationElement element)
        {
            try { return element.Current.Name ?? ""; }
            catch { return ""; }
        }
        
        private string GetElementControlType(AutomationElement element)
        {
            try { return element.Current.ControlType.ProgrammaticName ?? ""; }
            catch { return ""; }
        }
        
        private double GetElementX(AutomationElement element)
        {
            try { return element.Current.BoundingRectangle.X; }
            catch { return 0; }
        }
        
        private double GetElementY(AutomationElement element)
        {
            try { return element.Current.BoundingRectangle.Y; }
            catch { return 0; }
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