using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class ListOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ListOperation(ElementFinderService elementFinderService)
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

            if (element.Current.ControlType != System.Windows.Automation.ControlType.List)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a list" });

            try
            {
                switch (operation.ToLower())
                {
                    case "getinfo":
                        var listInfo = new Dictionary<string, object>
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
                            listInfo["CanSelectMultiple"] = selection.Current.CanSelectMultiple;
                            listInfo["IsSelectionRequired"] = selection.Current.IsSelectionRequired;
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = listInfo });

                    case "getitems":
                        var listItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.ListItem));
                        var items = new List<Dictionary<string, object>>();
                        
                        foreach (AutomationElement item in listItems)
                        {
                            var itemInfo = new Dictionary<string, object>
                            {
                                ["Name"] = item.Current.Name,
                                ["AutomationId"] = item.Current.AutomationId,
                                ["IsEnabled"] = item.Current.IsEnabled,
                                ["IsSelected"] = false
                            };

                            // Check if item is selected
                            if (item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && itemPattern is SelectionItemPattern selItem)
                            {
                                itemInfo["IsSelected"] = selItem.Current.IsSelected;
                            }

                            items.Add(itemInfo);
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = items });

                    case "select":
                        var selectItem = request.Parameters?.GetValueOrDefault("item")?.ToString() ?? "";
                        var itemCondition = new PropertyCondition(AutomationElement.NameProperty, selectItem);
                        var targetItem = element.FindFirst(TreeScope.Children, itemCondition);
                        
                        if (targetItem != null && targetItem.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectItemPattern) && selectItemPattern is SelectionItemPattern selectItem2)
                        {
                            selectItem2.Select();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Item not found in list or does not support selection" });

                    case "getselection":
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var getSelectionPattern) && getSelectionPattern is SelectionPattern getSelection)
                        {
                            var selectedItems = getSelection.Current.GetSelection();
                            var selectedItemsInfo = selectedItems.Select(item => new Dictionary<string, object>
                            {
                                ["Name"] = item.Current.Name,
                                ["AutomationId"] = item.Current.AutomationId
                            }).ToList();

                            return Task.FromResult(new OperationResult { Success = true, Data = selectedItemsInfo });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "List does not support selection" });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing list operation: {ex.Message}" });
            }
        }
    }
}