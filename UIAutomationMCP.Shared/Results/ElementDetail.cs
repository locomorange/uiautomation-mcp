using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 詳細なUI要素情報クラス（廃止予定）
    /// 新しい統合されたElementInfo構造体に置き換えられます
    /// </summary>
    [Obsolete("Use ElementInfo with Details and Hierarchy properties instead")]
    public class ElementDetail
    {
        /// <summary>
        /// 要素の表示名
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// UI Automation要素の一意識別子
        /// </summary>
        [JsonPropertyName("automationId")]
        public string AutomationId { get; set; } = string.Empty;

        /// <summary>
        /// コントロールタイプ（英語）
        /// </summary>
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        /// <summary>
        /// キーボードフォーカスを持っているかどうか
        /// </summary>
        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }

        /// <summary>
        /// キーボードフォーカス可能かどうか
        /// </summary>
        [JsonPropertyName("isKeyboardFocusable")]
        public bool IsKeyboardFocusable { get; set; }

        /// <summary>
        /// パスワードフィールドかどうか
        /// </summary>
        [JsonPropertyName("isPassword")]
        public bool IsPassword { get; set; }

        /// <summary>
        /// 画面外にあるかどうか
        /// </summary>
        [JsonPropertyName("isOffscreen")]
        public bool IsOffscreen { get; set; }

        /// <summary>
        /// ローカライズされたコントロールタイプ
        /// </summary>
        [JsonPropertyName("localizedControlType")]
        public string? LocalizedControlType { get; set; }

        /// <summary>
        /// フレームワークID
        /// </summary>
        [JsonPropertyName("frameworkId")]
        public string? FrameworkId { get; set; }

        // === 階層情報（オプション） ===

        /// <summary>
        /// 親要素の基本情報
        /// </summary>
        [JsonPropertyName("parent")]
        public BasicElementInfo? Parent { get; set; }

        /// <summary>
        /// 子要素の基本情報配列
        /// </summary>
        [JsonPropertyName("children")]
        public BasicElementInfo[]? Children { get; set; }
    }
}