using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 詳細なUI要素情報クラス
    /// ElementInfoを継承し、全パターン詳細情報と階層情報を追加
    /// GetElementDetailsツールで使用される包括的な情報を提供
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
        /// ヘルプテキスト
        /// </summary>
        [JsonPropertyName("helpText")]
        public string? HelpText { get; set; }

        // === パターン詳細情報（既存の型安全クラス使用） ===

        /// <summary>
        /// Range Value Pattern詳細情報
        /// </summary>
        [JsonPropertyName("range")]
        public RangeInfo? Range { get; set; }

        /// <summary>
        /// Toggle Pattern詳細情報
        /// </summary>
        [JsonPropertyName("toggle")]
        public ToggleInfo? Toggle { get; set; }

        /// <summary>
        /// Value Pattern詳細情報
        /// </summary>
        [JsonPropertyName("valueInfo")]
        public ValueInfo? ValueInfo { get; set; }

        /// <summary>
        /// Selection Pattern詳細情報
        /// </summary>
        [JsonPropertyName("selection")]
        public SelectionInfo? Selection { get; set; }

        /// <summary>
        /// Grid Pattern詳細情報
        /// </summary>
        [JsonPropertyName("grid")]
        public GridInfo? Grid { get; set; }

        /// <summary>
        /// Scroll Pattern詳細情報
        /// </summary>
        [JsonPropertyName("scroll")]
        public ScrollInfo? Scroll { get; set; }

        /// <summary>
        /// Text Pattern詳細情報
        /// </summary>
        [JsonPropertyName("text")]
        public TextInfo? Text { get; set; }

        /// <summary>
        /// Transform Pattern詳細情報
        /// </summary>
        [JsonPropertyName("transform")]
        public TransformInfo? Transform { get; set; }

        /// <summary>
        /// Invoke Pattern詳細情報
        /// </summary>
        [JsonPropertyName("invoke")]
        public InvokeInfo? Invoke { get; set; }

        /// <summary>
        /// Scroll Item Pattern詳細情報
        /// </summary>
        [JsonPropertyName("scrollItem")]
        public ScrollItemInfo? ScrollItem { get; set; }

        /// <summary>
        /// Expand Collapse Pattern詳細情報
        /// </summary>
        [JsonPropertyName("expandCollapse")]
        public ExpandCollapseInfo? ExpandCollapse { get; set; }

        /// <summary>
        /// Window Pattern詳細情報
        /// </summary>
        [JsonPropertyName("window")]
        public WindowPatternInfo? Window { get; set; }

        /// <summary>
        /// Grid Item Pattern詳細情報
        /// </summary>
        [JsonPropertyName("gridItem")]
        public GridItemInfo? GridItem { get; set; }

        /// <summary>
        /// Table Pattern詳細情報
        /// </summary>
        [JsonPropertyName("table")]
        public TableInfo? Table { get; set; }

        /// <summary>
        /// Table Item Pattern詳細情報
        /// </summary>
        [JsonPropertyName("tableItem")]
        public TableItemInfo? TableItem { get; set; }

        /// <summary>
        /// Dock Pattern詳細情報
        /// </summary>
        [JsonPropertyName("dock")]
        public DockInfo? Dock { get; set; }

        /// <summary>
        /// Multiple View Pattern詳細情報
        /// </summary>
        [JsonPropertyName("multipleView")]
        public MultipleViewInfo? MultipleView { get; set; }

        /// <summary>
        /// Virtualized Item Pattern詳細情報
        /// </summary>
        [JsonPropertyName("virtualizedItem")]
        public VirtualizedItemInfo? VirtualizedItem { get; set; }

        /// <summary>
        /// Item Container Pattern詳細情報
        /// </summary>
        [JsonPropertyName("itemContainer")]
        public ItemContainerInfo? ItemContainer { get; set; }

        /// <summary>
        /// Synchronized Input Pattern詳細情報
        /// </summary>
        [JsonPropertyName("synchronizedInput")]
        public SynchronizedInputInfo? SynchronizedInput { get; set; }

        /// <summary>
        /// アクセシビリティ関連詳細情報
        /// </summary>
        [JsonPropertyName("accessibility")]
        public AccessibilityInfo? Accessibility { get; set; }

        // === 階層情報（オプション） ===

        /// <summary>
        /// 親要素の基本情報
        /// </summary>
        [JsonPropertyName("parent")]
        public ElementInfo? Parent { get; set; }

        /// <summary>
        /// 子要素の基本情報配列
        /// </summary>
        [JsonPropertyName("children")]
        public ElementInfo[]? Children { get; set; }
    }
}