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
        /// 要素検索のコンテキストとなるプロセスID
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// Worker内でのUI Automation操作タイムアウト秒数（デフォルト: 10秒）
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;

    }

}