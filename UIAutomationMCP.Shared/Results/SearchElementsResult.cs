using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// SearchElementsツールの戻り値
    /// 軽量なElementInfo配列と検索メタデータを含む
    /// </summary>
    public class SearchElementsResult
    {
        /// <summary>
        /// 検索で見つかった要素の基本情報配列
        /// </summary>
        [JsonPropertyName("elements")]
        public ElementInfo[] Elements { get; set; } = [];

        /// <summary>
        /// 検索実行に関するメタデータ
        /// </summary>
        [JsonPropertyName("metadata")]
        public SearchMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// GetElementDetailsツールの戻り値
    /// 詳細なElementDetailと取得メタデータを含む
    /// </summary>
    public class ElementDetailResult
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
        public DetailMetadata Metadata { get; set; } = new();
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
}