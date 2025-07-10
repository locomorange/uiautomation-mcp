using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class ComboBoxOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ComboBoxOperation(ElementFinderService elementFinderService)
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

            if (element.Current.ControlType != ControlType.ComboBox)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a combo box" });

            try
            {
                switch (operation.ToLower())
                {
                    case "getinfo":
                        var comboBoxInfo = new Dictionary<string, object>
                        {
                            ["Name"] = element.Current.Name,
                            ["AutomationId"] = element.Current.AutomationId,
                            ["IsEnabled"] = element.Current.IsEnabled,
                            ["IsVisible"] = !element.Current.IsOffscreen,
                            ["SupportedPatterns"] = element.GetSupportedPatterns().Select(p => p.ProgrammaticName).ToList()
                        };

                        // Check for current value
                        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern value)
                        {
                            comboBoxInfo["CurrentValue"] = value.Current.Value;
                            comboBoxInfo["IsReadOnly"] = value.Current.IsReadOnly;
                        }

                        // Check for expand/collapse state
                        if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern expand)
                        {
                            comboBoxInfo["ExpandCollapseState"] = expand.Current.ExpandCollapseState.ToString();
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = comboBoxInfo });

                    case "expand":
                        if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern2) && expandPattern2 is ExpandCollapsePattern expand2)
                        {
                            expand2.Expand();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "ComboBox does not support expand" });

                    case "collapse":
                        if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var collapsePattern) && collapsePattern is ExpandCollapsePattern collapse)
                        {
                            collapse.Collapse();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "ComboBox does not support collapse" });

                    case "setvalue":
                        var setValue = request.Parameters?.GetValueOrDefault("value")?.ToString() ?? "";
                        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var setValuePattern) && setValuePattern is ValuePattern setValue2)
                        {
                            setValue2.SetValue(setValue);
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "ComboBox does not support set value" });

                    case "select":
                        var selectItem = request.Parameters?.GetValueOrDefault("item")?.ToString() ?? "";
                        if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionPattern selection)
                        {
                            // Find the item to select
                            var itemCondition = new PropertyCondition(AutomationElement.NameProperty, selectItem);
                            var item = element.FindFirst(TreeScope.Descendants, itemCondition);
                            if (item != null && item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && itemPattern is SelectionItemPattern selItem)
                            {
                                selItem.Select();
                                return Task.FromResult(new OperationResult { Success = true });
                            }
                            return Task.FromResult(new OperationResult { Success = false, Error = "Item not found in combo box" });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "ComboBox does not support selection" });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing combo box operation: {ex.Message}" });
            }
        }
    }
}