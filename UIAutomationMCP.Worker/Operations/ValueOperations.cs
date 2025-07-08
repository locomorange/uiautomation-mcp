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
    }
}