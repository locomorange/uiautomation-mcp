using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 軽量なUI要素基本情報クラス
    /// 検索結果や階層表示に使用される最低限の識別情報を提供
    /// </summary>
    public class ElementInfo
    {
        /// <summary>
        /// UI Automation要素の一意識別子
        /// </summary>
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }

        /// <summary>
        /// 要素の表示名
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// コントロールタイプ（英語）
        /// </summary>
        [JsonPropertyName("controlType")]
        public string ControlType { get; set; } = string.Empty;

        /// <summary>
        /// ローカライズされたコントロールタイプ
        /// </summary>
        [JsonPropertyName("localizedControlType")]
        public string? LocalizedControlType { get; set; }

        /// <summary>
        /// クラス名
        /// </summary>
        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        /// <summary>
        /// 要素が有効かどうか
        /// </summary>
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 要素が可視かどうか
        /// </summary>
        [JsonPropertyName("isVisible")]
        public bool IsVisible { get; set; }

        /// <summary>
        /// 要素が画面外にあるかどうか
        /// </summary>
        [JsonPropertyName("isOffscreen")]
        public bool IsOffscreen { get; set; }

        /// <summary>
        /// 要素の境界矩形
        /// </summary>
        [JsonPropertyName("boundingRectangle")]
        public BoundingRectangle? BoundingRectangle { get; set; }

        /// <summary>
        /// サポートされているUI Automationパターンのリスト
        /// </summary>
        [JsonPropertyName("supportedPatterns")]
        public string[] SupportedPatterns { get; set; } = [];

        /// <summary>
        /// プロセスID
        /// </summary>
        [JsonPropertyName("processId")]
        public int ProcessId { get; set; }

        /// <summary>
        /// フレームワークID（Win32、XAML等）
        /// </summary>
        [JsonPropertyName("frameworkId")]
        public string? FrameworkId { get; set; }
    }

    /// <summary>
    /// 要素の境界矩形情報
    /// </summary>
    public class BoundingRectangle
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }
    }
}