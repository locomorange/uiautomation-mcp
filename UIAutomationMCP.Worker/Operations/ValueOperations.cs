using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Worker.Core;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// ValuePattern操作の最小粒度API
    /// </summary>
    public class ValueOperations
    {
        private readonly ILogger<ValueOperations> _logger;

        public ValueOperations(ILogger<ValueOperations> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 要素の値を設定
        /// </summary>
        public OperationResult SetValue(AutomationElement element, string value)
        {
            try
            {
                var valuePattern = element.GetPattern<ValuePattern>(ValuePattern.Pattern);
                if (valuePattern == null)
                {
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support ValuePattern" 
                    };
                }

                valuePattern.SetValue(value);
                return new OperationResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set value");
                return new OperationResult 
                { 
                    Success = false, 
                    Error = ex.Message 
                };
            }
        }

        /// <summary>
        /// 要素の値を取得
        /// </summary>
        public OperationResult<string> GetValue(AutomationElement element)
        {
            try
            {
                var valuePattern = element.GetPattern<ValuePattern>(ValuePattern.Pattern);
                if (valuePattern == null)
                {
                    return new OperationResult<string> 
                    { 
                        Success = false, 
                        Error = "Element does not support ValuePattern" 
                    };
                }

                var value = valuePattern.Current.Value;
                return new OperationResult<string> 
                { 
                    Success = true, 
                    Data = value 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get value");
                return new OperationResult<string> 
                { 
                    Success = false, 
                    Error = ex.Message 
                };
            }
        }

        /// <summary>
        /// 要素が読み取り専用かチェック
        /// </summary>
        public OperationResult<bool> IsReadOnly(AutomationElement element)
        {
            try
            {
                var valuePattern = element.GetPattern<ValuePattern>(ValuePattern.Pattern);
                if (valuePattern == null)
                {
                    return new OperationResult<bool> 
                    { 
                        Success = false, 
                        Error = "Element does not support ValuePattern" 
                    };
                }

                var isReadOnly = valuePattern.Current.IsReadOnly;
                return new OperationResult<bool> 
                { 
                    Success = true, 
                    Data = isReadOnly 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check read-only status");
                return new OperationResult<bool> 
                { 
                    Success = false, 
                    Error = ex.Message 
                };
            }
        }

        /// <summary>
        /// 要素IDで要素を検索して値を設定
        /// </summary>
        public OperationResult SetElementValue(string elementId, string value, string windowTitle = "", int processId = 0)
        {
            try
            {
                var element = FindElementById(elementId, windowTitle, processId);
                if (element == null)
                {
                    return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };
                }

                return SetValue(element, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set element value: {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// 要素IDで要素を検索して値を取得
        /// </summary>
        public OperationResult GetElementValue(string elementId, string windowTitle = "", int processId = 0)
        {
            try
            {
                var element = FindElementById(elementId, windowTitle, processId);
                if (element == null)
                {
                    return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = GetValue(element);
                return new OperationResult { Success = result.Success, Data = result.Data, Error = result.Error };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get element value: {ElementId}", elementId);
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