using System.Windows.Automation;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    public class SelectionOperations
    {
        public OperationResult SelectElement(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                return new OperationResult { Success = false, Error = "SelectionItemPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            selectionPattern.Select();
            return new OperationResult { Success = true, Data = "Element selected successfully" };
        }

        /// <summary>
        /// Select item - alias for SelectElement to match interface
        /// </summary>
        public OperationResult SelectItem(string elementId, string windowTitle = "", int processId = 0)
        {
            return SelectElement(elementId, windowTitle, processId);
        }

        /// <summary>
        /// Add element to selection (for multi-select containers)
        /// </summary>
        public OperationResult AddToSelection(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                return new OperationResult { Success = false, Error = "SelectionItemPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            selectionPattern.AddToSelection();
            return new OperationResult { Success = true, Data = "Element added to selection successfully" };
        }

        /// <summary>
        /// Remove element from selection
        /// </summary>
        public OperationResult RemoveFromSelection(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionPattern)
                return new OperationResult { Success = false, Error = "SelectionItemPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            selectionPattern.RemoveFromSelection();
            return new OperationResult { Success = true, Data = "Element removed from selection successfully" };
        }

        /// <summary>
        /// Clear all selections in container
        /// </summary>
        public OperationResult ClearSelection(string containerElementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(containerElementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Container element not found" };

            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                return new OperationResult { Success = false, Error = "SelectionPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var selection = selectionPattern.Current.GetSelection();
            foreach (AutomationElement selectedElement in selection)
            {
                if (selectedElement != null && 
                    selectedElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var itemPattern) && 
                    itemPattern is SelectionItemPattern itemSelectionPattern)
                {
                    itemSelectionPattern.RemoveFromSelection();
                }
            }

            return new OperationResult { Success = true, Data = "Selection cleared successfully" };
        }

        public OperationResult GetSelection(string containerElementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(containerElementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Container element not found" };

            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                return new OperationResult { Success = false, Error = "SelectionPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var selection = selectionPattern.Current.GetSelection();
            var selectedInfo = new List<object>();

            foreach (AutomationElement selectedElement in selection)
            {
                // Minimal protection for element enumeration
                if (selectedElement != null)
                {
                    selectedInfo.Add(new
                    {
                        AutomationId = selectedElement.Current.AutomationId,
                        Name = selectedElement.Current.Name,
                        ControlType = selectedElement.Current.ControlType.LocalizedControlType
                    });
                }
            }

            return new OperationResult { Success = true, Data = selectedInfo };
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
