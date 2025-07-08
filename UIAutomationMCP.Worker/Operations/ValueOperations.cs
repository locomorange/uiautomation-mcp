using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// ValuePattern操作の最小粒度API
    /// </summary>
    public class ValueOperations
    {
        public ValueOperations()
        {
        }

        /// <summary>
        /// 要素の値を設定
        /// </summary>
        public OperationResult SetValue(AutomationElement element, string value)
        {
            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return new OperationResult { Success = false, Error = "ValuePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            valuePattern.SetValue(value);
            return new OperationResult { Success = true };
        }

        /// <summary>
        /// 要素の値を取得
        /// </summary>
        public OperationResult<string> GetValue(AutomationElement element)
        {
            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return new OperationResult<string> { Success = false, Error = "ValuePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var value = valuePattern.Current.Value;
            return new OperationResult<string> { Success = true, Data = value };
        }

        /// <summary>
        /// 要素が読み取り専用かチェック
        /// </summary>
        public OperationResult<bool> IsReadOnly(AutomationElement element)
        {
            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return new OperationResult<bool> { Success = false, Error = "ValuePattern not supported" };

            // Let exceptions flow naturally - no try-catch
            var isReadOnly = valuePattern.Current.IsReadOnly;
            return new OperationResult<bool> { Success = true, Data = isReadOnly };
        }

        /// <summary>
        /// 要素IDで要素を検索して値を設定
        /// </summary>
        public OperationResult SetElementValue(string elementId, string value, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            return SetValue(element, value);
        }

        /// <summary>
        /// 要素IDで要素を検索して値を取得
        /// </summary>
        public OperationResult GetElementValue(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            var result = GetValue(element);
            return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
        }

        /// <summary>
        /// Get value - interface compatibility method
        /// </summary>
        public OperationResult<string> GetValueResult(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult<string> { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            return GetValue(element);
        }

        /// <summary>
        /// Set value - interface compatibility method
        /// </summary>
        public OperationResult SetValueResult(string elementId, string value, string windowTitle = "", int processId = 0)
        {
            return SetElementValue(elementId, value, windowTitle, processId);
        }

        /// <summary>
        /// Check if element is read-only - interface compatibility method
        /// </summary>
        public OperationResult<bool> IsReadOnlyResult(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult<bool> { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            return IsReadOnly(element);
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