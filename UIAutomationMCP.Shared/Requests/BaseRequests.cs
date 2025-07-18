using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    /// <summary>
    /// 型安全なWorkerRequestの基底クラス
    /// </summary>
    public abstract class TypedWorkerRequest
    {
        [JsonPropertyName("operation")]
        public abstract string Operation { get; }
    }

    /// <summary>
    /// 要素を特定するための共通パラメータ
    /// </summary>
    public abstract class ElementTargetRequest : TypedWorkerRequest
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }
    }
}