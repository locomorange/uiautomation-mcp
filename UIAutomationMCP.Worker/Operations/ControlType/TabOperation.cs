using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class TabOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public TabOperation(ElementFinderService elementFinderService)
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

            if (element.Current.ControlType != System.Windows.Automation.ControlType.Tab)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a tab control" });

            try
            {
                switch (operation.ToLower())
                {
                    case "getinfo":
                        var tabInfo = new Dictionary<string, object>
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
                            tabInfo["CanSelectMultiple"] = selection.Current.CanSelectMultiple;
                            tabInfo["IsSelectionRequired"] = selection.Current.IsSelectionRequired;
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = tabInfo });

                    case "gettabs":
                        var tabItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.TabItem));
                        var tabs = new List<Dictionary<string, object>>();
                        
                        foreach (AutomationElement item in tabItems)
                        {
                            var tabItemInfo = new Dictionary<string, object>
                            {
                                ["Name"] = item.Current.Name,
                                ["AutomationId"] = item.Current.AutomationId,
                                ["IsEnabled"] = item.Current.IsEnabled,
                                ["IsSelected"] = false
                            };

                            // Check if tab is selected
                            if (item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && itemPattern is SelectionItemPattern selItem)
                            {
                                tabItemInfo["IsSelected"] = selItem.Current.IsSelected;
                            }

                            tabs.Add(tabItemInfo);
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = tabs });

                    case "selecttab":
                        var selectTab = request.Parameters?.GetValueOrDefault("tab")?.ToString() ?? "";
                        var tabCondition = new PropertyCondition(AutomationElement.NameProperty, selectTab);
                        var targetTab = element.FindFirst(TreeScope.Children, tabCondition);
                        
                        if (targetTab != null && targetTab.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectTabPattern) && selectTabPattern is SelectionItemPattern selectTab2)
                        {
                            selectTab2.Select();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Tab not found or does not support selection" });

                    case "getselectedtab":
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var getSelectionPattern) && getSelectionPattern is SelectionPattern getSelection)
                        {
                            var selectedTabs = getSelection.Current.GetSelection();
                            if (selectedTabs.Length > 0)
                            {
                                var selectedTab = selectedTabs[0];
                                var selectedTabInfo = new Dictionary<string, object>
                                {
                                    ["Name"] = selectedTab.Current.Name,
                                    ["AutomationId"] = selectedTab.Current.AutomationId
                                };
                                return Task.FromResult(new OperationResult { Success = true, Data = selectedTabInfo });
                            }
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "No tab selected or tab control does not support selection" });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing tab operation: {ex.Message}" });
            }
        }
    }
}