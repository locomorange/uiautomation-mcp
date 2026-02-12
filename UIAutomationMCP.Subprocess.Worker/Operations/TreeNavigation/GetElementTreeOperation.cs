using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Core.Options;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Worker.Extensions;
using UIAutomationMCP.Subprocess.Worker.Helpers;

namespace UIAutomationMCP.Subprocess.Worker.Operations.TreeNavigation
{
    public class GetElementTreeOperation : BaseUIAutomationOperation<GetElementTreeRequest, ElementTreeResult>
    {
        private readonly IOptions<UIAutomationOptions> _options;

        public GetElementTreeOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetElementTreeOperation> logger,
            IOptions<UIAutomationOptions> options)
            : base(elementFinderService, logger)
        {
            _options = options;
        }

        /// <summary>
        /// Tree building can be slow for deep hierarchies, allow more time but still prevent indefinite hangs
        /// </summary>
        protected override int OperationTimeoutSeconds => _options.Value.Performance.TreeTraversalTimeoutSeconds;

        protected override async Task<ElementTreeResult> ExecuteOperationAsync(GetElementTreeRequest request)
        {
            var startTime = DateTime.UtcNow;
            var maxElementCount = _options.Value.Performance.MaxElementCount;

            // Force UIAutomation cache refresh for real-time UI tree state
            if (request.BypassCache)
            {
                try
                {
                    // Access root element to trigger cache refresh
                    var refreshRoot = AutomationElement.RootElement;
                    var refreshCheck = refreshRoot?.Current.Name; // Access property to trigger refresh
                }
                catch (Exception)
                {
                    // Continue if cache refresh fails
                }
            }

            var windowTitle = request.WindowTitle ?? "";
            var windowHandle = request.WindowHandle;
            var maxDepth = request.MaxDepth;

            // Get search root
            var searchCriteria = new ElementSearchCriteria
            {
                WindowHandle = windowHandle,
                WindowTitle = windowTitle
            };
            var searchRoot = _elementFinderService.FindElement(searchCriteria) ?? AutomationElement.RootElement;
            if (searchRoot == null)
            {
                // If no specific window found, use desktop root
                searchRoot = AutomationElement.RootElement;
            }

            // Build element tree with cache optimization
            var elementCount = 0;
            var rootNode = await Task.Run(() =>
            {
                if (_options.Value.Performance.EnableCacheOptimization)
                {
                    var cacheRequest = CacheRequestHelper.CreateTreeTraversalCache();
                    _logger.LogDebug("Building element tree with cache optimization enabled. Max depth: {MaxDepth}, Max elements: {MaxElements}",
                        maxDepth, maxElementCount);
                    return BuildElementTreeWithCache(searchRoot, maxDepth, 0, ref elementCount, cacheRequest, maxElementCount);
                }
                else
                {
                    _logger.LogDebug("Building element tree without cache optimization. Max depth: {MaxDepth}, Max elements: {MaxElements}",
                        maxDepth, maxElementCount);
                    return BuildElementTree(searchRoot, maxDepth, 0, ref elementCount, maxElementCount);
                }
            });

            // Log warning if element count limit was reached
            if (elementCount >= maxElementCount)
            {
                _logger.LogWarning(
                    "Element tree traversal reached maximum element count ({MaxCount}). " +
                    "Results may be incomplete. Consider reducing MaxDepth or increasing MaxElementCount in configuration.",
                    maxElementCount);
            }

            // Calculate build duration
            var buildDuration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Element tree built successfully. Total elements: {ElementCount}, Max depth: {Depth}, Duration: {Duration}ms",
                elementCount, maxDepth, buildDuration.TotalMilliseconds);

