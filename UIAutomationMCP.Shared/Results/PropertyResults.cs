namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 要素プロパティの結果
    /// </summary>
    public class ElementPropertiesResult : BaseOperationResult
    {
        /// <summary>
        /// 基本プロパティ
        /// </summary>
        public ElementInfo BasicInfo { get; set; } = new();

        /// <summary>
        /// 追加プロパティ
        /// </summary>
        public Dictionary<string, object> ExtendedProperties { get; set; } = new();

        /// <summary>
        /// サポートされているパターン
        /// </summary>
        public List<string> SupportedPatterns { get; set; } = new();

        /// <summary>
        /// ランタイムID
        /// </summary>
        public int[]? RuntimeId { get; set; }

        /// <summary>
        /// フレームワークID
        /// </summary>
        public string? FrameworkId { get; set; }
    }

    /// <summary>
    /// グリッド情報の結果
    /// </summary>
    public class GridInfoResult
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public bool CanSelectMultiple { get; set; }
        public List<GridItemInfo> Items { get; set; } = new();
    }

    /// <summary>
    /// グリッドアイテム情報
    /// </summary>
    public class GridItemInfo
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public ElementInfo Element { get; set; } = new();
    }

    /// <summary>
    /// テーブル情報の結果
    /// </summary>
    public class TableInfoResult : GridInfoResult
    {
        public List<ElementInfo> RowHeaders { get; set; } = new();
        public List<ElementInfo> ColumnHeaders { get; set; } = new();
        public string RowOrColumnMajor { get; set; } = "";
    }

    /// <summary>
    /// 利用可能なビューの結果
    /// </summary>
    public class AvailableViewsResult
    {
        public List<ViewInfo> Views { get; set; } = new();
        public int CurrentViewId { get; set; }
        public string CurrentViewName { get; set; } = "";
    }

    /// <summary>
    /// ビュー情報
    /// </summary>
    public class ViewInfo
    {
        public int ViewId { get; set; }
        public string ViewName { get; set; } = "";
        public bool IsCurrent { get; set; }
    }

    /// <summary>
    /// 要素ツリーの結果
    /// </summary>
    public class ElementTreeResult
    {
        public TreeNode RootNode { get; set; } = new();
        public int TotalElements { get; set; }
        public int MaxDepth { get; set; }
    }

    /// <summary>
    /// ツリーノード
    /// </summary>
    public class TreeNode
    {
        public ElementInfo Element { get; set; } = new();
        public List<TreeNode> Children { get; set; } = new();
        public int Depth { get; set; }
        public bool HasChildren => Children.Count > 0;
    }

    /// <summary>
    /// 変形（Transform）機能の結果
    /// </summary>
    public class TransformCapabilitiesResult
    {
        public bool CanMove { get; set; }
        public bool CanResize { get; set; }
        public bool CanRotate { get; set; }
        public BoundingRectangle CurrentBounds { get; set; } = new();
    }

    /// <summary>
    /// ウィンドウ機能の結果
    /// </summary>
    public class WindowCapabilitiesResult
    {
        public bool CanMaximize { get; set; }
        public bool CanMinimize { get; set; }
        public bool IsModal { get; set; }
        public bool IsTopmost { get; set; }
        public string WindowVisualState { get; set; } = "";
        public string WindowInteractionState { get; set; } = "";
    }

    /// <summary>
    /// パターン情報の結果
    /// </summary>
    public class PatternInfoResult
    {
        public string PatternName { get; set; } = "";
        public bool IsAvailable { get; set; }
        public Dictionary<string, object>? CurrentState { get; set; }
    }

    /// <summary>
    /// 複数パターン情報の結果
    /// </summary>
    public class PatternsInfoResult
    {
        public List<PatternInfoResult> Patterns { get; set; } = new();
        public int PatternCount => Patterns.Count;
        public List<string> AvailablePatternNames => Patterns.Where(p => p.IsAvailable).Select(p => p.PatternName).ToList();
    }

    /// <summary>
    /// コントロールタイプ情報の結果
    /// </summary>
    public class ControlTypeInfoResult
    {
        public string ControlType { get; set; } = "";
        public string ControlTypeName { get; set; } = "";
        public string LocalizedControlType { get; set; } = "";
        public string AutomationId { get; set; } = "";
        public string Name { get; set; } = "";
        public List<string> AvailablePatterns { get; set; } = new();
        public object? PatternValidation { get; set; }
        public object? DefaultProperties { get; set; }
    }

    /// <summary>
    /// スクロール情報の結果
    /// </summary>
    public class ScrollInfoResult
    {
        public double HorizontalScrollPercent { get; set; }
        public double VerticalScrollPercent { get; set; }
        public double HorizontalViewSize { get; set; }
        public double VerticalViewSize { get; set; }
        public bool HorizontallyScrollable { get; set; }
        public bool VerticallyScrollable { get; set; }
    }

    /// <summary>
    /// 選択情報の結果
    /// </summary>
    public class SelectionInfoResult
    {
        public List<ElementInfo> SelectedItems { get; set; } = new();
        public bool CanSelectMultiple { get; set; }
        public bool IsSelectionRequired { get; set; }
        public int SelectionCount => SelectedItems.Count;
        public bool HasSelection => SelectionCount > 0;
    }

    /// <summary>
    /// テキスト情報の結果
    /// </summary>
    public class TextInfoResult
    {
        public string Text { get; set; } = "";
        public int Length => Text.Length;
        public bool IsEmpty => string.IsNullOrEmpty(Text);
        public List<string> Lines => Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
        public int LineCount => Lines.Count;
    }

    /// <summary>
    /// グリッドアイテムの結果（単一アイテム）
    /// </summary>
    public class GridItemResult
    {
        public ElementInfo Element { get; set; } = new();
        public int Row { get; set; }
        public int Column { get; set; }
        public bool HasElement => Element != null && !string.IsNullOrEmpty(Element.AutomationId);
    }

    /// <summary>
    /// デスクトップウィンドウの結果
    /// </summary>
    public class DesktopWindowsResult
    {
        public List<WindowInfo> Windows { get; set; } = new();
        public int Count => Windows.Count;
        public bool HasWindows => Count > 0;
    }

    /// <summary>
    /// ウィンドウ情報
    /// </summary>
    public class WindowInfo
    {
        public string Title { get; set; } = "";
        public string ClassName { get; set; } = "";
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = "";
        public long Handle { get; set; }
        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
    }

    /// <summary>
    /// ツリーナビゲーションの結果
    /// </summary>
    public class TreeNavigationResult
    {
        public List<ElementInfo> Elements { get; set; } = new();
        public string NavigationType { get; set; } = ""; // "Parent", "Children", "Siblings", "Ancestors", "Descendants"
        public int Count => Elements.Count;
        public bool HasElements => Count > 0;
    }

    /// <summary>
    /// テキスト属性の結果
    /// </summary>
    public class TextAttributesResult
    {
        public List<TextRangeAttributes> TextRanges { get; set; } = new();
        public int RangeCount => TextRanges.Count;
        public bool HasRanges => RangeCount > 0;
    }

    /// <summary>
    /// 単一テキスト範囲の属性情報
    /// </summary>
    public class TextRangeAttributes
    {
        public string Text { get; set; } = "";
        public Dictionary<string, object> Attributes { get; set; } = new();
        public double[] BoundingRectangle { get; set; } = Array.Empty<double>();
    }

    /// <summary>
    /// テキストトラバーサル（移動）の結果
    /// </summary>
    public class TextTraversalResult
    {
        public List<TextMoveInfo> MoveResults { get; set; } = new();
        public int MoveCount => MoveResults.Count;
        public bool HasResults => MoveCount > 0;
    }

    /// <summary>
    /// 単一のテキスト移動情報
    /// </summary>
    public class TextMoveInfo
    {
        public int MovedUnits { get; set; }
        public string Text { get; set; } = "";
        public double[] BoundingRectangle { get; set; } = Array.Empty<double>();
    }
}