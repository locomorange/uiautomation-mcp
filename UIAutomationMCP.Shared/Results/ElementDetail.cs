using System.Text.Json.Serialization;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 詳細なUI要素情報クラス
    /// 既存のElementInfoを継承し、階層情報を追加
    /// GetElementDetailsツールで使用される包括的な情報を提供
    /// 既存のElementInfoには既にすべてのパターン詳細が含まれている
    /// </summary>
    public class ElementDetail : ElementInfo
    {
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