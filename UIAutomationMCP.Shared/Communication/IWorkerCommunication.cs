using UIAutomationMCP.Shared.Requests;

namespace UIAutomationMCP.Shared.Communication
{
    /// <summary>
    /// Worker プロセスとの通信インターフェース
    /// </summary>
    public interface IWorkerCommunication : IDisposable
    {
        /// <summary>
        /// 通信チャネルが利用可能かどうか
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 型安全なリクエストを送信し、型安全なレスポンスを受信
        /// </summary>
        Task<TResponse> SendRequestAsync<TResponse>(TypedWorkerRequest request, int timeoutSeconds = 60);

        /// <summary>
        /// 従来のWorkerRequestを送信（後方互換性のため）
        /// </summary>
        Task<WorkerResponse<object>> SendRequestAsync(WorkerRequest request, int timeoutSeconds = 60);

        /// <summary>
        /// 通信チャネルを開始
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 通信チャネルを停止
        /// </summary>
        Task StopAsync();
    }

    /// <summary>
    /// 改善されたプロセス間通信の方法
    /// </summary>
    public enum CommunicationMethod
    {
        /// <summary>
        /// 現在のConsole標準入出力 (後方互換性)
        /// </summary>
        StandardIO,

        /// <summary>
        /// Named Pipes (Windows推奨)
        /// </summary>
        NamedPipes,

        /// <summary>
        /// TCP Socket (クロスプラットフォーム)
        /// </summary>
        TcpSocket,

        /// <summary>
        /// gRPC (高パフォーマンス、型安全)
        /// </summary>
        Grpc
    }

    /// <summary>
    /// 通信設定
    /// </summary>
    public class CommunicationOptions
    {
        /// <summary>
        /// 通信方法
        /// </summary>
        public CommunicationMethod Method { get; set; } = CommunicationMethod.StandardIO;

        /// <summary>
        /// Named Pipes使用時のパイプ名
        /// </summary>
        public string PipeName { get; set; } = "UIAutomationMCP";

        /// <summary>
        /// TCP Socket使用時のポート
        /// </summary>
        public int Port { get; set; } = 0; // 0 = auto-assign

        /// <summary>
        /// バッファサイズ
        /// </summary>
        public int BufferSize { get; set; } = 8192;

        /// <summary>
        /// タイムアウト設定
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// JSON以外のシリアライゼーション使用
        /// </summary>
        public SerializationFormat SerializationFormat { get; set; } = SerializationFormat.Json;
    }

    /// <summary>
    /// シリアライゼーション形式
    /// </summary>
    public enum SerializationFormat
    {
        /// <summary>
        /// JSON (現在の形式)
        /// </summary>
        Json,

        /// <summary>
        /// MessagePack (効率的なバイナリ)
        /// </summary>
        MessagePack,

        /// <summary>
        /// Protocol Buffers
        /// </summary>
        ProtocolBuffers
    }

    /// <summary>
    /// パフォーマンス統計
    /// </summary>
    public class CommunicationStats
    {
        public long TotalRequests { get; set; }
        public long TotalResponses { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public long TotalBytesTransferred { get; set; }
        public int ActiveConnections { get; set; }
        public DateTime LastActivity { get; set; }
    }
}