using System.Windows.Automation;

namespace UIAutomationMCP.Worker.Core
{
    /// <summary>
    /// AutomationElement用の拡張メソッド
    /// </summary>
    public static class AutomationElementExtensions
    {
        /// <summary>
        /// 要素からパターンを安全に取得
        /// </summary>
        public static TPattern? GetPattern<TPattern>(this AutomationElement element, AutomationPattern pattern) 
            where TPattern : class
        {
            try
            {
                if (element.TryGetCurrentPattern(pattern, out var patternObject))
                {
                    return patternObject as TPattern;
                }
                return null;
            }
            catch (ElementNotAvailableException)
            {
                return null;
            }
        }

        /// <summary>
        /// 要素のプロパティを安全に取得
        /// </summary>
        public static T? GetPropertyValue<T>(this AutomationElement element, AutomationProperty property)
        {
            try
            {
                var value = element.GetCurrentPropertyValue(property);
                return value is T typedValue ? typedValue : default;
            }
            catch (ElementNotAvailableException)
            {
                return default;
            }
        }

        /// <summary>
        /// 要素を安全に検索（単一）
        /// </summary>
        public static AutomationElement? FindElementSafe(this AutomationElement element, TreeScope scope, Condition condition)
        {
            try
            {
                return element.FindFirst(scope, condition);
            }
            catch (ElementNotAvailableException)
            {
                return null;
            }
        }

        /// <summary>
        /// 要素を安全に検索（複数）
        /// </summary>
        public static AutomationElementCollection FindElementsSafe(this AutomationElement element, TreeScope scope, Condition condition)
        {
            try
            {
                return element.FindAll(scope, condition);
            }
            catch (ElementNotAvailableException)
            {
                // 空のコレクションを返すため、存在しないプロセスIDで検索
                return element.FindAll(TreeScope.Element, new PropertyCondition(AutomationElement.ProcessIdProperty, -1));
            }
        }
    }
}