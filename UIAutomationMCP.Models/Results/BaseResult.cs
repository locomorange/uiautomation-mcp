using System.Text.Json.Serialization;
using MessagePack;

namespace UIAutomationMCP.Models.Results
{
    /// <summary>
    /// 全ての操作結果の基底クラス
    /// </summary>
    [MessagePackObject]
    public abstract class BaseOperationResult
    {
        /// <summary>
        /// 操作が成功したかどうか
        /// </summary>
        [Key(0)]
        [JsonPropertyName("success")]
        public bool Success { get; set; } = true;

        /// <summary>
        /// エラーメッセージ（失敗時）
        /// </summary>
        [Key(1)]
        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 操作実行時刻
        /// </summary>
        [Key(2)]
        [JsonPropertyName("executedAt")]
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 操作の実行時間
        /// </summary>
        [Key(3)]
        [JsonPropertyName("executionTime")]
        public TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// 操作名
        /// </summary>
        [Key(4)]
        [JsonPropertyName("operationName")]
        public string OperationName { get; set; } = "";

        /// <summary>
        /// 追加のメタデータ
        /// </summary>
        [Key(5)]
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// 操作が失敗したかどうか
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public bool Failed => !Success;

        /// <summary>
        /// エラーメッセージが設定されているかどうか
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
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
    [MessagePackObject]
    public abstract class ValueOperationResult<T> : BaseOperationResult
    {
        /// <summary>
        /// 操作結果の値
        /// </summary>
        [Key(6)]
        [JsonPropertyName("value")]
        public T Value { get; set; } = default!;

        /// <summary>
        /// 値が設定されているかどうか
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public bool HasValue => Value != null;
    }

    /// <summary>
    /// コレクションを返す操作結果の基底クラス
    /// </summary>
    [MessagePackObject]
    public abstract class CollectionOperationResult<T> : BaseOperationResult
    {
        /// <summary>
        /// 操作結果のコレクション
        /// </summary>
        [Key(6)]
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// アイテム数
        /// </summary>
        [Key(7)]
        [JsonPropertyName("count")]
        public int Count => Items.Count;

        /// <summary>
        /// 結果が存在するかどうか
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public bool HasItems => Count > 0;

        /// <summary>
        /// 空の結果かどうか
        /// </summary>
        [JsonIgnore]
        [IgnoreMember]
        public bool IsEmpty => Count == 0;
    }
}