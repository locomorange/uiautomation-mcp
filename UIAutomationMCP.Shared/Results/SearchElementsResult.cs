using System.Text.Json.Serialization;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// SearchElementsツールの戻り値
    /// ElementInfo配列（基本情報 + オプショナル詳細情報）と検索メタデータを含む
    /// </summary>
    public class SearchElementsResult : BaseOperationResult
    {
        /// <summary>
        /// 検索で見つかった要素の情報配列（includeDetails, includeHierarchyオプションに応じて詳細情報を含む）
        /// </summary>
        [JsonPropertyName("elements")]
        public ElementInfo[] Elements { get; set; } = [];

        /// <summary>
        /// 検索実行に関するメタデータ
        /// </summary>
        [JsonPropertyName("metadata")]
        public new SearchMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// GetElementDetailsツールの戻り値
    /// 詳細なElementDetailと取得メタデータを含む
    /// </summary>
    public class ElementDetailResult : BaseOperationResult
    {
        /// <summary>
        /// 取得された要素の詳細情報
        /// </summary>
        [JsonPropertyName("element")]
        public ElementDetail Element { get; set; } = new();

        /// <summary>
        /// 詳細取得実行に関するメタデータ
        /// </summary>
        [JsonPropertyName("metadata")]
        public new DetailMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// 検索実行に関するメタデータ
    /// </summary>
    public class SearchMetadata
    {
        /// <summary>
        /// 検索で見つかった総件数
        /// </summary>
        [JsonPropertyName("totalFound")]
        public int TotalFound { get; set; }

        /// <summary>
        /// 実際に返された件数
        /// </summary>
        [JsonPropertyName("returned")]
        public int Returned { get; set; }

        /// <summary>
        /// 検索にかかった時間
        /// </summary>
        [JsonPropertyName("searchDuration")]
        public TimeSpan SearchDuration { get; set; }

        /// <summary>
        /// 使用された検索条件の説明
        /// </summary>
        [JsonPropertyName("searchCriteria")]
        public string SearchCriteria { get; set; } = string.Empty;

        /// <summary>
        /// 結果が切り詰められたかどうか
        /// </summary>
        [JsonPropertyName("wasTruncated")]
        public bool WasTruncated { get; set; }

        /// <summary>
        /// 検索改善のための提案
        /// </summary>
        [JsonPropertyName("suggestedRefinements")]
        public string[] SuggestedRefinements { get; set; } = [];

        /// <summary>
        /// 検索実行日時
        /// </summary>
        [JsonPropertyName("executedAt")]
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 詳細取得に関するメタデータ
    /// </summary>
    public class DetailMetadata
    {
        /// <summary>
        /// 詳細取得にかかった時間
        /// </summary>
        [JsonPropertyName("retrievalDuration")]
        public TimeSpan RetrievalDuration { get; set; }

        /// <summary>
        /// 取得されたパターン数
        /// </summary>
        [JsonPropertyName("patternsRetrieved")]
        public int PatternsRetrieved { get; set; }

        /// <summary>
        /// 階層情報が含まれているかどうか
        /// </summary>
        [JsonPropertyName("includesHierarchy")]
        public bool IncludesHierarchy { get; set; }

        /// <summary>
        /// 子要素の数（includeChildren=trueの場合）
        /// </summary>
        [JsonPropertyName("childrenCount")]
        public int ChildrenCount { get; set; }

        /// <summary>
        /// 詳細取得実行日時
        /// </summary>
        [JsonPropertyName("executedAt")]
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 要素識別に使用された条件
        /// </summary>
        [JsonPropertyName("identificationCriteria")]
        public string IdentificationCriteria { get; set; } = string.Empty;
    }

    /// <summary>
    /// 軽量なUI要素基本情報クラス
    /// 検索結果で使用される最低限の識別情報を提供
    /// パターン詳細は含まず、高速な検索結果表示に特化
    /// </summary>
    public class BasicElementInfo
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

}