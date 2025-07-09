using System.Windows.Automation;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    public class ControlTypeOperations
    {
        public OperationResult ButtonOperation(string elementId, string operation, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Button element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "click":
                    if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) && invokePattern is InvokePattern ip)
                    {
                        ip.Invoke();
                        return new OperationResult { Success = true, Data = "Button clicked successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "Button does not support InvokePattern" };
                    }

                case "getstatus":
                    return new OperationResult
                    {
                        Success = true,
                        Data = new
                        {
                            IsEnabled = element.Current.IsEnabled,
                            IsVisible = !element.Current.IsOffscreen,
                            HasKeyboardFocus = element.Current.HasKeyboardFocus,
                            Name = element.Current.Name,
                            AutomationId = element.Current.AutomationId
                        }
                    };

                case "toggle":
                    if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) && togglePattern is TogglePattern tp)
                    {
                        var oldState = tp.Current.ToggleState;
                        tp.Toggle();
                        var newState = tp.Current.ToggleState;
                        return new OperationResult
                        {
                            Success = true,
                            Data = new { OldState = oldState.ToString(), NewState = newState.ToString() }
                        };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "Button does not support TogglePattern" };
                    }

                case "gettext":
                    var text = element.Current.Name ?? "";
                    return new OperationResult { Success = true, Data = new { Text = text } };

                case "isfocused":
                    return new OperationResult
                    {
                        Success = true,
                        Data = new { HasFocus = element.Current.HasKeyboardFocus }
                    };

                case "setfocus":
                    element.SetFocus();
                    return new OperationResult { Success = true, Data = "Focus set successfully" };

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported button operation: {operation}" };
            }
        }

        public OperationResult CalendarOperation(string elementId, string operation, string? dateString = null, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Calendar element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "selectdate":
                    if (string.IsNullOrEmpty(dateString) || !DateTime.TryParse(dateString, out var date))
                        return new OperationResult { Success = false, Error = "Invalid date format" };

                    if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionPattern sp)
                    {
                        // Find date element and select it
                        var dateCondition = new PropertyCondition(AutomationElement.NameProperty, date.ToString("d"));
                        var dateElement = element.FindFirst(TreeScope.Descendants, dateCondition);
                        if (dateElement != null && dateElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && itemPattern is SelectionItemPattern sip)
                        {
                            sip.Select();
                            return new OperationResult { Success = true, Data = $"Date {date:yyyy-MM-dd} selected successfully" };
                        }
                        else
                        {
                            return new OperationResult { Success = false, Error = $"Date {date:yyyy-MM-dd} not found in calendar" };
                        }
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "Calendar does not support SelectionPattern" };
                    }

                case "getselecteddate":
                    if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var getSelectionPattern) && getSelectionPattern is SelectionPattern gsp)
                    {
                        var selection = gsp.Current.GetSelection();
                        if (selection.Length > 0)
                        {
                            var selectedElement = selection[0];
                            return new OperationResult
                            {
                                Success = true,
                                Data = new { SelectedDate = selectedElement.Current.Name }
                            };
                        }
                        else
                        {
                            return new OperationResult { Success = true, Data = new { SelectedDate = (string?)null } };
                        }
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "Calendar does not support SelectionPattern" };
                    }

                case "navigate":
                    // Basic navigation - this would need more specific implementation based on calendar type
                    return new OperationResult { Success = true, Data = "Calendar navigation not fully implemented" };

                case "getavailabledates":
                    var dateElements = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
                    var availableDates = new List<string>();
                    foreach (AutomationElement dateElement in dateElements)
                    {
                        if (dateElement.Current.IsEnabled)
                        {
                            availableDates.Add(dateElement.Current.Name);
                        }
                    }
                    return new OperationResult { Success = true, Data = new { AvailableDates = availableDates } };

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported calendar operation: {operation}" };
            }
        }

        public OperationResult ComboBoxOperation(string elementId, string operation, string? itemToSelect = null, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "ComboBox element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "open":
                    if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern ep)
                    {
                        ep.Expand();
                        return new OperationResult { Success = true, Data = "ComboBox opened successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "ComboBox does not support ExpandCollapsePattern" };
                    }

                case "close":
                    if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var collapsePattern) && collapsePattern is ExpandCollapsePattern cp)
                    {
                        cp.Collapse();
                        return new OperationResult { Success = true, Data = "ComboBox closed successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "ComboBox does not support ExpandCollapsePattern" };
                    }

                case "select":
                    if (string.IsNullOrEmpty(itemToSelect))
                        return new OperationResult { Success = false, Error = "Item to select is required" };

                    // First expand if collapsed
                    if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var selectExpandPattern) && selectExpandPattern is ExpandCollapsePattern sep)
                    {
                        if (sep.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                        {
                            sep.Expand();
                        }
                    }

                    // Find and select item
                    var itemCondition = new PropertyCondition(AutomationElement.NameProperty, itemToSelect);
                    var itemElement = element.FindFirst(TreeScope.Descendants, itemCondition);
                    if (itemElement != null && itemElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemSelectionPattern) && itemSelectionPattern is SelectionItemPattern sip)
                    {
                        sip.Select();
                        return new OperationResult { Success = true, Data = $"Item '{itemToSelect}' selected successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = $"Item '{itemToSelect}' not found in ComboBox" };
                    }

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported ComboBox operation: {operation}" };
            }
        }

        public OperationResult HyperlinkOperation(string elementId, string operation, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Hyperlink element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "click":
                    if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) && invokePattern is InvokePattern ip)
                    {
                        ip.Invoke();
                        return new OperationResult { Success = true, Data = "Hyperlink clicked successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "Hyperlink does not support InvokePattern" };
                    }

                case "gettext":
                    var text = element.Current.Name ?? "";
                    return new OperationResult { Success = true, Data = new { Text = text } };

                case "geturl":
                    var url = element.Current.HelpText ?? element.Current.Name ?? "";
                    return new OperationResult { Success = true, Data = new { Url = url } };

                case "getstatus":
                    return new OperationResult
                    {
                        Success = true,
                        Data = new
                        {
                            IsEnabled = element.Current.IsEnabled,
                            IsVisible = !element.Current.IsOffscreen,
                            Text = element.Current.Name,
                            Url = element.Current.HelpText
                        }
                    };

                case "setfocus":
                    element.SetFocus();
                    return new OperationResult { Success = true, Data = "Focus set successfully" };

                case "isfocused":
                    return new OperationResult
                    {
                        Success = true,
                        Data = new { HasFocus = element.Current.HasKeyboardFocus }
                    };

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported hyperlink operation: {operation}" };
            }
        }

        public OperationResult ListOperation(string elementId, string operation, string? itemName = null, int? itemIndex = null, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "List element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "select":
                    AutomationElement? itemElement = null;
                    
                    if (!string.IsNullOrEmpty(itemName))
                    {
                        var nameCondition = new PropertyCondition(AutomationElement.NameProperty, itemName);
                        itemElement = element.FindFirst(TreeScope.Descendants, nameCondition);
                    }
                    else if (itemIndex.HasValue)
                    {
                        var items = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));
                        if (itemIndex.Value >= 0 && itemIndex.Value < items.Count)
                        {
                            itemElement = items[itemIndex.Value];
                        }
                    }

                    if (itemElement != null && itemElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var listSelectionPattern) && listSelectionPattern is SelectionItemPattern listSip)
                    {
                        listSip.Select();
                        return new OperationResult { Success = true, Data = $"List item selected successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "List item not found or does not support selection" };
                    }

                case "getselected":
                    if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var getSelectionPattern) && getSelectionPattern is SelectionPattern gsp)
                    {
                        var selection = gsp.Current.GetSelection();
                        var selectedItems = new List<object>();
                        foreach (var selectedElement in selection)
                        {
                            selectedItems.Add(new
                            {
                                Name = selectedElement.Current.Name,
                                AutomationId = selectedElement.Current.AutomationId
                            });
                        }
                        return new OperationResult { Success = true, Data = new { SelectedItems = selectedItems } };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = "List does not support SelectionPattern" };
                    }

                case "getitems":
                    var allItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));
                    var itemList = new List<object>();
                    for (int i = 0; i < allItems.Count; i++)
                    {
                        var item = allItems[i];
                        itemList.Add(new
                        {
                            Index = i,
                            Name = item.Current.Name,
                            AutomationId = item.Current.AutomationId,
                            IsEnabled = item.Current.IsEnabled
                        });
                    }
                    return new OperationResult { Success = true, Data = new { Items = itemList } };

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported list operation: {operation}" };
            }
        }

        public OperationResult MenuOperation(string menuPath, string windowTitle = "", int processId = 0)
        {
            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var pathParts = menuPath.Split('/');

            AutomationElement currentElement = searchRoot;
            
            // Let exceptions flow naturally - no try-catch
            foreach (var part in pathParts)
            {
                var menuCondition = new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, part),
                    new OrCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Menu),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem)
                    )
                );

                var menuElement = currentElement.FindFirst(TreeScope.Descendants, menuCondition);
                if (menuElement == null)
                    return new OperationResult { Success = false, Error = $"Menu item '{part}' not found" };

                // Expand menu if it has children
                if (menuElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern ep)
                {
                    if (ep.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                    {
                        ep.Expand();
                    }
                }

                currentElement = menuElement;
            }

            // Invoke the final menu item
            if (currentElement.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) && invokePattern is InvokePattern ip)
            {
                ip.Invoke();
                return new OperationResult { Success = true, Data = $"Menu item '{menuPath}' invoked successfully" };
            }
            else
            {
                return new OperationResult { Success = false, Error = $"Menu item '{menuPath}' does not support InvokePattern" };
            }
        }

        public OperationResult TabOperation(string elementId, string operation, string? tabName = null, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Tab control element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "list":
                    var tabItems = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                    var tabList = new List<object>();
                    for (int i = 0; i < tabItems.Count; i++)
                    {
                        var tab = tabItems[i];
                        tabList.Add(new
                        {
                            Index = i,
                            Name = tab.Current.Name,
                            AutomationId = tab.Current.AutomationId,
                            IsSelected = tab.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) && 
                                       pattern is SelectionItemPattern tabSip && tabSip.Current.IsSelected
                        });
                    }
                    return new OperationResult { Success = true, Data = new { Tabs = tabList } };

                case "select":
                    if (string.IsNullOrEmpty(tabName))
                        return new OperationResult { Success = false, Error = "Tab name is required for select operation" };

                    var tabCondition = new AndCondition(
                        new PropertyCondition(AutomationElement.NameProperty, tabName),
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem)
                    );
                    var targetTab = element.FindFirst(TreeScope.Descendants, tabCondition);
                    if (targetTab != null && targetTab.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionItemPattern sip)
                    {
                        sip.Select();
                        return new OperationResult { Success = true, Data = $"Tab '{tabName}' selected successfully" };
                    }
                    else
                    {
                        return new OperationResult { Success = false, Error = $"Tab '{tabName}' not found or does not support selection" };
                    }

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported tab operation: {operation}" };
            }
        }

        public OperationResult TreeViewOperation(string elementId, string operation, string? nodePath = null, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "TreeView element not found" };

            // Let exceptions flow naturally - no try-catch
            switch (operation.ToLowerInvariant())
            {
                case "expand":
                case "collapse":
                case "select":
                    if (string.IsNullOrEmpty(nodePath))
                        return new OperationResult { Success = false, Error = "Node path is required for this operation" };

                    var pathParts = nodePath.Split('/');
                    AutomationElement currentNode = element;

                    foreach (var part in pathParts)
                    {
                        var nodeCondition = new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, part),
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem)
                        );
                        var node = currentNode.FindFirst(TreeScope.Children, nodeCondition);
                        if (node == null)
                            return new OperationResult { Success = false, Error = $"Tree node '{part}' not found" };

                        currentNode = node;
                    }

                    if (operation == "expand" || operation == "collapse")
                    {
                        if (currentNode.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern ep)
                        {
                            if (operation == "expand")
                                ep.Expand();
                            else
                                ep.Collapse();
                            return new OperationResult { Success = true, Data = $"Tree node '{nodePath}' {operation}ed successfully" };
                        }
                        else
                        {
                            return new OperationResult { Success = false, Error = $"Tree node '{nodePath}' does not support ExpandCollapsePattern" };
                        }
                    }
                    else if (operation == "select")
                    {
                        if (currentNode.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionItemPattern sip)
                        {
                            sip.Select();
                            return new OperationResult { Success = true, Data = $"Tree node '{nodePath}' selected successfully" };
                        }
                        else
                        {
                            return new OperationResult { Success = false, Error = $"Tree node '{nodePath}' does not support SelectionItemPattern" };
                        }
                    }
                    break;

                default:
                    return new OperationResult { Success = false, Error = $"Unsupported tree view operation: {operation}" };
            }

            return new OperationResult { Success = false, Error = "Operation not implemented" };
        }

        private AutomationElement? FindElementById(string elementId, string windowTitle, int processId)
        {
            var searchRoot = GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, elementId);
            return searchRoot.FindFirst(TreeScope.Descendants, condition);
        }

        private AutomationElement? GetSearchRoot(string windowTitle, int processId)
        {
            if (processId > 0)
            {
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            else if (!string.IsNullOrEmpty(windowTitle))
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
            }
            return null;
        }
    }
}
