using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Worker.Extensions;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
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
            
            var windowTitle = request.WindowTitle ?? "";
            var processId = request.ProcessId ?? 0;
            var maxDepth = request.MaxDepth;

            // Get search root
            var searchRoot = _elementFinderService.GetSearchRoot(processId, windowTitle);
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
                ProcessId = processId,
                BuildDuration = buildDuration,
                TreeScope = "Subtree"
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(GetElementTreeRequest request)
        {
            if (request.MaxDepth < 0)
            {
                return Core.Validation.ValidationResult.Failure("MaxDepth must be non-negative");
            }

            return Core.Validation.ValidationResult.Success;
        }

        private TreeNode BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            // Use ElementInfoBuilder to create base ElementInfo with all latest features
            var elementInfo = UIAutomationMCP.Common.Helpers.ElementInfoBuilder.CreateElementInfo(element, includeDetails: false);
            
            // Create TreeNode from ElementInfo using constructor
            var node = new TreeNode(elementInfo)
            {
                // TreeNode specific properties  
                RuntimeId = element.GetRuntimeId()?.ToString() ?? "",
                IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                HasKeyboardFocus = element.Current.HasKeyboardFocus,
                IsPassword = element.Current.IsPassword,
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
                                childNode.ParentName = node.Name;
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