            return new ElementTreeResult
            {
                RootNode = rootNode,
                TotalElements = CountElements(rootNode),
                MaxDepth = GetMaxDepth(rootNode),
                WindowTitle = windowTitle,
                WindowHandle = windowHandle,
                BuildDuration = buildDuration,
                TreeScope = "Subtree"
            };
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(GetElementTreeRequest request)
        {
            if (request.MaxDepth < 0)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("MaxDepth must be non-negative");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        private TreeNode BuildElementTreeWithCache(AutomationElement element, int maxDepth, int currentDepth, ref int elementCount, CacheRequest cacheRequest, int maxElementCount)
        {
            OperationCancellationToken.ThrowIfCancellationRequested();
            elementCount++;

            // Use cache request to get element with pre-cached properties
            AutomationElement cachedElement;
            try
            {
                cachedElement = CacheRequestHelper.UpdateElementCache(element, cacheRequest);
            }
            catch (Exception)
            {
                // If caching fails, fall back to original element
                cachedElement = element;
            }

            // Use ElementInfoBuilder to create base ElementInfo with all latest features
            // ElementInfoBuilder will use Cached properties when available, reducing COM calls
            var elementInfo = UIAutomationMCP.Subprocess.Core.Helpers.ElementInfoBuilder.CreateElementInfo(cachedElement, includeDetails: false);

            // Create TreeNode from ElementInfo using constructor
            var node = new TreeNode(elementInfo)
            {
                // TreeNode specific properties  
                Depth = currentDepth,
                HasChildren = false
            };

            // Build children if within depth limit
            if (currentDepth < maxDepth)
            {
                try
                {
                    // Use cache request to get children with pre-cached properties
                    // This is a major optimization: one COM call instead of N+1 calls (N children + 1 for collection)
                    AutomationElementCollection children;
                    using (cacheRequest.Activate())
                    {
                        children = cachedElement.FindAll(TreeScope.Children, Condition.TrueCondition);
                    }

                    node.HasChildren = children.Count > 0;

                    foreach (AutomationElement child in children)
                    {
                        if (child != null && elementCount < maxElementCount)
                        {
                            try
                            {
                                var childNode = BuildElementTreeWithCache(child, maxDepth, currentDepth + 1, ref elementCount, cacheRequest, maxElementCount);
                                childNode.ParentAutomationId = node.AutomationId;
                                node.Children.Add(childNode);
                            }
                            catch (ElementNotAvailableException)
                            {
                                // Skip unavailable elements
                                continue;
                            }
                        }
                        else if (elementCount >= maxElementCount)
                        {
                            // Stop processing if we've hit the limit
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log children enumeration errors but continue
                    _logger.LogDebug(ex, "Error enumerating children for element {AutomationId}", node.AutomationId);
                }
            }

            return node;
        }

        private TreeNode BuildElementTree(AutomationElement element, int maxDepth, int currentDepth, ref int elementCount, int maxElementCount)
        {
            OperationCancellationToken.ThrowIfCancellationRequested();
            elementCount++;

            // Use ElementInfoBuilder to create base ElementInfo with all latest features
            var elementInfo = UIAutomationMCP.Subprocess.Core.Helpers.ElementInfoBuilder.CreateElementInfo(element, includeDetails: false);

            // Create TreeNode from ElementInfo using constructor
            var node = new TreeNode(elementInfo)
            {
                // TreeNode specific properties  
                Depth = currentDepth,
                HasChildren = false
            };

            // Build children if within depth limit
            if (currentDepth < maxDepth)
            {
                try
                {
                    var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                    node.HasChildren = children.Count > 0;

                    foreach (AutomationElement child in children)
                    {
                        if (child != null && elementCount < maxElementCount)
                        {
                            try
                            {
                                var childNode = BuildElementTree(child, maxDepth, currentDepth + 1, ref elementCount, maxElementCount);
                                childNode.ParentAutomationId = node.AutomationId;
                                node.Children.Add(childNode);
                            }
                            catch (ElementNotAvailableException)
                            {
                                // Skip unavailable elements
                                continue;
                            }
                        }
                        else if (elementCount >= maxElementCount)
                        {
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore children enumeration errors
                }
            }

            return node;
        }

        private int CountElements(TreeNode node)
        {
            int count = 1; // Count this node
            foreach (var child in node.Children)
            {
                count += CountElements(child);
            }
            return count;
        }

        private int GetMaxDepth(TreeNode node)
        {
            if (node.Children.Count == 0)
                return node.Depth;

            int maxChildDepth = node.Depth;
            foreach (var child in node.Children)
            {
                int childDepth = GetMaxDepth(child);
                if (childDepth > maxChildDepth)
                    maxChildDepth = childDepth;
            }
            return maxChildDepth;
        }
    }
}
