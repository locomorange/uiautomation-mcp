using System.Text.Json.Serialization;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 要素ツリー構造の結果
    /// 新しいElementInfoベースのTreeNodeを使用
    /// </summary>
    public class ElementTreeResult : BaseOperationResult
    {
        /// <summary>
        /// ルート要素のAutomationId
        /// </summary>
        [JsonPropertyName("rootAutomationId")]
        public string? RootAutomationId { get; set; }

        /// <summary>
        /// ルート要素のName
        /// </summary>
        [JsonPropertyName("rootName")]
        public string? RootName { get; set; }

        /// <summary>
        /// ルートツリーノード
        /// </summary>
        [JsonPropertyName("rootNode")]
        public TreeNode? RootNode { get; set; }

        /// <summary>
        /// ツリー内の総要素数
        /// </summary>
        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        /// <summary>
        /// 最大階層深度
        /// </summary>
        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// プロセスID
        /// </summary>
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        /// <summary>
        /// ツリー構築にかかった時間
        /// </summary>
        [JsonPropertyName("buildDuration")]
        public TimeSpan BuildDuration { get; set; }

        /// <summary>
        /// 不可視要素を含むかどうか
        /// </summary>
        [JsonPropertyName("includeInvisible")]
        public bool IncludeInvisible { get; set; }

        /// <summary>
        /// 画面外要素を含むかどうか
        /// </summary>
        [JsonPropertyName("includeOffscreen")]
        public bool IncludeOffscreen { get; set; }

        /// <summary>
        /// ツリースコープ
        /// </summary>
        [JsonPropertyName("treeScope")]
        public string? TreeScope { get; set; }
    }

    /// <summary>
    /// ElementInfoベースのツリーノード
    /// ElementInfoを継承してツリー特有のプロパティを追加
    /// </summary>
    public class TreeNode : ElementInfo
    {
        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public TreeNode() { }
        
        /// <summary>
        /// ElementInfoからTreeNodeを作成するコンストラクタ
        /// </summary>
        public TreeNode(ElementInfo elementInfo)
        {
            // ElementInfoの全プロパティをコピー
            AutomationId = elementInfo.AutomationId;
            Name = elementInfo.Name;
            ControlType = elementInfo.ControlType;
            LocalizedControlType = elementInfo.LocalizedControlType;
            ClassName = elementInfo.ClassName;
            ProcessId = elementInfo.ProcessId;
            MainProcessId = elementInfo.MainProcessId;
            BoundingRectangle = elementInfo.BoundingRectangle;
            IsEnabled = elementInfo.IsEnabled;
            IsVisible = elementInfo.IsVisible;
            IsOffscreen = elementInfo.IsOffscreen;
            FrameworkId = elementInfo.FrameworkId;
            SupportedPatterns = elementInfo.SupportedPatterns;
            Details = elementInfo.Details;
        }
        /// <summary>
        /// 子ノードのリスト
        /// </summary>
        [JsonPropertyName("children")]
        public List<TreeNode> Children { get; set; } = new();

        /// <summary>
        /// ツリー内での階層深度
        /// </summary>
        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        /// <summary>
        /// ノードが展開されているかどうか
        /// </summary>
        [JsonPropertyName("isExpanded")]
        public bool IsExpanded { get; set; }

        /// <summary>
        /// 子要素を持っているかどうか
        /// </summary>
        [JsonPropertyName("hasChildren")]
        public bool HasChildren { get; set; }

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
    }
}