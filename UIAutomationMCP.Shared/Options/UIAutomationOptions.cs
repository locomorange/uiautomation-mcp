using System.ComponentModel.DataAnnotations;

namespace UIAutomationMCP.Shared.Options
{
    /// <summary>
    /// UI Automation操作の設定オプション
    /// </summary>
    public class UIAutomationOptions
    {
        public const string SectionName = "UIAutomation";

        /// <summary>
        /// 要素検索のデフォルト設定
        /// </summary>
        public ElementSearchOptions ElementSearch { get; set; } = new();

        /// <summary>
        /// ウィンドウ操作のデフォルト設定
        /// </summary>
        public WindowOperationOptions WindowOperation { get; set; } = new();

        /// <summary>
        /// テキスト操作のデフォルト設定
        /// </summary>
        public TextOperationOptions TextOperation { get; set; } = new();

        /// <summary>
        /// 変換操作のデフォルト設定
        /// </summary>
        public TransformOptions Transform { get; set; } = new();

        /// <summary>
        /// 範囲値操作のデフォルト設定
        /// </summary>
        public RangeValueOptions RangeValue { get; set; } = new();

        /// <summary>
        /// レイアウト操作のデフォルト設定
        /// </summary>
        public LayoutOptions Layout { get; set; } = new();
    }

    /// <summary>
    /// 要素検索のオプション
    /// </summary>
    public class ElementSearchOptions
    {
        /// <summary>
        /// デフォルト検索スコープ
        /// </summary>
        [Required]
        public string DefaultScope { get; set; } = "descendants";

        /// <summary>
        /// キャッシュを使用するか
        /// </summary>
        public bool UseCache { get; set; } = true;

        /// <summary>
        /// 正規表現を使用するか
        /// </summary>
        public bool UseRegex { get; set; } = false;

        /// <summary>
        /// ワイルドカードを使用するか
        /// </summary>
        public bool UseWildcard { get; set; } = false;

        /// <summary>
        /// 最大検索結果数
        /// </summary>
        [Range(1, 1000)]
        public int MaxResults { get; set; } = 100;

        /// <summary>
        /// パターン検証を行うか
        /// </summary>
        public bool ValidatePatterns { get; set; } = true;
    }

    /// <summary>
    /// ウィンドウ操作のオプション
    /// </summary>
    public class WindowOperationOptions
    {
        /// <summary>
        /// 非表示ウィンドウを含めるか
        /// </summary>
        public bool IncludeInvisible { get; set; } = false;

        /// <summary>
        /// ウィンドウアクションのデフォルト動作
        /// </summary>
        public string DefaultAction { get; set; } = "setfocus";
    }

    /// <summary>
    /// テキスト操作のオプション
    /// </summary>
    public class TextOperationOptions
    {
        /// <summary>
        /// テキスト検索で大文字小文字を区別しないか
        /// </summary>
        public bool DefaultIgnoreCase { get; set; } = true;

        /// <summary>
        /// テキスト検索のデフォルト方向（前方向）
        /// </summary>
        public bool DefaultBackward { get; set; } = false;

        /// <summary>
        /// テキスト移動のデフォルト単位
        /// </summary>
        public string DefaultTraverseUnit { get; set; } = "character";

        /// <summary>
        /// テキスト移動のデフォルト回数
        /// </summary>
        [Range(1, 1000)]
        public int DefaultTraverseCount { get; set; } = 1;
    }

    /// <summary>
    /// 変換操作のオプション
    /// </summary>
    public class TransformOptions
    {
        /// <summary>
        /// 移動操作のデフォルトX座標
        /// </summary>
        public double DefaultX { get; set; } = 0;

        /// <summary>
        /// 移動操作のデフォルトY座標
        /// </summary>
        public double DefaultY { get; set; } = 0;

        /// <summary>
        /// リサイズ操作のデフォルト幅
        /// </summary>
        [Range(1, 10000)]
        public double DefaultWidth { get; set; } = 100;

        /// <summary>
        /// リサイズ操作のデフォルト高さ
        /// </summary>
        [Range(1, 10000)]
        public double DefaultHeight { get; set; } = 100;

        /// <summary>
        /// 回転操作のデフォルト角度
        /// </summary>
        [Range(-360, 360)]
        public double DefaultRotationDegrees { get; set; } = 0;
    }

    /// <summary>
    /// 範囲値操作のオプション
    /// </summary>
    public class RangeValueOptions
    {
        /// <summary>
        /// デフォルト値
        /// </summary>
        public double DefaultValue { get; set; } = 0;

        /// <summary>
        /// 最小値
        /// </summary>
        public double DefaultMinimum { get; set; } = 0;

        /// <summary>
        /// 最大値
        /// </summary>
        public double DefaultMaximum { get; set; } = 100;

        /// <summary>
        /// ステップ値
        /// </summary>
        [Range(0.01, 100)]
        public double DefaultStep { get; set; } = 1;
    }

    /// <summary>
    /// レイアウト操作のオプション
    /// </summary>
    public class LayoutOptions
    {
        /// <summary>
        /// デフォルトのドック位置
        /// </summary>
        public string DefaultDockPosition { get; set; } = "none";

        /// <summary>
        /// デフォルトのExpandCollapse動作
        /// </summary>
        public string DefaultExpandCollapseAction { get; set; } = "toggle";

        /// <summary>
        /// デフォルトのスクロール方向
        /// </summary>
        public string DefaultScrollDirection { get; set; } = "down";

        /// <summary>
        /// デフォルトのスクロール量
        /// </summary>
        [Range(0.1, 10.0)]
        public double DefaultScrollAmount { get; set; } = 1.0;

        /// <summary>
        /// デフォルトの水平スクロール位置（パーセント）
        /// </summary>
        [Range(-1, 100)]
        public double DefaultHorizontalScrollPercent { get; set; } = -1;

        /// <summary>
        /// デフォルトの垂直スクロール位置（パーセント）
        /// </summary>
        [Range(-1, 100)]
        public double DefaultVerticalScrollPercent { get; set; } = -1;
    }

}