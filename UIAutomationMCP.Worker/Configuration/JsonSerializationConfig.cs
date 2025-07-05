using System.Text.Json;

namespace UiAutomationWorker.Configuration
{
    /// <summary>
    /// JSON シリアライゼーションの設定を管理するクラス
    /// </summary>
    public static class JsonSerializationConfig
    {
        /// <summary>
        /// 共通のJSON シリアライゼーション オプションを取得します
        /// </summary>
        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
        }
    }
}
