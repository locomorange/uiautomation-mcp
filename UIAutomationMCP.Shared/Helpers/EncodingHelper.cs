using System.Diagnostics;
using System.Text;

namespace UIAutomationMCP.Shared.Helpers
{
    /// <summary>
    /// エンコーディング設定を統一管理するヘルパークラス
    /// Microsoft UI Automation ドキュメントによると、UI要素の文字列プロパティは
    /// OS言語に応じてローカライズされるため、UTF-8エンコーディングを使用して
    /// 国際化対応を行う
    /// </summary>
    public static class EncodingHelper
    {
        
        /// <summary>
        /// UI Automationプロパティ文字列を安全に処理
        /// Microsoft UI Automationドキュメントによると、Name、LocalizedControlType、HelpText等の
        /// プロパティはOS言語でローカライズされるため、多言語対応処理を行う
        /// 対応言語：日本語、中国語、韓国語、アラビア語、ロシア語、ヨーロッパ言語等
        /// </summary>
        /// <param name="text">UI Automationプロパティの文字列</param>
        /// <returns>安全に処理された文字列</returns>
        public static string ProcessUIAutomationProperty(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;
                
            try
            {
                // UTF-8エンコーディング確保
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // 極めて稀なケース: 文字列処理自体が失敗した場合
                return string.Empty;
            }
        }

    }
}