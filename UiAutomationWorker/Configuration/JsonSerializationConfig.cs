using System.Text.Json;

namespace UiAutomationWorker.Configuration
{
    /// <summary>
    /// JSON シリアライゼーションの設定を管理するクラス
    /// </summary>
    public static class JsonSerializationConfig
    {
        /// <summary>
        /// 入力用のJSON シリアライゼーション オプションを取得します
        /// </summary>
        public static JsonSerializerOptions GetInputOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
        }

        /// <summary>
        /// 出力用のJSON シリアライゼーション オプションを取得します
        /// </summary>
        public static JsonSerializerOptions GetOutputOptions()
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
