using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace UIAutomationMCP.Server.Services
{
    public class ControlTypeService : IControlTypeService
    {
        private readonly ILogger<ControlTypeService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly ElementInfoExtractor _elementInfoExtractor;
        private readonly AutomationHelper _automationHelper;

        public ControlTypeService(ILogger<ControlTypeService> logger, UIAutomationExecutor executor, ElementInfoExtractor elementInfoExtractor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _elementInfoExtractor = elementInfoExtractor;
            _automationHelper = automationHelper;
        }

        public async Task<object> ComboBoxOperationAsync(string elementId, string operation, string? itemToSelect = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing ComboBox operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var element = _automationHelper.FindElementById(elementId, searchRoot);

                    if (element == null)
                    {
                        return new { Success = false, Error = $"ComboBox '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "open":
                            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPatternObj) &&
                                expandPatternObj is ExpandCollapsePattern expandPattern)
                            {
                                expandPattern.Expand();
                                return new { elementId, operation, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "ComboBox does not support expand operation" };

                        case "close":
                            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var collapsePatternObj) &&
                                collapsePatternObj is ExpandCollapsePattern collapsePattern)
                            {
                                collapsePattern.Collapse();
                                return new { elementId, operation, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "ComboBox does not support collapse operation" };

                        case "select":
                            if (string.IsNullOrEmpty(itemToSelect))
                            {
                                return new { error = "itemToSelect is required for select operation" };
                            }

                            // First try to expand if needed
                            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandForSelectObj) &&
                                expandForSelectObj is ExpandCollapsePattern expandForSelect &&
                                expandForSelect.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                            {
                                expandForSelect.Expand();
                                System.Threading.Thread.Sleep(500); // Give time for expansion
                            }

                            // Find the item to select
                            var items = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));
                            var targetItem = items.Cast<AutomationElement>().FirstOrDefault(item => 
                                item.Current.Name.Equals(itemToSelect, StringComparison.OrdinalIgnoreCase));

                            if (targetItem == null)
                            {
                                return new { error = $"Item '{itemToSelect}' not found in ComboBox" };
                            }

                            if (targetItem.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPatternObj) &&
                                selectionPatternObj is SelectionItemPattern selectionPattern)
                            {
                                selectionPattern.Select();
                                return new { elementId, operation, itemSelected = itemToSelect, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Item does not support selection" };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: open, close, select" };
                    }
                }, timeoutSeconds, $"ComboBoxOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing ComboBox operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> MenuOperationAsync(string menuPath, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing Menu operation: {MenuPath}", menuPath);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var menuItems = menuPath.Split(new[] { '/', '\\', '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (menuItems.Length == 0)
                    {
                        return new { error = "Invalid menu path" };
                    }

                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    AutomationElement currentElement = searchRoot;

                    foreach (var menuItem in menuItems)
                    {
                        var menuElement = currentElement.FindFirst(TreeScope.Children,
                            new AndCondition(
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem),
                                new PropertyCondition(AutomationElement.NameProperty, menuItem)
                            ));

                        if (menuElement == null)
                        {
                            return new { error = $"Menu item '{menuItem}' not found" };
                        }

                        // If this is the last item, invoke it
                        if (menuItem == menuItems.Last())
                        {
                            if (menuElement.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) &&
                                invokePattern is InvokePattern invokePatternInstance)
                            {
                                invokePatternInstance.Invoke();
                                return new { menuPath, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = $"Menu item '{menuItem}' cannot be invoked" };
                        }

                        currentElement = menuElement;
                    }

                    return new { error = "Menu operation failed" };
                }, timeoutSeconds, $"MenuOperation_{menuPath}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Menu operation: {MenuPath}", menuPath);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> TabControlOperationAsync(string elementId, string operation, string? tabName = null, int? tabIndex = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing TabControl operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var tabControl = _automationHelper.FindElementById(elementId, searchRoot);

                    if (tabControl == null)
                    {
                        return new { error = $"TabControl '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "select":
                            AutomationElement? tabToSelect = null;

                            if (!string.IsNullOrEmpty(tabName))
                            {
                                tabToSelect = tabControl.FindFirst(TreeScope.Children,
                                    new AndCondition(
                                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem),
                                        new PropertyCondition(AutomationElement.NameProperty, tabName)
                                    ));
                            }
                            else if (tabIndex.HasValue)
                            {
                                var tabs = tabControl.FindAll(TreeScope.Children,
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

                                if (tabIndex.Value >= 0 && tabIndex.Value < tabs.Count)
                                {
                                    tabToSelect = tabs[tabIndex.Value];
                                }
                            }

                            if (tabToSelect == null)
                            {
                                return new { error = "Tab not found" };
                            }

                            if (tabToSelect.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) &&
                                selectionPattern is SelectionItemPattern selectionItemPattern)
                            {
                                selectionItemPattern.Select();
                                return new { elementId, operation, tabSelected = tabToSelect.Current.Name, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Tab does not support selection" };

                        case "getselected":
                            var selectedTab = tabControl.FindFirst(TreeScope.Children,
                                new AndCondition(
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem),
                                    new PropertyCondition(SelectionItemPattern.IsSelectedProperty, true)
                                ));

                            if (selectedTab != null)
                            {
                                return new { elementId, selectedTab = selectedTab.Current.Name, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "No tab is currently selected" };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: select, getselected" };
                    }
                }, timeoutSeconds, $"TabControlOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing TabControl operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
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

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: expand, collapse, select" };
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

        public async Task<object> ListViewOperationAsync(string elementId, string operation, string? itemName = null, int? itemIndex = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing ListView operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var listView = _automationHelper.FindElementById(elementId, searchRoot);

                    if (listView == null)
                    {
                        return new { error = $"ListView '{elementId}' not found" };
                    }

                    switch (operation.ToLowerInvariant())
                    {
                        case "select":
                            AutomationElement? itemToSelect = null;

                            if (!string.IsNullOrEmpty(itemName))
                            {
                                itemToSelect = listView.FindFirst(TreeScope.Children,
                                    new AndCondition(
                                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
                                        new PropertyCondition(AutomationElement.NameProperty, itemName)
                                    ));
                            }
                            else if (itemIndex.HasValue)
                            {
                                var items = listView.FindAll(TreeScope.Children,
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));

                                if (itemIndex.Value >= 0 && itemIndex.Value < items.Count)
                                {
                                    itemToSelect = items[itemIndex.Value];
                                }
                            }

                            if (itemToSelect == null)
                            {
                                return new { error = "List item not found" };
                            }

                            if (itemToSelect.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) &&
                                selectionPattern is SelectionItemPattern selectionItemPattern)
                            {
                                selectionItemPattern.Select();
                                return new { elementId, operation, itemSelected = itemToSelect.Current.Name, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Item does not support selection" };

                        case "getselected":
                            var selectedItems = listView.FindAll(TreeScope.Children,
                                new AndCondition(
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
                                    new PropertyCondition(SelectionItemPattern.IsSelectedProperty, true)
                                ));

                            var selectedItemNames = selectedItems.Cast<AutomationElement>()
                                .Select(item => item.Current.Name)
                                .ToArray();

                            return new { elementId, selectedItems = selectedItemNames, success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: select, getselected" };
                    }
                }, timeoutSeconds, $"ListViewOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing ListView operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> TabOperationAsync(string elementId, string operation, string? tabName = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Executing tab operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var condition = new OrCondition(
                        new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                        new PropertyCondition(AutomationElement.NameProperty, elementId)
                    );

                    var element = searchRoot.FindFirst(TreeScope.Descendants, condition);
                    if (element == null)
                    {
                        return new { error = $"Tab element '{elementId}' not found" };
                    }

                    switch (operation.ToLower())
                    {
                        case "select":
                            if (string.IsNullOrEmpty(tabName))
                            {
                                return new { error = "tabName is required for select operation" };
                            }

                            // Find the specific tab item
                            var tabItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                            var targetTab = tabItems.Cast<AutomationElement>().FirstOrDefault(tab => 
                                tab.Current.Name.Equals(tabName, StringComparison.OrdinalIgnoreCase));

                            if (targetTab == null)
                            {
                                return new { error = $"Tab '{tabName}' not found" };
                            }

                            if (targetTab.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) &&
                                selectionPattern is SelectionItemPattern selectionItemPattern)
                            {
                                selectionItemPattern.Select();
                                return new { elementId, operation, tabSelected = tabName, success = true, timestamp = DateTime.UtcNow };
                            }
                            return new { error = "Tab does not support selection" };

                        case "getselected":
                            if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) &&
                                pattern is SelectionPattern selectionPatternGet)
                            {
                                var selectedItems = selectionPatternGet.Current.GetSelection();
                                var selectedTab = selectedItems.FirstOrDefault();
                                return new { 
                                    elementId, 
                                    operation, 
                                    selectedTab = selectedTab?.Current.Name ?? "None",
                                    selectedTabId = selectedTab?.Current.AutomationId ?? "None",
                                    success = true, 
                                    timestamp = DateTime.UtcNow 
                                };
                            }
                            return new { error = "Tab control does not support selection pattern" };

                        case "gettabs":
                            var allTabItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                            var tabList = allTabItems.Cast<AutomationElement>().Select(tab => new
                            {
                                name = tab.Current.Name,
                                automationId = tab.Current.AutomationId,
                                isSelected = tab.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) &&
                                           itemPattern is SelectionItemPattern selectionItem &&
                                           selectionItem.Current.IsSelected
                            }).ToList();

                            return new { 
                                elementId, 
                                operation, 
                                tabs = tabList,
                                totalTabs = tabList.Count,
                                success = true, 
                                timestamp = DateTime.UtcNow 
                            };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: select, getselected, gettabs" };
                    }
                }, timeoutSeconds, $"TabOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Tab operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}
