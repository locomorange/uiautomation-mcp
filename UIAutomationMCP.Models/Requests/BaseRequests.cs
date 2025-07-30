using System.Text.Json.Serialization;
using UIAutomationMCP.Core.Serialization;

namespace UIAutomationMCP.Models.Requests
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
    /// 要素を特定するための共通パラメータ（改善版）
    /// AutomationIdとNameを明確に分離
    /// </summary>
    public abstract class ElementTargetRequest : TypedWorkerRequest
    {
        /// <summary>
        /// UI Automation要素のAutomationIdプロパティ（推奨）
        /// 安定したプログラム識別子
        /// </summary>
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }

        /// <summary>
        /// UI Automation要素のNameプロパティ（フォールバック）
        /// 表示名による識別
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// UI Automation要素のControlType（パフォーマンス向上とより確実な特定）
        /// </summary>
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }

        /// <summary>
        /// 要素検索のコンテキストとなるウィンドウタイトル
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }


        /// <summary>
        /// 要素検索時に使用する必須パターン（検索精度向上用、省略可）
        /// </summary>
        [JsonPropertyName("requiredPattern")]
        public string? RequiredPattern { get; set; }

        /// <summary>
        /// ネイティブウィンドウハンドル（HWND）による直接要素指定
        /// 指定された場合、他の検索条件より優先される
        /// </summary>
        [JsonPropertyName("windowHandle")]
        public long? WindowHandle { get; set; }

    }

}