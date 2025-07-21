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

                var searchRoot = _elementFinderService.GetSearchRoot(request.WindowTitle ?? "", request.ProcessId ?? 0);
                searchRoot ??= AutomationElement.RootElement;

                // Build search conditions based on request parameters
                var conditions = BuildSearchConditions(request);
                var combinedCondition = conditions.Count == 1 ? conditions[0] : new AndCondition(conditions.ToArray());

                // Create cache request for better performance
                var cacheRequest = CreateCacheRequest();
                
                // Perform the search
                AutomationElementCollection foundElements;
                using (cacheRequest.Activate())
                {
                    foundElements = _elementFinderService.FindElements(combinedCondition, request.WindowTitle ?? "", request.ProcessId ?? 0);
                }

                // Convert to ElementInfo array with optional details
                var elements = ConvertToElementInfoArray(foundElements, request);

                // Apply result limits
                if (elements.Length > request.MaxResults)
                {
                    elements = elements.Take(request.MaxResults).ToArray();
                }

                searchStopwatch.Stop();

                return Task.FromResult(new SearchElementsResult
                {
                    Success = true,
                    OperationName = "SearchElements",
                    Elements = elements,
                    Metadata = new SearchMetadata
                    {
                        TotalFound = foundElements.Count,
                        Returned = elements.Length,
                        SearchDuration = searchStopwatch.Elapsed,
                        SearchCriteria = BuildSearchCriteria(request),
                        WasTruncated = foundElements.Count > request.MaxResults,
                        SuggestedRefinements = GenerateSuggestedRefinements(request, foundElements.Count),
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

        private List<Condition> BuildSearchConditions(SearchElementsRequest request)
        {
            var conditions = new List<Condition>();

            // Add conditions based on search parameters
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                // Search in both Name and AutomationId
                var nameCondition = new PropertyCondition(AutomationElement.NameProperty, request.SearchText);
                var automationIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, request.SearchText);
                conditions.Add(new OrCondition(nameCondition, automationIdCondition));
            }

            if (!string.IsNullOrEmpty(request.AutomationId))
            {
                conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, request.AutomationId));
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                conditions.Add(new PropertyCondition(AutomationElement.NameProperty, request.Name));
            }

            if (!string.IsNullOrEmpty(request.ControlType))
            {
                // Try to get ControlType by programmatic name
                if (TryGetControlTypeByName(request.ControlType, out var controlType))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));
                }
            }

            if (request.VisibleOnly)
            {
                conditions.Add(new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
            }

            // If no specific conditions, use TrueCondition
            if (conditions.Count == 0)
            {
                conditions.Add(Condition.TrueCondition);
            }

            return conditions;
        }

        private bool TryGetControlTypeByName(string controlTypeName, out ControlType controlType)
        {
            controlType = ControlType.Custom;
            
            // Map common control type names
            return controlTypeName.ToLowerInvariant() switch
            {
                "button" => (controlType = ControlType.Button) != null,
                "text" => (controlType = ControlType.Text) != null,
                "edit" => (controlType = ControlType.Edit) != null,
                "combobox" => (controlType = ControlType.ComboBox) != null,
                "listbox" => (controlType = ControlType.List) != null,
                "checkbox" => (controlType = ControlType.CheckBox) != null,
                "radiobutton" => (controlType = ControlType.RadioButton) != null,
                "group" => (controlType = ControlType.Group) != null,
                "window" => (controlType = ControlType.Window) != null,
                "menu" => (controlType = ControlType.Menu) != null,
                "menuitem" => (controlType = ControlType.MenuItem) != null,
                "tab" => (controlType = ControlType.Tab) != null,
                "tabitem" => (controlType = ControlType.TabItem) != null,
                "tree" => (controlType = ControlType.Tree) != null,
                "treeitem" => (controlType = ControlType.TreeItem) != null,
                "table" => (controlType = ControlType.Table) != null,
                _ => false
            };
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
            cacheRequest.AutomationElementMode = AutomationElementMode.None;
            cacheRequest.TreeFilter = Automation.RawViewCondition;
            return cacheRequest;
        }

        private ElementInfo[] ConvertToElementInfoArray(AutomationElementCollection elements, SearchElementsRequest request)
        {
            var result = new List<ElementInfo>();

            foreach (AutomationElement element in elements)
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
            }
            else if (totalFound > request.MaxResults)
            {
                suggestions.Add($"Consider increasing MaxResults (current: {request.MaxResults})");
                suggestions.Add("Add more specific search criteria");
            }
            else if (!request.IncludeDetails)
            {
                suggestions.Add("Use IncludeDetails=true for more information");
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
            if (!string.IsNullOrEmpty(request.ControlType))
                criteria.Add($"ControlType='{request.ControlType}'");
            if (request.VisibleOnly)
                criteria.Add("VisibleOnly=true");
            if (request.IncludeDetails)
                criteria.Add("IncludeDetails=true");
            
            return string.Join(", ", criteria);
        }
    }
}