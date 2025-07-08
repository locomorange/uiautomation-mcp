using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// InvokePattern操作の最小粒度API
    /// </summary>
    public class InvokeOperations
    {
        public InvokeOperations()
        {
        }

        /// <summary>
        /// 要素をInvokeする
        /// </summary>
        public OperationResult Invoke(AutomationElement element)
        {
            if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern) || pattern is not InvokePattern invokePattern)
                return new OperationResult { Success = false, Error = "InvokePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            invokePattern.Invoke();
            return new OperationResult { Success = true };
        }

        /// <summary>
        /// 要素がInvokePatternをサポートしているかチェック
        /// </summary>
        public OperationResult<bool> SupportsInvoke(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            bool supportsPattern = element.TryGetCurrentPattern(InvokePattern.Pattern, out _);
            return new OperationResult<bool> { Success = true, Data = supportsPattern };
        }

        /// <summary>
        /// 要素IDで要素を検索してInvokeする
        /// </summary>
        public OperationResult InvokeElement(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            return Invoke(element);
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