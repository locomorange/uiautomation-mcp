using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Worker.Core;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// InvokePattern操作の最小粒度API
    /// </summary>
    public class InvokeOperations
    {
        private readonly ILogger<InvokeOperations> _logger;

        public InvokeOperations(ILogger<InvokeOperations> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 要素をInvokeする
        /// </summary>
        public OperationResult Invoke(AutomationElement element)
        {
            try
            {
                var invokePattern = element.GetPattern<InvokePattern>(InvokePattern.Pattern);
                if (invokePattern == null)
                {
                    return new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support InvokePattern" 
                    };
                }

                invokePattern.Invoke();
                return new OperationResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invoke element");
                return new OperationResult 
                { 
                    Success = false, 
                    Error = ex.Message 
                };
            }
        }

        /// <summary>
        /// 要素がInvokePatternをサポートしているかチェック
        /// </summary>
        public OperationResult<bool> SupportsInvoke(AutomationElement element)
        {
            try
            {
                var invokePattern = element.GetPattern<InvokePattern>(InvokePattern.Pattern);
                return new OperationResult<bool> 
                { 
                    Success = true, 
                    Data = invokePattern != null 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check InvokePattern support");
                return new OperationResult<bool> 
                { 
                    Success = false, 
                    Error = ex.Message 
                };
            }
        }

        /// <summary>
        /// 要素IDで要素を検索してInvokeする
        /// </summary>
        public OperationResult InvokeElement(string elementId, string windowTitle = "", int processId = 0)
        {
            try
            {
                var element = FindElementById(elementId, windowTitle, processId);
                if (element == null)
                {
                    return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };
                }

                return Invoke(element);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invoke element: {ElementId}", elementId);
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