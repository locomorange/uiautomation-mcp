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
    }
}