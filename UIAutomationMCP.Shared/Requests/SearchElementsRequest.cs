using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Requests
{
    /// <summary>
    /// SearchElementsツールのリクエストパラメータ
    /// UI Automation標準プロパティに基づく軽量検索
    /// </summary>
    public class SearchElementsRequest : TypedWorkerRequest
    {
        public override string Operation => "SearchElements";

        /// <summary>
        /// Name、AutomationId、ClassNameを横断する汎用検索テキスト
        /// </summary>
        [JsonPropertyName("searchText")]
        public string? SearchText { get; set; }

        /// <summary>
        /// 特定のAutomationIdで検索
        /// </summary>
        [JsonPropertyName("automationId")]
        public string? AutomationId { get; set; }

        /// <summary>
        /// 特定のName（表示名）で検索
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// コントロールタイプでフィルタリング（Button、Slider、TextBox等）
        /// </summary>
        [JsonPropertyName("controlType")]
        public string? ControlType { get; set; }

        /// <summary>
        /// クラス名でフィルタリング
        /// </summary>
        [JsonPropertyName("className")]
        public string? ClassName { get; set; }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// プロセスID
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// 検索スコープ（descendants、children、subtree）
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "descendants";

        /// <summary>
        /// 必須のUI Automationパターン（全て満たす必要がある）
        /// </summary>
        [JsonPropertyName("requiredPatterns")]
        public string[]? RequiredPatterns { get; set; }

        /// <summary>
        /// いずれかのUI Automationパターン（一つでも満たせばOK）
        /// </summary>
        [JsonPropertyName("anyOfPatterns")]
        public string[]? AnyOfPatterns { get; set; }

        /// <summary>
        /// 可視要素のみに限定するかどうか
        /// </summary>
        [JsonPropertyName("visibleOnly")]
        public bool VisibleOnly { get; set; } = true;

        /// <summary>
        /// ファジーマッチングを有効にするかどうか
        /// </summary>
        [JsonPropertyName("fuzzyMatch")]
        public bool FuzzyMatch { get; set; } = false;

        /// <summary>
        /// 有効な要素のみに限定するかどうか
        /// </summary>
        [JsonPropertyName("enabledOnly")]
        public bool EnabledOnly { get; set; } = false;

        /// <summary>
        /// 最大結果数
        /// </summary>
        [JsonPropertyName("maxResults")]
        public int MaxResults { get; set; } = 50;

        /// <summary>
        /// 結果ソート方法（Name、ControlType、Position等）
        /// </summary>
        [JsonPropertyName("sortBy")]
        public string? SortBy { get; set; }

        /// <summary>
        /// タイムアウト秒数
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// GetElementDetailsツールのリクエストパラメータ
    /// 特定要素の包括的詳細情報取得
    /// </summary>
    public class GetElementDetailsRequest : TypedWorkerRequest
    {
        public override string Operation => "GetElementDetails";

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
        /// プロセスID
        /// </summary>
        [JsonPropertyName("processId")]
        public int? ProcessId { get; set; }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        [JsonPropertyName("windowTitle")]
        public string? WindowTitle { get; set; }

        /// <summary>
        /// 直接の子要素の基本情報を含めるかどうか
        /// </summary>
        [JsonPropertyName("includeChildren")]
        public bool IncludeChildren { get; set; } = false;

        /// <summary>
        /// 親要素の基本情報を含めるかどうか
        /// </summary>
        [JsonPropertyName("includeParent")]
        public bool IncludeParent { get; set; } = false;

        /// <summary>
        /// タイムアウト秒数
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }
}