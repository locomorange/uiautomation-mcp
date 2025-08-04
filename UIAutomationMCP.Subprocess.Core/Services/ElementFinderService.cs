using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Subprocess.Core.Helpers;

namespace UIAutomationMCP.Subprocess.Core.Services
{
    /// <summary>
    /// Unified element finding service for UI Automation operations
    /// Provides consistent, performant element search functionality for all MCP tools
    /// </summary>
    public class ElementFinderService
    {
        private readonly ILogger<ElementFinderService> _logger;

        public ElementFinderService(ILogger<ElementFinderService> logger)
        {
            _logger = logger;
        }

        #region Core Search Methods

        /// <summary>
        /// Find a single element using search criteria
        /// </summary>
        /// <param name="searchCriteria">Search parameters</param>
        /// <returns>Found element or null if not found</returns>
        public AutomationElement? FindElement(ElementSearchCriteria searchCriteria)
        {
            try
            {
                var rootElement = GetSearchRoot(searchCriteria);
                if (rootElement == null)
                {
                    _logger.LogDebug("Search root not found");
                    return null;
                }

                var condition = BuildSearchCondition(searchCriteria);
                if (condition == null)
                {
                    _logger.LogWarning("No valid search condition could be built");
                    return null;
                }

                var scope = GetSafeScope(searchCriteria.Scope);
                var element = rootElement.FindFirst(scope, condition);

                if (element != null)
                {
                    _logger.LogDebug("Found element: AutomationId={AutomationId}, Name={Name}, ControlType={ControlType}",
                        GetSafeProperty(element, e => e.Current.AutomationId),
                        GetSafeProperty(element, e => e.Current.Name),
                        GetSafeProperty(element, e => e.Current.ControlType.ProgrammaticName));

                    // Check pattern requirement if specified
                    if (!string.IsNullOrEmpty(searchCriteria.RequiredPattern))
                    {
                        if (!SupportsPattern(element, searchCriteria.RequiredPattern))
                        {
                            _logger.LogDebug("Element found but does not support required pattern: {Pattern}", searchCriteria.RequiredPattern);
                            return null;
                        }
                    }
                }

                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element");
                return null;
            }
        }

        /// <summary>
        /// Find multiple elements using search criteria
        /// </summary>
        /// <param name="searchCriteria">Search parameters</param>
        /// <returns>Collection of found elements (empty if none found)</returns>
        public AutomationElementCollection FindElements(ElementSearchCriteria searchCriteria)
        {
            try
            {
                var rootElement = GetSearchRoot(searchCriteria);
                if (rootElement == null)
                {
                    _logger.LogDebug("Search root not found");
                    return GetEmptyElementCollection();
                }

                var condition = BuildSearchCondition(searchCriteria);
                if (condition == null)
                {
                    _logger.LogWarning("No valid search condition could be built");
                    return GetEmptyElementCollection();
                }

                var scope = GetSafeScope(searchCriteria.Scope);

                _logger.LogDebug("Searching for elements with scope: {Scope}", scope);
                var elements = rootElement.FindAll(scope, condition);
                _logger.LogDebug("Found {Count} elements", elements?.Count ?? 0);

                return elements ?? GetEmptyElementCollection();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding elements");
                return GetEmptyElementCollection();
            }
        }

        /// <summary>
        /// Convert AutomationElement to ElementInfo using ElementInfoBuilder
        /// </summary>
        public ElementInfo GetElementBasicInfo(AutomationElement element)
        {
            try
            {
                // Use ElementInfoBuilder for consistent element information extraction
                return ElementInfoBuilder.CreateElementInfo(element, includeDetails: false, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element basic info");
                return new ElementInfo
                {
                    AutomationId = "Error",
                    Name = "Error retrieving element info",
                    ControlType = "Unknown"
                };
            }
        }

        #endregion

        #region Helper Methods

        private AutomationElement? GetSearchRoot(ElementSearchCriteria criteria)
        {
            // Priority 1: Use WindowHandle if specified and not using it as filter
            if (criteria.WindowHandle.HasValue && !criteria.UseWindowHandleAsFilter)
            {
                try
                {
                    _logger.LogDebug("Creating AutomationElement from WindowHandle: {WindowHandle}", criteria.WindowHandle.Value);
                    var element = AutomationElement.FromHandle(new IntPtr(criteria.WindowHandle.Value));
                    if (element != null)
                    {
                        _logger.LogDebug("Successfully created AutomationElement from HWND: {WindowHandle} -> Name: {ElementName}",
                            criteria.WindowHandle.Value, GetSafeProperty(element, e => e.Current.Name));
                        return element;
                    }
                    else
                    {
                        _logger.LogWarning("AutomationElement.FromHandle returned null for HWND: {WindowHandle}", criteria.WindowHandle.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create AutomationElement from WindowHandle: {WindowHandle}", criteria.WindowHandle.Value);
                }
            }

            AutomationElement? rootElement = AutomationElement.RootElement;
            _logger.LogDebug("Starting with AutomationElement.RootElement: {RootName}", rootElement?.Current.Name ?? "null");

            if (rootElement == null)
            {
                _logger.LogError("AutomationElement.RootElement is null");
                return null;
            }

            // When using WindowHandle as filter, always start from RootElement
            if (criteria.UseWindowHandleAsFilter)
            {
                _logger.LogInformation("*** FILTER MODE *** Using WindowHandle as filter, starting from RootElement");
                Console.Error.WriteLine($"*** ELEMENTFINDER DEBUG *** Filter mode active for WindowHandle={criteria.WindowHandle}");
                return rootElement;
            }

            // Find window by title if specified
            if (!string.IsNullOrEmpty(criteria.WindowTitle))
            {
                _logger.LogDebug("Searching for window by title: {WindowTitle}", criteria.WindowTitle);
                var windowCondition = new PropertyCondition(AutomationElement.NameProperty, criteria.WindowTitle);
                var window = rootElement.FindFirst(TreeScope.Children, windowCondition);
                if (window != null)
                {
                    _logger.LogDebug("Found window with title: {WindowTitle}", criteria.WindowTitle);
                    return window;
                }
                else
                {
                    _logger.LogDebug("Window not found with title: {WindowTitle}", criteria.WindowTitle);
                    return null;
                }
            }


            // Default case: use RootElement for global searches
            _logger.LogDebug("Using RootElement as search root for global search");
            return rootElement;
        }

        private Condition? BuildSearchCondition(ElementSearchCriteria criteria)
        {
            var conditions = new List<Condition>();

            _logger.LogDebug("Building search condition with criteria: AutomationId={AutomationId}, Name={Name}, ControlType={ControlType}, WindowTitle={WindowTitle}, WindowHandle={WindowHandle}",
                criteria.AutomationId, criteria.Name, criteria.ControlType, criteria.WindowTitle, criteria.WindowHandle);

            // Primary identifiers
            if (!string.IsNullOrEmpty(criteria.AutomationId))
            {
                conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, criteria.AutomationId));
                _logger.LogDebug("Added AutomationId condition: {AutomationId}", criteria.AutomationId);
            }

            if (!string.IsNullOrEmpty(criteria.Name))
            {
                conditions.Add(new PropertyCondition(AutomationElement.NameProperty, criteria.Name));
                _logger.LogDebug("Added Name condition: {Name}", criteria.Name);
            }

            if (!string.IsNullOrEmpty(criteria.ClassName))
            {
                conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, criteria.ClassName));
                _logger.LogDebug("Added ClassName condition: {ClassName}", criteria.ClassName);
            }

