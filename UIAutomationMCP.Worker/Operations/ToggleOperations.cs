using System.Windows.Automation;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    public class ToggleOperations
    {
        public OperationResult ToggleElement(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return new OperationResult { Success = false, Error = "TogglePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var currentState = togglePattern.Current.ToggleState;
            togglePattern.Toggle();
            var newState = togglePattern.Current.ToggleState;
            
            return new OperationResult 
            { 
                Success = true, 
                Data = new { CurrentState = currentState.ToString(), NewState = newState.ToString() }
            };
        }

        /// <summary>
        /// Get current toggle state
        /// </summary>
        public OperationResult GetToggleState(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return new OperationResult { Success = false, Error = "TogglePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var state = togglePattern.Current.ToggleState;
            return new OperationResult { Success = true, Data = state.ToString() };
        }

        /// <summary>
        /// Set specific toggle state
        /// </summary>
        public OperationResult SetToggleState(string elementId, string toggleState, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return new OperationResult { Success = false, Error = "TogglePattern not supported" };

            // Parse target state
            if (!Enum.TryParse<ToggleState>(toggleState, true, out var targetState))
                return new OperationResult { Success = false, Error = $"Invalid toggle state: {toggleState}. Valid values: On, Off, Indeterminate" };

            // Let exceptions flow naturally - no try-catch
            var currentState = togglePattern.Current.ToggleState;
            
            // Toggle until we reach the desired state (TogglePattern cycles through states)
            while (currentState != targetState)
            {
                togglePattern.Toggle();
                var newState = togglePattern.Current.ToggleState;
                
                // Prevent infinite loop if element doesn't support the target state
                if (newState == currentState)
                {
                    return new OperationResult { Success = false, Error = $"Element does not support toggle state: {toggleState}" };
                }
                
                currentState = newState;
            }

            return new OperationResult 
            { 
                Success = true, 
                Data = new { TargetState = targetState.ToString(), FinalState = currentState.ToString() }
            };
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