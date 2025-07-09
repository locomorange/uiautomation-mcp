using System.Windows.Automation;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// 要素プロパティ操作の最小粒度API
    /// </summary>
    public class ElementPropertyOperations
    {
        public ElementPropertyOperations()
        {
        }

        /// <summary>
        /// 要素の名前を取得
        /// </summary>
        public OperationResult<string> GetName(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var name = element.Current.Name;
            return new OperationResult<string>
            {
                Success = true,
                Data = name ?? string.Empty
            };
        }

        /// <summary>
        /// 要素のAutomationIdを取得
        /// </summary>
        public OperationResult<string> GetAutomationId(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var automationId = element.Current.AutomationId;
            return new OperationResult<string>
            {
                Success = true,
                Data = automationId ?? string.Empty
            };
        }

        /// <summary>
        /// 要素のコントロールタイプを取得
        /// </summary>
        public OperationResult<ControlType> GetControlType(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var controlType = element.Current.ControlType;
            return new OperationResult<ControlType>
            {
                Success = true,
                Data = controlType
            };
        }

        /// <summary>
        /// 要素のクラス名を取得
        /// </summary>
        public OperationResult<string> GetClassName(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var className = element.Current.ClassName;
            return new OperationResult<string>
            {
                Success = true,
                Data = className ?? string.Empty
            };
        }

        /// <summary>
        /// 要素のBounding Rectangleを取得
        /// </summary>
        public OperationResult<System.Windows.Rect> GetBoundingRectangle(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var boundingRect = element.Current.BoundingRectangle;
            return new OperationResult<System.Windows.Rect>
            {
                Success = true,
                Data = boundingRect
            };
        }

        /// <summary>
        /// 要素が有効かどうかを取得
        /// </summary>
        public OperationResult<bool> IsEnabled(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var isEnabled = element.Current.IsEnabled;
            return new OperationResult<bool>
            {
                Success = true,
                Data = isEnabled
            };
        }

        /// <summary>
        /// 要素が表示されているかを取得
        /// </summary>
        public OperationResult<bool> IsVisible(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var isOffscreen = element.Current.IsOffscreen;
            return new OperationResult<bool>
            {
                Success = true,
                Data = !isOffscreen // IsOffscreenの逆
            };
        }

        /// <summary>
        /// 要素のプロセスIDを取得
        /// </summary>
        public OperationResult<int> GetProcessId(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var processId = element.Current.ProcessId;
            return new OperationResult<int>
            {
                Success = true,
                Data = processId
            };
        }

        /// <summary>
        /// 要素のヘルプテキストを取得
        /// </summary>
        public OperationResult<string> GetHelpText(AutomationElement element)
        {
            // Let exceptions flow naturally - no try-catch
            var helpText = element.Current.HelpText;
            return new OperationResult<string>
            {
                Success = true,
                Data = helpText ?? string.Empty
            };
        }

        /// <summary>
        /// 要素の任意のプロパティを取得
        /// </summary>
        public OperationResult<T> GetProperty<T>(AutomationElement element, AutomationProperty property)
        {
            // Let exceptions flow naturally - no try-catch
            var value = element.GetCurrentPropertyValue(property);
            return new OperationResult<T>
            {
                Success = true,
                Data = (T)value
            };
        }
    }
}
