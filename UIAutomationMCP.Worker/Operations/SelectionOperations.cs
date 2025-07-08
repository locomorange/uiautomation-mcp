using System.Windows.Automation;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    public class SelectionOperations
    {
        public OperationResult SelectElement(string elementId, string windowTitle = "", int processId = 0)
        {
            try
            {
                var element = FindElementById(elementId, windowTitle, processId);
                if (element == null)
                {
                    return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };
                }

                if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) && pattern is SelectionItemPattern selectionPattern)
                {
                    selectionPattern.Select();
                    return new OperationResult { Success = true, Data = "Element selected successfully" };
                }
                else
                {
                    return new OperationResult { Success = false, Error = "Element does not support SelectionItemPattern" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public OperationResult GetSelection(string containerElementId, string windowTitle = "", int processId = 0)
        {
            try
            {
                var element = FindElementById(containerElementId, windowTitle, processId);
                if (element == null)
                {
                    return new OperationResult { Success = false, Error = $"Container element '{containerElementId}' not found" };
                }

                if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) && pattern is SelectionPattern selectionPattern)
                {
                    var selection = selectionPattern.Current.GetSelection();
                    var selectedInfo = new List<object>();

                    foreach (AutomationElement selectedElement in selection)
                    {
                        try
                        {
                            selectedInfo.Add(new
                            {
                                AutomationId = selectedElement.Current.AutomationId,
                                Name = selectedElement.Current.Name,
                                ControlType = selectedElement.Current.ControlType.LocalizedControlType
                            });
                        }
                        catch (Exception)
                        {
                            // Skip elements that can't be processed
                        }
                    }

                    return new OperationResult { Success = true, Data = selectedInfo };
                }
                else
                {
                    return new OperationResult { Success = false, Error = "Container element does not support SelectionPattern" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = ex.Message };
            }
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