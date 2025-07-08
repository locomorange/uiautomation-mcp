using System.Windows.Automation;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    public class ToggleOperations
    {
        public OperationResult ToggleElement(string elementId, string windowTitle = "", int processId = 0)
        {
            try
            {
                var element = FindElementById(elementId, windowTitle, processId);
                if (element == null)
                {
                    return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };
                }

                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) && pattern is TogglePattern togglePattern)
                {
                    var currentState = togglePattern.Current.ToggleState;
                    togglePattern.Toggle();
                    var newState = togglePattern.Current.ToggleState;
                    
                    return new OperationResult 
                    { 
                        Success = true, 
                        Data = new { CurrentState = currentState.ToString(), NewState = newState.ToString() }
                    };
                }
                else
                {
                    return new OperationResult { Success = false, Error = "Element does not support TogglePattern" };
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