namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 要素検索操作の結果
    /// </summary>
    public class ElementSearchResult : CollectionOperationResult<ElementInfo>
    {
        /// <summary>
        /// 見つかった要素のリスト
        /// </summary>
        public List<ElementInfo> Elements 
        { 
            get => Items; 
            set => Items = value; 
        }

        /// <summary>
        /// 要素が見つかったかどうか
        /// </summary>
        public bool HasResults => HasItems;

        /// <summary>
        /// 検索条件（デバッグ用）
        /// </summary>
        public SearchCriteria? SearchCriteria { get; set; }
    }

    /// <summary>
    /// 検索条件
    /// </summary>
    public class SearchCriteria
    {
        public string? SearchText { get; set; }
        public string? ControlType { get; set; }
        public string? WindowTitle { get; set; }
        public int? ProcessId { get; set; }
        public string? Scope { get; set; }
        public string? PatternType { get; set; }
        public Dictionary<string, object>? AdditionalCriteria { get; set; }
    }

    /// <summary>
    /// コントロールタイプ別検索の結果
    /// </summary>
    public class ControlTypeSearchResult : ElementSearchResult
    {
        /// <summary>
        /// 検索サマリー情報
        /// </summary>
        public ControlTypeSearchSummary? SearchSummary { get; set; }
    }

    /// <summary>
    /// コントロールタイプ検索のサマリー
    /// </summary>
    public class ControlTypeSearchSummary
    {
        public string ControlType { get; set; } = "";
        public int TotalFound { get; set; }
        public int ValidElements { get; set; }
        public int InvalidElements { get; set; }
        public TimeSpan SearchDuration { get; set; }
        public string Scope { get; set; } = "";
        public int MaxResults { get; set; }
        public bool ValidationEnabled { get; set; }
    }

    /// <summary>
    /// パターン別検索の結果
    /// </summary>
    public class PatternSearchResult : ElementSearchResult  
    {
        /// <summary>
        /// 検索したパターン名
        /// </summary>
        public string PatternSearched { get; set; } = "";

        /// <summary>
        /// パターン検証が有効だったか
        /// </summary>
        public bool ValidationPerformed { get; set; }
    }
}