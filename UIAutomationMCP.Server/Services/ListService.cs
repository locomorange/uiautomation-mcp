using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services
{
    public class ListService : IListService
    {
        private readonly ILogger<ListService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public ListService(ILogger<ListService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
        }

        public async Task<object> ListOperationAsync(string elementId, string operation, string? itemName = null, int? itemIndex = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Performing List operation: {Operation} on element: {ElementId}", operation, elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    var listView = _automationHelper.FindElementById(elementId, searchRoot);

                    if (listView == null)
                    {
                        return new { error = $"List '{elementId}' not found" };
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

                        case "getitems":
                            var allItems = listView.FindAll(TreeScope.Children,
                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));

                            var itemList = allItems.Cast<AutomationElement>().Select(item => new
                            {
                                name = item.Current.Name,
                                automationId = item.Current.AutomationId,
                                isSelected = item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) &&
                                           itemPattern is SelectionItemPattern selectionItem &&
                                           selectionItem.Current.IsSelected
                            }).ToList();

                            return new { elementId, operation, items = itemList, totalItems = itemList.Count, success = true, timestamp = DateTime.UtcNow };

                        case "selectmultiple":
                            if (string.IsNullOrEmpty(itemName))
                            {
                                return new { error = "itemName is required for selectmultiple operation" };
                            }

                            var itemNames = itemName.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(name => name.Trim())
                                .ToArray();

                            var selectedCount = 0;
                            foreach (var name in itemNames)
                            {
                                var item = listView.FindFirst(TreeScope.Children,
                                    new AndCondition(
                                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
                                        new PropertyCondition(AutomationElement.NameProperty, name)
                                    ));

                                if (item != null && item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) &&
                                    pattern is SelectionItemPattern selectionItemPatternMultiple)
                                {
                                    selectionItemPatternMultiple.AddToSelection();
                                    selectedCount++;
                                }
                            }

                            return new { elementId, operation, itemsSelected = selectedCount, totalRequested = itemNames.Length, success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: select, getselected, getitems, selectmultiple" };
                    }
                }, timeoutSeconds, $"ListOperation_{operation}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing List operation: {Operation} on element: {ElementId}", operation, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }
    }
}