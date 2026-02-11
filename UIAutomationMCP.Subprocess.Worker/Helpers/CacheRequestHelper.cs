using System.Windows.Automation;

namespace UIAutomationMCP.Subprocess.Worker.Helpers
{
    /// <summary>
    /// Helper class for creating optimized CacheRequest configurations to reduce cross-process COM calls
    /// </summary>
    public static class CacheRequestHelper
    {
        /// <summary>
        /// Creates a CacheRequest optimized for tree traversal operations.
        /// Caches commonly accessed properties to minimize COM calls during recursive tree walking.
        /// </summary>
        /// <returns>CacheRequest configured for efficient tree traversal</returns>
        public static CacheRequest CreateTreeTraversalCache()
        {
            var cacheRequest = new CacheRequest();

            // Set scope to cache element and its children in a single COM call
            cacheRequest.TreeScope = TreeScope.Element | TreeScope.Children;

            // Cache basic identification properties
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.ClassNameProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.LocalizedControlTypeProperty);

            // Cache state properties for visibility and interaction checks
            cacheRequest.Add(AutomationElement.IsEnabledProperty);
            cacheRequest.Add(AutomationElement.IsOffscreenProperty);
            cacheRequest.Add(AutomationElement.IsKeyboardFocusableProperty);
            cacheRequest.Add(AutomationElement.HasKeyboardFocusProperty);
            cacheRequest.Add(AutomationElement.IsControlElementProperty);
            cacheRequest.Add(AutomationElement.IsContentElementProperty);

            // Cache layout properties
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);

            // Cache process information
            cacheRequest.Add(AutomationElement.ProcessIdProperty);
            cacheRequest.Add(AutomationElement.NativeWindowHandleProperty);

            // Cache framework information
            cacheRequest.Add(AutomationElement.FrameworkIdProperty);

            // Cache additional useful properties
            cacheRequest.Add(AutomationElement.HelpTextProperty);
            cacheRequest.Add(AutomationElement.AcceleratorKeyProperty);
            cacheRequest.Add(AutomationElement.AccessKeyProperty);

            // Use TreeFilter to include all elements (raw view)
            // This ensures we get the complete hierarchy
            cacheRequest.TreeFilter = Automation.RawViewCondition;

            // AutomationElementMode.Full ensures we get all properties
            cacheRequest.AutomationElementMode = AutomationElementMode.Full;

            return cacheRequest;
        }

        /// <summary>
        /// Creates a CacheRequest optimized for element search operations.
        /// Focuses on properties needed for matching and filtering elements.
        /// </summary>
        /// <returns>CacheRequest configured for efficient element search</returns>
        public static CacheRequest CreateElementSearchCache()
        {
            var cacheRequest = new CacheRequest();

            // Search typically looks at descendants, not just direct children
            cacheRequest.TreeScope = TreeScope.Element | TreeScope.Descendants;

            // Cache properties commonly used in search criteria
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.ClassNameProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.LocalizedControlTypeProperty);

            // Cache state for filtering
            cacheRequest.Add(AutomationElement.IsEnabledProperty);
            cacheRequest.Add(AutomationElement.IsOffscreenProperty);
            cacheRequest.Add(AutomationElement.IsControlElementProperty);

            // Cache layout for bounding box checks
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);

            // Cache window handle for window-based searches
            cacheRequest.Add(AutomationElement.NativeWindowHandleProperty);

            // Use control view (common for searches)
            cacheRequest.TreeFilter = Automation.ControlViewCondition;

            cacheRequest.AutomationElementMode = AutomationElementMode.Full;

            return cacheRequest;
        }

        /// <summary>
        /// Creates a minimal CacheRequest for lightweight property access.
        /// Use when you only need basic element information.
        /// </summary>
        /// <returns>CacheRequest configured for minimal property access</returns>
        public static CacheRequest CreateMinimalCache()
        {
            var cacheRequest = new CacheRequest();

            // Only cache the element itself
            cacheRequest.TreeScope = TreeScope.Element;

            // Minimal properties
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.BoundingRectangleProperty);

            cacheRequest.TreeFilter = Automation.ControlViewCondition;
            cacheRequest.AutomationElementMode = AutomationElementMode.Full;

            return cacheRequest;
        }

        /// <summary>
        /// Gets a cached child elements collection using the provided cache request.
        /// This method performs a single COM call to retrieve both the children and their cached properties.
        /// </summary>
        /// <param name="element">Parent element</param>
        /// <param name="cacheRequest">Cache request to use (or null for default tree traversal cache)</param>
        /// <returns>Collection of cached child elements</returns>
        public static AutomationElementCollection GetCachedChildren(AutomationElement element, CacheRequest? cacheRequest = null)
        {
            cacheRequest ??= CreateTreeTraversalCache();

            // Ensure the cache request includes children scope
            if ((cacheRequest.TreeScope & TreeScope.Children) == 0)
            {
                // If not set, we need to adjust the scope
                var tempRequest = CreateTreeTraversalCache();
                tempRequest.TreeScope = cacheRequest.TreeScope | TreeScope.Children;
                cacheRequest = tempRequest;
            }

            using (cacheRequest.Activate())
            {
                // FindAll with cache returns elements with pre-cached properties
                return element.FindAll(TreeScope.Children, Condition.TrueCondition);
            }
        }

        /// <summary>
        /// Updates the cache for an existing element with the specified cache request.
        /// Useful when you need to refresh cached properties or cache additional properties.
        /// </summary>
        /// <param name="element">Element to update</param>
        /// <param name="cacheRequest">Cache request to use</param>
        /// <returns>Element with updated cache</returns>
        public static AutomationElement UpdateElementCache(AutomationElement element, CacheRequest cacheRequest)
        {
            using (cacheRequest.Activate())
            {
                return element.GetUpdatedCache(cacheRequest);
            }
        }
    }
}
