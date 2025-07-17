using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// シリアライズ済みJSONデータを表すクラス
    /// </summary>
    public class JsonResult
    {
        /// <summary>
        /// JSON文字列
        /// </summary>
        [JsonPropertyName("json")]
        public string Json { get; set; } = "";

        /// <summary>
        /// データのサイズ（文字数）
        /// </summary>
        [JsonPropertyName("size")]
        public int Size => Json.Length;

        /// <summary>
        /// データの種類
        /// </summary>
        [JsonPropertyName("dataType")]
        public string DataType { get; set; } = "";

        /// <summary>
        /// シリアライズ時刻
        /// </summary>
        [JsonPropertyName("serializedAt")]
        public DateTime SerializedAt { get; set; } = DateTime.UtcNow;

        public JsonResult() { }

        public JsonResult(string json, string dataType = "")
        {
            Json = json;
            DataType = dataType;
        }

        /// <summary>
        /// 暗黙的な文字列変換
        /// </summary>
        public static implicit operator string(JsonResult jsonResult) => jsonResult.Json;

        /// <summary>
        /// 暗黙的なJsonResult作成
        /// </summary>
        public static implicit operator JsonResult(string json) => new(json);

        public override string ToString() => Json;
    }
}