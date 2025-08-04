using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Worker.Extensions;

namespace UIAutomationMCP.Subprocess.Worker.Operations.TreeNavigation
{
    public class GetElementTreeOperation : BaseUIAutomationOperation<GetElementTreeRequest, ElementTreeResult>
    {
        public GetElementTreeOperation(ElementFinderService elementFinderService, ILogger<GetElementTreeOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<ElementTreeResult> ExecuteOperationAsync(GetElementTreeRequest request)
        {
            var startTime = DateTime.UtcNow;

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

            // Build element tree
            var rootNode = await Task.Run(() => BuildElementTree(searchRoot, maxDepth, 0));

            // Calculate build duration
            var buildDuration = DateTime.UtcNow - startTime;

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

        private TreeNode BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
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
                        if (child != null)
                        {
                            try
                            {
                                var childNode = BuildElementTree(child, maxDepth, currentDepth + 1);
                                childNode.ParentAutomationId = node.AutomationId;
                                node.Children.Add(childNode);
                            }
                            catch (ElementNotAvailableException)
                            {
                                // Skip unavailable elements
                                continue;
                            }
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

