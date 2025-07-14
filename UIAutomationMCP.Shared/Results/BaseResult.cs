using System.Text.Json.Serialization;

namespace UIAutomationMCP.Shared.Results
{
    /// <summary>
    /// 全ての操作結果の基底クラス
    /// </summary>
    public abstract class BaseOperationResult
    {
        /// <summary>
        /// 操作が成功したかどうか
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        /// <summary>
        /// エラーメッセージ（失敗時）
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 操作実行時刻
        /// </summary>
        [JsonPropertyName("executedAt")]
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 操作の実行時間
        /// </summary>
        [JsonPropertyName("executionTime")]
        public TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// 操作名
        /// </summary>
        [JsonPropertyName("operationName")]
        public string OperationName { get; set; } = "";

        /// <summary>
        /// 追加のメタデータ
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// 操作が失敗したかどうか
        /// </summary>
        [JsonIgnore]
        public bool Failed => !Success;

        /// <summary>
        /// エラーメッセージが設定されているかどうか
        /// </summary>
        [JsonIgnore]
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// 操作を失敗として設定
        /// </summary>
        public void SetFailure(string errorMessage)
        {
            Success = false;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// メタデータを追加
        /// </summary>
        public void AddMetadata(string key, object value)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata[key] = value;
        }
    }

    /// <summary>
    /// 値を返す操作結果の基底クラス
    /// </summary>
    public abstract class ValueOperationResult<T> : BaseOperationResult
    {
        /// <summary>
        /// 操作結果の値
        /// </summary>
        [JsonPropertyName("value")]
        public T Value { get; set; } = default!;

        /// <summary>
        /// 値が設定されているかどうか
        /// </summary>
        [JsonIgnore]
        public bool HasValue => Value != null;
    }

    /// <summary>
    /// コレクションを返す操作結果の基底クラス
    /// </summary>
    public abstract class CollectionOperationResult<T> : BaseOperationResult
    {
        /// <summary>
        /// 操作結果のコレクション
        /// </summary>
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// アイテム数
        /// </summary>
        [JsonPropertyName("count")]
        public int Count => Items.Count;

        /// <summary>
        /// 結果が存在するかどうか
        /// </summary>
        [JsonIgnore]
        public bool HasItems => Count > 0;

        /// <summary>
        /// 空の結果かどうか
        /// </summary>
        [JsonIgnore]
        public bool IsEmpty => Count == 0;
    }
}