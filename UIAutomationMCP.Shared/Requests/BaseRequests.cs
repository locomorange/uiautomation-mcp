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

        // === 後方互換性のための非推奨プロパティ ===
        
        /// <summary>
        /// 非推奨: AutomationIdまたはNameを使用してください
        /// </summary>
        [JsonPropertyName("elementId")]
        [Obsolete("Use AutomationId or Name instead. ElementId will be removed in future versions.")]
        public string ElementId 
        { 
            get => AutomationId ?? Name ?? "";
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // 既存のコードとの互換性のため、elementIdが設定されたらAutomationIdとして扱う
                    if (string.IsNullOrEmpty(AutomationId))
                        AutomationId = value;
                }
            }
        }
    }

    /// <summary>
    /// 旧形式のElementTargetRequest（後方互換性維持）
    /// </summary>
    [Obsolete("Use the new ElementTargetRequest with AutomationId/Name properties")]
    public abstract class LegacyElementTargetRequest : TypedWorkerRequest
    {
        [JsonPropertyName("elementId")]
        public string ElementId { get; set; } = "";

        [JsonPropertyName("windowTitle")]
        public string WindowTitle { get; set; } = "";

        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 10;
    }
}