using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class TreeViewOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public TreeViewOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var operation = request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "getinfo";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (element.Current.ControlType != System.Windows.Automation.ControlType.Tree)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a tree view" });

            try
            {
                switch (operation.ToLower())
                {
                    case "getinfo":
                        var treeInfo = new Dictionary<string, object>
                        {
                            ["Name"] = element.Current.Name,
                            ["AutomationId"] = element.Current.AutomationId,
                            ["IsEnabled"] = element.Current.IsEnabled,
                            ["IsVisible"] = !element.Current.IsOffscreen,
                            ["SupportedPatterns"] = element.GetSupportedPatterns().Select(p => p.ProgrammaticName).ToList()
                        };

                        // Check for selection capabilities
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionPattern selection)
                        {
                            treeInfo["CanSelectMultiple"] = selection.Current.CanSelectMultiple;
                            treeInfo["IsSelectionRequired"] = selection.Current.IsSelectionRequired;
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = treeInfo });

                    case "getnodes":
                        var treeItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.TreeItem));
                        var nodes = new List<Dictionary<string, object>>();
                        
                        foreach (AutomationElement item in treeItems)
                        {
                            var nodeInfo = new Dictionary<string, object>
                            {
                                ["Name"] = item.Current.Name,
                                ["AutomationId"] = item.Current.AutomationId,
                                ["IsEnabled"] = item.Current.IsEnabled,
                                ["IsSelected"] = false,
                                ["IsExpanded"] = false,
                                ["HasChildren"] = false
                            };

                            // Check if node is selected
                            if (item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && itemPattern is SelectionItemPattern selItem)
                            {
                                nodeInfo["IsSelected"] = selItem.Current.IsSelected;
                            }

                            // Check if node is expanded and has children
                            if (item.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern expand)
                            {
                                nodeInfo["IsExpanded"] = expand.Current.ExpandCollapseState == ExpandCollapseState.Expanded;
                                nodeInfo["HasChildren"] = expand.Current.ExpandCollapseState != ExpandCollapseState.LeafNode;
                            }

                            nodes.Add(nodeInfo);
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = nodes });

                    case "selectnode":
                        var selectNode = request.Parameters?.GetValueOrDefault("node")?.ToString() ?? "";
                        var nodeCondition = new PropertyCondition(AutomationElement.NameProperty, selectNode);
                        var targetNode = element.FindFirst(TreeScope.Descendants, nodeCondition);
                        
                        if (targetNode != null && targetNode.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectNodePattern) && selectNodePattern is SelectionItemPattern selectNode2)
                        {
                            selectNode2.Select();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Node not found or does not support selection" });

                    case "expandnode":
                        var expandNode = request.Parameters?.GetValueOrDefault("node")?.ToString() ?? "";
                        var expandNodeCondition = new PropertyCondition(AutomationElement.NameProperty, expandNode);
                        var targetExpandNode = element.FindFirst(TreeScope.Descendants, expandNodeCondition);
                        
                        if (targetExpandNode != null && targetExpandNode.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandNodePattern) && expandNodePattern is ExpandCollapsePattern expandNode2)
                        {
                            expandNode2.Expand();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Node not found or does not support expand" });

                    case "collapsenode":
                        var collapseNode = request.Parameters?.GetValueOrDefault("node")?.ToString() ?? "";
                        var collapseNodeCondition = new PropertyCondition(AutomationElement.NameProperty, collapseNode);
                        var targetCollapseNode = element.FindFirst(TreeScope.Descendants, collapseNodeCondition);
                        
                        if (targetCollapseNode != null && targetCollapseNode.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var collapseNodePattern) && collapseNodePattern is ExpandCollapsePattern collapseNode2)
                        {
                            collapseNode2.Collapse();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Node not found or does not support collapse" });

                    case "getselection":
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var getSelectionPattern) && getSelectionPattern is SelectionPattern getSelection)
                        {
                            var selectedNodes = getSelection.Current.GetSelection();
                            var selectedNodesInfo = selectedNodes.Select(node => new Dictionary<string, object>
                            {
                                ["Name"] = node.Current.Name,
                                ["AutomationId"] = node.Current.AutomationId
                            }).ToList();

                            return Task.FromResult(new OperationResult { Success = true, Data = selectedNodesInfo });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Tree view does not support selection" });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing tree view operation: {ex.Message}" });
            }
        }
    }
}