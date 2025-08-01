using System.Text.Json.Serialization;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models;
using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// 要素ツリー構造の結果
    /// 新しいElementInfoベースのTreeNodeを使用
    /// </summary>
    [MessagePackObject]
    public class ElementTreeResult : BaseOperationResult
    {
        /// <summary>
        /// ルート要素のAutomationId
        /// </summary>
        [Key(6)]
        [JsonPropertyName("rootAutomationId")]
        public string? RootAutomationId { get; set; }

        /// <summary>
        /// ルート要素のName
        /// </summary>
        [Key(7)]
        [JsonPropertyName("rootName")]
        public string? RootName { get; set; }

        /// <summary>
        /// ルートツリーノード
        /// </summary>
        [Key(8)]
        [JsonPropertyName("rootNode")]
        public TreeNode? RootNode { get; set; }

        /// <summary>
        /// ツリー内の総要素数
        /// </summary>
        [Key(9)]
        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        /// <summary>
        /// 最大階層深度
        /// </summary>
        [Key(10)]
        [JsonPropertyName("maxDepth")]
        public int MaxDepth { get; set; }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        [Key(11)]
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// ウィンドウハンドル (HWND)
        /// </summary>
        [Key(12)]
        [JsonPropertyName("windowHandle")]
        public long? WindowHandle { get; set; }

        /// <summary>
        /// ツリー構築にかかった時間
        /// </summary>
        [Key(13)]
        [JsonPropertyName("buildDuration")]
        public TimeSpan BuildDuration { get; set; }

        /// <summary>
        /// 不可視要素を含むかどうか
        /// </summary>
        [Key(14)]
        [JsonPropertyName("includeInvisible")]
        public bool IncludeInvisible { get; set; }

        /// <summary>
        /// 画面外要素を含むかどうか
        /// </summary>
        [Key(15)]
        [JsonPropertyName("includeOffscreen")]
        public bool IncludeOffscreen { get; set; }

        /// <summary>
        /// ツリースコープ
        /// </summary>
        [Key(16)]
        [JsonPropertyName("treeScope")]
        public string? TreeScope { get; set; }
    }

    /// <summary>
    /// ElementInfoベースのツリーノード
    /// ElementInfoを継承してツリー特有のプロパティを追加
    /// </summary>
    [MessagePackObject]
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
            BoundingRectangle = elementInfo.BoundingRectangle;
            IsEnabled = elementInfo.IsEnabled;
            IsVisible = elementInfo.IsVisible;
            IsOffscreen = elementInfo.IsOffscreen;
            SupportedPatterns = elementInfo.SupportedPatterns;
            Details = elementInfo.Details;
        }
        /// <summary>
        /// 子ノードのリスト
        /// </summary>
        [Key(14)]
        [JsonPropertyName("children")]
        public List<TreeNode> Children { get; set; } = new();

        /// <summary>
        /// ツリー内での階層深度
        /// </summary>
        [Key(15)]
        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        /// <summary>
        /// 子要素を持っているかどうか
        /// </summary>
        [Key(16)]
        [JsonPropertyName("hasChildren")]
        public bool HasChildren { get; set; }

        /// <summary>
        /// 親要素のAutomationId
        /// </summary>
        [Key(17)]
        [JsonPropertyName("parentAutomationId")]
        public string? ParentAutomationId { get; set; }
    }
}