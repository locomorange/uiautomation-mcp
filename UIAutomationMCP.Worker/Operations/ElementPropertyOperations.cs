using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Worker.Core;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// 要素プロパティ操作の最小粒度API
    /// </summary>
    public class ElementPropertyOperations
    {
        private readonly ILogger<ElementPropertyOperations> _logger;

        public ElementPropertyOperations(ILogger<ElementPropertyOperations> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 要素の名前を取得
        /// </summary>
        public OperationResult<string> GetName(AutomationElement element)
        {
            try
            {
                var name = element.GetPropertyValue<string>(AutomationElement.NameProperty);
                return new OperationResult<string>
                {
                    Success = true,
                    Data = name ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get element name");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素のAutomationIdを取得
        /// </summary>
        public OperationResult<string> GetAutomationId(AutomationElement element)
        {
            try
            {
                var automationId = element.GetPropertyValue<string>(AutomationElement.AutomationIdProperty);
                return new OperationResult<string>
                {
                    Success = true,
                    Data = automationId ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get automation ID");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素のコントロールタイプを取得
        /// </summary>
        public OperationResult<ControlType> GetControlType(AutomationElement element)
        {
            try
            {
                var controlType = element.GetPropertyValue<ControlType>(AutomationElement.ControlTypeProperty);
                return new OperationResult<ControlType>
                {
                    Success = true,
                    Data = controlType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get control type");
                return new OperationResult<ControlType>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素のクラス名を取得
        /// </summary>
        public OperationResult<string> GetClassName(AutomationElement element)
        {
            try
            {
                var className = element.GetPropertyValue<string>(AutomationElement.ClassNameProperty);
                return new OperationResult<string>
                {
                    Success = true,
                    Data = className ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get class name");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素のBounding Rectangleを取得
        /// </summary>
        public OperationResult<System.Windows.Rect> GetBoundingRectangle(AutomationElement element)
        {
            try
            {
                var boundingRect = element.GetPropertyValue<System.Windows.Rect>(AutomationElement.BoundingRectangleProperty);
                return new OperationResult<System.Windows.Rect>
                {
                    Success = true,
                    Data = boundingRect
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bounding rectangle");
                return new OperationResult<System.Windows.Rect>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素が有効かどうかを取得
        /// </summary>
        public OperationResult<bool> IsEnabled(AutomationElement element)
        {
            try
            {
                var isEnabled = element.GetPropertyValue<bool>(AutomationElement.IsEnabledProperty);
                return new OperationResult<bool>
                {
                    Success = true,
                    Data = isEnabled
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if element is enabled");
                return new OperationResult<bool>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素が表示されているかを取得
        /// </summary>
        public OperationResult<bool> IsVisible(AutomationElement element)
        {
            try
            {
                var isOffscreen = element.GetPropertyValue<bool>(AutomationElement.IsOffscreenProperty);
                return new OperationResult<bool>
                {
                    Success = true,
                    Data = !isOffscreen // IsOffscreenの逆
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if element is visible");
                return new OperationResult<bool>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素のプロセスIDを取得
        /// </summary>
        public OperationResult<int> GetProcessId(AutomationElement element)
        {
            try
            {
                var processId = element.GetPropertyValue<int>(AutomationElement.ProcessIdProperty);
                return new OperationResult<int>
                {
                    Success = true,
                    Data = processId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get process ID");
                return new OperationResult<int>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素のヘルプテキストを取得
        /// </summary>
        public OperationResult<string> GetHelpText(AutomationElement element)
        {
            try
            {
                var helpText = element.GetPropertyValue<string>(AutomationElement.HelpTextProperty);
                return new OperationResult<string>
                {
                    Success = true,
                    Data = helpText ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get help text");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 要素の任意のプロパティを取得
        /// </summary>
        public OperationResult<T> GetProperty<T>(AutomationElement element, AutomationProperty property)
        {
            try
            {
                var value = element.GetPropertyValue<T>(property);
                return new OperationResult<T>
                {
                    Success = true,
                    Data = value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get property: {PropertyName}", property.ProgrammaticName);
                return new OperationResult<T>
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}