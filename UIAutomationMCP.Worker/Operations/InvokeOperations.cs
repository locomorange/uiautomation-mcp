using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// InvokePattern操作の最小粒度API
    /// </summary>
    public class InvokeOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public InvokeOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
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
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            // Let exceptions flow naturally - no try-catch
            return Invoke(element);
        }
    }
}
