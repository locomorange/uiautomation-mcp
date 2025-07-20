using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// ツリーナビゲーション結果
    /// ElementInfoベースのクラスを使用してプロパティ重複を削除
    /// </summary>
    public class TreeNavigationResult : BaseOperationResult
    {
        /// <summary>
        /// 対象要素のAutomationId
        /// </summary>
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }
        
        /// <summary>
        /// 対象要素のName
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        /// <summary>
        /// 親要素のAutomationId
        /// </summary>
        [JsonPropertyName("parentAutomationId")]
        public string? ParentAutomationId { get; set; }
        
        /// <summary>
        /// 親要素のName
        /// </summary>
        [JsonPropertyName("parentName")]
        public string? ParentName { get; set; }
        
        /// <summary>
        /// 子要素のリスト
        /// </summary>
        [JsonPropertyName("children")]
        public List<TreeElement> Children { get; set; } = new();
        
        /// <summary>
        /// 最初の子要素のAutomationId
        /// </summary>
        [JsonPropertyName("firstChildAutomationId")]
        public string? FirstChildAutomationId { get; set; }
        
        /// <summary>
        /// 最後の子要素のAutomationId
        /// </summary>
        [JsonPropertyName("lastChildAutomationId")]
        public string? LastChildAutomationId { get; set; }
        
        /// <summary>
        /// 次の兄弟要素のAutomationId
        /// </summary>
        [JsonPropertyName("nextSiblingAutomationId")]
        public string? NextSiblingAutomationId { get; set; }
        
        /// <summary>
        /// 前の兄弟要素のAutomationId
        /// </summary>
        [JsonPropertyName("previousSiblingAutomationId")]
        public string? PreviousSiblingAutomationId { get; set; }
        
        /// <summary>
        /// 子要素数
        /// </summary>
        [JsonPropertyName("childCount")]
        public int ChildCount { get; set; }
        
        /// <summary>
        /// ツリー内の階層深度
        /// </summary>
        [JsonPropertyName("depth")]
        public int Depth { get; set; }
        
        /// <summary>
        /// ツリーパス
        /// </summary>
        [JsonPropertyName("treePath")]
        public string? TreePath { get; set; }
        
        /// <summary>
        /// 親要素の基本情報（オプション）
        /// </summary>
        [JsonPropertyName("parentElement")]
        public BasicElementInfo? ParentElement { get; set; }
        
        /// <summary>
        /// 最初の子要素の基本情報（オプション）
        /// </summary>
        [JsonPropertyName("firstChildElement")]
        public BasicElementInfo? FirstChildElement { get; set; }
        
        /// <summary>
        /// 最後の子要素の基本情報（オプション）
        /// </summary>
        [JsonPropertyName("lastChildElement")]
        public BasicElementInfo? LastChildElement { get; set; }
        
        /// <summary>
        /// 次の兄弟要素の基本情報（オプション）
        /// </summary>
        [JsonPropertyName("nextSiblingElement")]
        public BasicElementInfo? NextSiblingElement { get; set; }
        
        /// <summary>
        /// 前の兄弟要素の基本情報（オプション）
        /// </summary>
        [JsonPropertyName("previousSiblingElement")]
        public BasicElementInfo? PreviousSiblingElement { get; set; }
        
        /// <summary>
        /// ナビゲーション種別
        /// </summary>
        [JsonPropertyName("navigationType")]
        public string? NavigationType { get; set; }
        
        /// <summary>
        /// 要素のリスト
        /// </summary>
        [JsonPropertyName("elements")]
        public List<TreeElement> Elements { get; set; } = new();
    }

    /// <summary>
    /// ツリー要素
    /// ElementInfoを継承してツリー特有のプロパティを追加
    /// </summary>
    public class TreeElement : ElementInfo
    {
        /// <summary>
        /// 親要素のAutomationId
        /// </summary>
        [JsonPropertyName("parentAutomationId")]
        public string? ParentAutomationId { get; set; }
        
        /// <summary>
        /// 親要素のName
        /// </summary>
        [JsonPropertyName("parentName")]
        public string? ParentName { get; set; }
        
        /// <summary>
        /// RuntimeId（UI Automation内部識別子）
        /// </summary>
        [JsonPropertyName("runtimeId")]
        public string? RuntimeId { get; set; }
        
        /// <summary>
        /// ネイティブウィンドウハンドル
        /// </summary>
        [JsonPropertyName("nativeWindowHandle")]
        public int NativeWindowHandle { get; set; }
        
        /// <summary>
        /// コントロール要素かどうか
        /// </summary>
        [JsonPropertyName("isControlElement")]
        public bool IsControlElement { get; set; }
        
        /// <summary>
        /// コンテンツ要素かどうか
        /// </summary>
        [JsonPropertyName("isContentElement")]
        public bool IsContentElement { get; set; }
        
        /// <summary>
        /// 子要素を持っているかどうか
        /// </summary>
        [JsonPropertyName("hasChildren")]
        public bool HasChildren { get; set; }
        
        /// <summary>
        /// 子要素数
        /// </summary>
        [JsonPropertyName("childCount")]
        public int ChildCount { get; set; }
        
        /// <summary>
        /// キーボードフォーカス可能かどうか
        /// </summary>
        [JsonPropertyName("isKeyboardFocusable")]
        public bool IsKeyboardFocusable { get; set; }
        
        /// <summary>
        /// キーボードフォーカスを持っているかどうか
        /// </summary>
        [JsonPropertyName("hasKeyboardFocus")]
        public bool HasKeyboardFocus { get; set; }
        
        /// <summary>
        /// パスワードフィールドかどうか
        /// </summary>
        [JsonPropertyName("isPassword")]
        public bool IsPassword { get; set; }
        
        // 子要素は再帰的に含めない（循環参照回避）
        [JsonIgnore]
        public List<TreeElement> Children { get; set; } = new();
    }
}