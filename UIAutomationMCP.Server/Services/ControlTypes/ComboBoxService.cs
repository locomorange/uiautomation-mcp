using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class ComboBoxService : IComboBoxService
    {
        private readonly ILogger<ComboBoxService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public ComboBoxService(ILogger<ComboBoxService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
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

                        case "getitems":
                            var allItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));
                            var itemList = allItems.Cast<AutomationElement>().Select(item => new
                            {
                                name = item.Current.Name,
                                automationId = item.Current.AutomationId,
                                isSelected = item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) &&
                                           itemPattern is SelectionItemPattern selectionItem &&
                                           selectionItem.Current.IsSelected
                            }).ToList();

                            return new { elementId, operation, items = itemList, totalItems = itemList.Count, success = true, timestamp = DateTime.UtcNow };

                        case "getselected":
                            var selectedItems = element.FindAll(TreeScope.Children,
                                new AndCondition(
                                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem),
                                    new PropertyCondition(SelectionItemPattern.IsSelectedProperty, true)
                                ));

                            var selectedItem = selectedItems.Cast<AutomationElement>().FirstOrDefault();
                            return new { elementId, operation, selectedItem = selectedItem?.Current.Name ?? "None", success = true, timestamp = DateTime.UtcNow };

                        default:
                            return new { error = $"Unknown operation: {operation}. Supported operations: open, close, select, getitems, getselected" };
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
    }
}