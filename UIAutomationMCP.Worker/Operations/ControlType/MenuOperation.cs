using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class MenuOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public MenuOperation(ElementFinderService elementFinderService)
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

            if (element.Current.ControlType != ControlType.Menu && element.Current.ControlType != ControlType.MenuBar)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a menu or menu bar" });

            try
            {
                switch (operation.ToLower())
                {
                    case "getinfo":
                        var menuInfo = new Dictionary<string, object>
                        {
                            ["Name"] = element.Current.Name,
                            ["AutomationId"] = element.Current.AutomationId,
                            ["IsEnabled"] = element.Current.IsEnabled,
                            ["IsVisible"] = !element.Current.IsOffscreen,
                            ["ControlType"] = element.Current.ControlType.LocalizedControlType,
                            ["SupportedPatterns"] = element.GetSupportedPatterns().Select(p => p.ProgrammaticName).ToList()
                        };

                        return Task.FromResult(new OperationResult { Success = true, Data = menuInfo });

                    case "getmenuitems":
                        var menuItems = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem));
                        var items = new List<Dictionary<string, object>>();
                        
                        foreach (AutomationElement item in menuItems)
                        {
                            var itemInfo = new Dictionary<string, object>
                            {
                                ["Name"] = item.Current.Name,
                                ["AutomationId"] = item.Current.AutomationId,
                                ["IsEnabled"] = item.Current.IsEnabled,
                                ["AcceleratorKey"] = item.Current.AcceleratorKey,
                                ["AccessKey"] = item.Current.AccessKey,
                                ["HasSubMenu"] = false
                            };

                            // Check if item has submenu
                            if (item.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandPattern) && expandPattern is ExpandCollapsePattern expand)
                            {
                                itemInfo["HasSubMenu"] = expand.Current.ExpandCollapseState != ExpandCollapseState.LeafNode;
                            }

                            items.Add(itemInfo);
                        }

                        return Task.FromResult(new OperationResult { Success = true, Data = items });

                    case "clickitem":
                        var clickItem = request.Parameters?.GetValueOrDefault("item")?.ToString() ?? "";
                        var itemCondition = new PropertyCondition(AutomationElement.NameProperty, clickItem);
                        var targetItem = element.FindFirst(TreeScope.Descendants, itemCondition);
                        
                        if (targetItem != null && targetItem.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) && invokePattern is InvokePattern invoke)
                        {
                            invoke.Invoke();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Menu item not found or does not support invoke" });

                    case "expanditem":
                        var expandItem = request.Parameters?.GetValueOrDefault("item")?.ToString() ?? "";
                        var expandItemCondition = new PropertyCondition(AutomationElement.NameProperty, expandItem);
                        var targetExpandItem = element.FindFirst(TreeScope.Descendants, expandItemCondition);
                        
                        if (targetExpandItem != null && targetExpandItem.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandItemPattern) && expandItemPattern is ExpandCollapsePattern expandItem2)
                        {
                            expandItem2.Expand();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Menu item not found or does not support expand" });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing menu operation: {ex.Message}" });
            }
        }
    }
}