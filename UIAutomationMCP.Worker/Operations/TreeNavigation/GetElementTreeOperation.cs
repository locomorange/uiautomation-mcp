using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetElementTreeOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetElementTreeOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public async Task<OperationResult<ElementTreeResult>> ExecuteAsync(string parametersJson)
        {
            var result = await ExecuteInternalAsync(parametersJson);
            return result;
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

        private async Task<OperationResult<ElementTreeResult>> ExecuteInternalAsync(string parametersJson)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                // Deserialize request
                var typedRequest = JsonSerializationHelper.Deserialize<GetElementTreeRequest>(parametersJson);
                
                if (typedRequest == null)
                {
                    return new OperationResult<ElementTreeResult>
                    {
                        Success = false,
                        Error = "Invalid request format. Expected GetElementTreeRequest.",
                        Data = new ElementTreeResult()
                    };
                }

                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;
                var maxDepth = typedRequest.MaxDepth;

                // Get search root
                var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (searchRoot == null)
                {
                    // If no specific window found, use desktop root
                    searchRoot = AutomationElement.RootElement;
                }

                // Build element tree
                var rootNode = await Task.Run(() => BuildElementTree(searchRoot, maxDepth, 0));
                
                // Calculate build duration
                var buildDuration = DateTime.UtcNow - startTime;
                
                var result = new ElementTreeResult
                {
                    RootNode = rootNode,
                    TotalElements = CountElements(rootNode),
                    MaxDepth = GetMaxDepth(rootNode),
                    WindowTitle = windowTitle,
                    ProcessId = processId,
                    BuildDuration = buildDuration,
                    TreeScope = "Subtree"
                };

                return new OperationResult<ElementTreeResult>
                {
                    Success = true,
                    Data = result,
                    ExecutionSeconds = buildDuration.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                return new OperationResult<ElementTreeResult>
                {
                    Success = false,
                    Error = $"Error building element tree: {ex.Message}",
                    Data = new ElementTreeResult(),
                    ExecutionSeconds = (DateTime.UtcNow - startTime).TotalSeconds
                };
            }
        }

        private TreeNode BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            // Use ElementInfoBuilder to create base ElementInfo with all latest features
            var elementInfo = ElementInfoBuilder.CreateElementInfo(element, includeDetails: false);
            
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