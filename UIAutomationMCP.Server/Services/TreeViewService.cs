using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services
{
    public class TreeViewService : ITreeViewService
    {
        private readonly ILogger<TreeViewService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public TreeViewService(ILogger<TreeViewService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> TreeViewOperationAsync(string elementId, string operation, string? nodePath = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing TreeView operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var treeView = _automationHelper.FindElementById(elementId, searchRoot);

                    if (treeView == null)
                    {
                        return new { error = $"TreeView '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "expand":
                        case "collapse":
                        case "select":
                            if (string.IsNullOrEmpty(nodePath))
                            {
                                return new { error = "nodePath is required for this operation" };
                            }

                            var nodeNames = nodePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                            AutomationElement currentNode = treeView;

                            foreach (var nodeName in nodeNames)
                            {
                                var foundNode = currentNode.FindFirst(TreeScope.Children,
                                    new AndCondition(
                                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem),
                                        new PropertyCondition(AutomationElement.NameProperty, nodeName)
                                    ));

                                if (foundNode == null)
                                {
                                    return new { error = $"Tree node '{nodeName}' not found" };
                                }

                                currentNode = foundNode;
                            }

                            if (operation.ToLowerInvariant() == "select")
                            {
                                if (currentNode.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) &&
                                    selectionPattern is SelectionItemPattern selectionItemPattern)
                                {
                                    selectionItemPattern.Select();
                                    return new { elementId, operation, nodeSelected = currentNode.Current.Name, success = true, timestamp = DateTime.UtcNow };
                                }
                                return new { error = "Node does not support selection" };
                            }
                            else
                            {
                                if (currentNode.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandCollapsePattern) &&
                                    expandCollapsePattern is ExpandCollapsePattern expandCollapsePatternInstance)
                                {
                                    if (operation.ToLowerInvariant() == "expand")
                                    {
                                        expandCollapsePatternInstance.Expand();
                                    }
                                    else
                                    {
                                        expandCollapsePatternInstance.Collapse();
                                    }
                                    return new { elementId, operation, nodeAffected = currentNode.Current.Name, success = true, timestamp = DateTime.UtcNow };
                                }
                                return new { error = "Node does not support expand/collapse" };
                            }

                        case "getnodes":
                            var allNodes = treeView.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem));
                            var nodeList = allNodes.Cast<AutomationElement>().Select(node => new
                            {
                                name = node.Current.Name,
                                automationId = node.Current.AutomationId,
                                isSelected = node.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) &&
                                           itemPattern is SelectionItemPattern selectionItem &&
                                           selectionItem.Current.IsSelected,
                                isExpanded = node.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) &&
                                           expandPattern is ExpandCollapsePattern expandCollapsePattern &&
                                           expandCollapsePattern.Current.ExpandCollapseState == ExpandCollapseState.Expanded
                            }).ToList();

                            return new { elementId, operation, nodes = nodeList, totalNodes = nodeList.Count, success = true, timestamp = DateTime.UtcNow };

                        case "getselected":
                            var selectedNodes = treeView.FindAll(TreeScope.Descendants,
                                new AndCondition(
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem),
                                    new PropertyCondition(SelectionItemPattern.IsSelectedProperty, true)
                                ));

                            var selectedNodeNames = selectedNodes.Cast<AutomationElement>()
                                .Select(node => node.Current.Name)
                                .ToArray();

                            return new { elementId, operation, selectedNodes = selectedNodeNames, success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: expand, collapse, select, getnodes, getselected" };
                    }
                }, timeoutSeconds, $"TreeViewOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing TreeView operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}