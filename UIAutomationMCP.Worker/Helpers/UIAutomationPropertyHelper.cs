using System.Windows.Automation;
using UIAutomationMCP.Shared.Helpers;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// UI Automationプロパティの取得をラップし、適切なエンコーディング処理を行うヘルパークラス
    /// Microsoft UI Automationドキュメントに基づき、ローカライズされたプロパティを適切に処理する
    /// </summary>
    public static class UIAutomationPropertyHelper
    {
        /// <summary>
        /// 要素の名前を取得（ローカライズされたプロパティ）
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理された名前</returns>
        public static string GetName(AutomationElement element)
        {
            try
            {
                return EncodingHelper.ProcessUIAutomationProperty(element.Current.Name);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のクラス名を取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理されたクラス名</returns>
        public static string GetClassName(AutomationElement element)
        {
            try
            {
                return EncodingHelper.ProcessUIAutomationProperty(element.Current.ClassName);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のローカライズされたコントロールタイプを取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理されたローカライズされたコントロールタイプ</returns>
        public static string GetLocalizedControlType(AutomationElement element)
        {
            try
            {
                return EncodingHelper.ProcessUIAutomationProperty(element.Current.LocalizedControlType);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のヘルプテキストを取得（ローカライズされたプロパティ）
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理されたヘルプテキスト</returns>
        public static string GetHelpText(AutomationElement element)
        {
            try
            {
                return EncodingHelper.ProcessUIAutomationProperty(element.Current.HelpText);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のアクセスキーを取得（ローカライズされたプロパティ）
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理されたアクセスキー</returns>
        public static string GetAccessKey(AutomationElement element)
        {
            try
            {
                return EncodingHelper.ProcessUIAutomationProperty(element.Current.AccessKey);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のアクセラレーターキーを取得（ローカライズされたプロパティ）
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理されたアクセラレーターキー</returns>
        public static string GetAcceleratorKey(AutomationElement element)
        {
            try
            {
                return EncodingHelper.ProcessUIAutomationProperty(element.Current.AcceleratorKey);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のAutomationIdを取得（非ローカライズプロパティ）
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>AutomationId</returns>
        public static string GetAutomationId(AutomationElement element)
        {
            try
            {
                return element.Current.AutomationId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素の値を取得（ValuePatternから）
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>UTF-8エンコーディングで処理された値</returns>
        public static string GetValue(AutomationElement element)
        {
            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
                {
                    var valuePattern = pattern as ValuePattern;
                    return EncodingHelper.ProcessUIAutomationProperty(valuePattern?.Current.Value);
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 要素のプロセスIDを取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>プロセスID</returns>
        public static int GetProcessId(AutomationElement element)
        {
            try
            {
                return element.Current.ProcessId;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 要素の境界矩形を取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>境界矩形</returns>
        public static System.Windows.Rect GetBoundingRectangle(AutomationElement element)
        {
            try
            {
                return element.Current.BoundingRectangle;
            }
            catch
            {
                return System.Windows.Rect.Empty;
            }
        }

        /// <summary>
        /// 要素の有効状態を取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>有効状態</returns>
        public static bool IsEnabled(AutomationElement element)
        {
            try
            {
                return element.Current.IsEnabled;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 要素のオフスクリーン状態を取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>オフスクリーン状態</returns>
        public static bool IsOffscreen(AutomationElement element)
        {
            try
            {
                return element.Current.IsOffscreen;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 要素のネイティブウィンドウハンドルを取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>ネイティブウィンドウハンドル</returns>
        public static int GetNativeWindowHandle(AutomationElement element)
        {
            try
            {
                return element.Current.NativeWindowHandle;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 要素のコントロールタイプを取得
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>コントロールタイプ</returns>
        public static ControlType GetControlType(AutomationElement element)
        {
            try
            {
                return element.Current.ControlType;
            }
            catch
            {
                return ControlType.Custom;
            }
        }
    }
}