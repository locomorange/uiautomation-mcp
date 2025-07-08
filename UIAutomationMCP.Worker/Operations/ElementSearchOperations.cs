using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Worker.Core;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// 要素検索操作の最小粒度API
    /// </summary>
    public class ElementSearchOperations
    {
        private readonly ILogger<ElementSearchOperations> _logger;

        public ElementSearchOperations(ILogger<ElementSearchOperations> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 単一要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElement(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition)
        {
            try
            {
                var element = searchRoot.FindElementSafe(scope, condition);
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = element
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element");
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 複数要素を検索
        /// </summary>
        public OperationResult<AutomationElementCollection> FindElements(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition)
        {
            try
            {
                var elements = searchRoot.FindElementsSafe(scope, condition);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = true,
                    Data = elements
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements");
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// AutomationIdで要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElementById(
            AutomationElement searchRoot,
            string automationId)
        {
            try
            {
                var condition = ConditionBuilder.ByAutomationId(automationId);
                var element = searchRoot.FindElementSafe(TreeScope.Descendants, condition);
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = element
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by ID: {AutomationId}", automationId);
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 名前で要素を検索
        /// </summary>
        public OperationResult<AutomationElement> FindElementByName(
            AutomationElement searchRoot,
            string name)
        {
            try
            {
                var condition = ConditionBuilder.ByName(name);
                var element = searchRoot.FindElementSafe(TreeScope.Descendants, condition);
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = element
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by name: {Name}", name);
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// コントロールタイプで要素を検索
        /// </summary>
        public OperationResult<AutomationElementCollection> FindElementsByControlType(
            AutomationElement searchRoot,
            ControlType controlType)
        {
            try
            {
                var condition = ConditionBuilder.ByControlType(controlType);
                var elements = searchRoot.FindElementsSafe(TreeScope.Descendants, condition);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = true,
                    Data = elements
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find elements by control type: {ControlType}", controlType.LocalizedControlType);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// ルート要素を取得
        /// </summary>
        public OperationResult<AutomationElement> GetRootElement()
        {
            try
            {
                var rootElement = AutomationElement.RootElement;
                return new OperationResult<AutomationElement>
                {
                    Success = true,
                    Data = rootElement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get root element");
                return new OperationResult<AutomationElement>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// デスクトップのウィンドウを取得
        /// </summary>
        public OperationResult<AutomationElementCollection> GetDesktopWindows()
        {
            try
            {
                var rootElement = AutomationElement.RootElement;
                var condition = ConditionBuilder.Windows();
                var windows = rootElement.FindElementsSafe(TreeScope.Children, condition);
                return new OperationResult<AutomationElementCollection>
                {
                    Success = true,
                    Data = windows
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get desktop windows");
                return new OperationResult<AutomationElementCollection>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}