            // Control type filter
            if (!string.IsNullOrEmpty(criteria.ControlType))
            {
                _logger.LogDebug("Trying to resolve ControlType: {ControlTypeName}", criteria.ControlType);
                if (ControlTypeHelper.TryGetControlType(criteria.ControlType, out var controlType))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlType));
                    _logger.LogDebug("✁EResolved {ControlTypeName} -> {ProgrammaticName}", criteria.ControlType, controlType.ProgrammaticName);
                }
                else
                {
                    _logger.LogWarning("✁EFailed to resolve ControlType: {ControlTypeName}", criteria.ControlType);
                }
            }

            // WindowHandle filter (when used as filter)
            if (criteria.UseWindowHandleAsFilter && criteria.WindowHandle.HasValue)
            {
                conditions.Add(new PropertyCondition(AutomationElement.NativeWindowHandleProperty, (int)criteria.WindowHandle.Value));
                _logger.LogInformation("*** FILTER MODE ACTIVE *** Added WindowHandle filter condition: {WindowHandle}", criteria.WindowHandle.Value);
            }

            // Visibility filter
            if (criteria.VisibleOnly)
            {
                conditions.Add(new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
                _logger.LogDebug("Added VisibleOnly condition");
            }

            // Enabled filter
            if (criteria.EnabledOnly)
            {
                conditions.Add(new PropertyCondition(AutomationElement.IsEnabledProperty, true));
                _logger.LogDebug("Added EnabledOnly condition");
            }

            // Return appropriate condition
            if (conditions.Count == 0)
            {
                // If no specific conditions are specified, return a condition that matches all elements
                // This is useful when searching by ProcessId only to get all child elements
                _logger.LogDebug("No conditions specified, using TrueCondition");
                return Condition.TrueCondition;
            }

            if (conditions.Count == 1)
            {
                _logger.LogDebug("Using single condition");
                return conditions[0];
            }

            _logger.LogDebug("Using AndCondition with {Count} conditions", conditions.Count);
            return new AndCondition(conditions.ToArray());
        }

        private TreeScope GetSafeScope(string? scope)
        {
            // Default to Children for better performance and to avoid hangs
            return scope?.ToLower() switch
            {
                "children" => TreeScope.Children,
                "descendants" => TreeScope.Descendants, // Use with caution
                "subtree" => TreeScope.Subtree,
                _ => TreeScope.Children // Safe default
            };
        }


        private bool SupportsPattern(AutomationElement element, string patternName)
        {
            try
            {
                var supportedPatterns = element.GetSupportedPatterns();
                return supportedPatterns.Any(p => p.ProgrammaticName.Contains(patternName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private T GetSafeProperty<T>(AutomationElement element, Func<AutomationElement, T> getter)
        {
            try
            {
                return getter(element);
            }
            catch
            {
                return default(T)!;
            }
        }

        private AutomationElementCollection GetEmptyElementCollection()
        {
            return AutomationElement.RootElement.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "___NonExistentElement___"));
        }

        #endregion

    }

    /// <summary>
    /// Unified search criteria for element finding operations
    /// </summary>
    public class ElementSearchCriteria
    {
        public string? AutomationId { get; set; }
        public string? Name { get; set; }
        public string? ClassName { get; set; }
        public string? ControlType { get; set; }
        public string? WindowTitle { get; set; }
        public string? Scope { get; set; } = "Descendants"; // Changed default for better coverage
        public bool VisibleOnly { get; set; } = false;
        public bool EnabledOnly { get; set; } = false;
        public string? RequiredPattern { get; set; }
        public long? WindowHandle { get; set; }
        public bool UseWindowHandleAsFilter { get; set; } = false;
    }
}

