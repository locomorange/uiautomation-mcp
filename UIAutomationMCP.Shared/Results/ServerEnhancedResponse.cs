using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// サーバー側で拡張された情報を含むレスポンス型
    /// MCP JsonContext制限を回避するため、JSON文字列としてシリアライズされます
    /// </summary>
    public class ServerEnhancedResponse<T>
    {
        /// <summary>
        /// 操作が成功したかどうか
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        /// <summary>
        /// Worker からの実際のデータ
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        
        /// <summary>
        /// エラーメッセージ（失敗時）
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// サーバー側実行情報
        /// </summary>
        [JsonPropertyName("executionInfo")]
        public ServerExecutionInfo ExecutionInfo { get; set; } = new();
        
        /// <summary>
        /// リクエストメタデータ
        /// </summary>
        [JsonPropertyName("requestMetadata")]
        public RequestMetadata RequestMetadata { get; set; } = new();
    }

    /// <summary>
    /// サーバー側での実行情報
    /// </summary>
    public class ServerExecutionInfo
    {
        /// <summary>
        /// サーバー側での実行時刻
        /// </summary>
        [JsonPropertyName("serverExecutedAt")]
        public DateTime ServerExecutedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// サーバー側での処理時間
        /// </summary>
        [JsonPropertyName("serverProcessingTime")]
        public string ServerProcessingTime { get; set; } = "";
        
        /// <summary>
        /// サーバーバージョン
        /// </summary>
        [JsonPropertyName("serverVersion")]
        public string ServerVersion { get; set; } = "1.0.0";
        
        /// <summary>
        /// 操作の一意識別子
        /// </summary>
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; } = "";
        
        /// <summary>
        /// サーバー側ログ（操作に関連するログエントリ）
        /// </summary>
        [JsonPropertyName("serverLogs")]
        public List<string> ServerLogs { get; set; } = new();
        
        /// <summary>
        /// 追加情報（任意のキー・値ペア）
        /// </summary>
        [JsonPropertyName("additionalInfo")]
        public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    }

    /// <summary>
    /// リクエストのメタデータ
    /// </summary>
    public class RequestMetadata
    {
        /// <summary>
        /// 呼び出されたメソッド名
        /// </summary>
        [JsonPropertyName("requestedMethod")]
        public string RequestedMethod { get; set; } = "";
        
        /// <summary>
        /// リクエストパラメータ
        /// </summary>
        [JsonPropertyName("requestParameters")]
        public Dictionary<string, object> RequestParameters { get; set; } = new();
        
        /// <summary>
        /// クライアント情報
        /// </summary>
        [JsonPropertyName("clientInfo")]
        public string ClientInfo { get; set; } = "MCP Client";
        
        /// <summary>
        /// タイムアウト秒数
        /// </summary>
        [JsonPropertyName("timeoutSeconds")]
        public int TimeoutSeconds { get; set; }
    }
}