using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Services.ControlTypes
{
    public class TabService : ITabService
    {
        private readonly ILogger<TabService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;

        public TabService(ILogger<TabService> logger, UIAutomationExecutor executor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
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