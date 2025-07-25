using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// 操作に関連するログを収集するヘルパークラス
    /// 特定の操作IDに関連するログエントリを追跡し、レスポンスに含めるために使用
    /// </summary>
    public class OperationLogCollector
    {
        private readonly ConcurrentDictionary<string, List<string>> _operationLogs = new();
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// 操作に関連するログエントリを追加
        /// </summary>
        /// <param name="operationId">操作の一意識別子</param>
        /// <param name="logMessage">ログメッセージ</param>
        public void AddLog(string operationId, string logMessage)
        {
            if (string.IsNullOrEmpty(operationId) || string.IsNullOrEmpty(logMessage))
                return;
                
            lock (_lockObject)
            {
                if (!_operationLogs.ContainsKey(operationId))
                {
                    _operationLogs[operationId] = new List<string>();
                }
                
                var timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff");
                _operationLogs[operationId].Add($"[{timestamp}] {logMessage}");
                
                // メモリリーク防止のため、古いログエントリを制限
                if (_operationLogs[operationId].Count > 100)
                {
                    _operationLogs[operationId] = _operationLogs[operationId].TakeLast(50).ToList();
                }
            }
        }
        
        /// <summary>
        /// 操作に関連するログエントリを取得
        /// </summary>
        /// <param name="operationId">操作の一意識別子</param>
        /// <returns>ログエントリのリスト</returns>
        public List<string> GetLogs(string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
                return new List<string>();
                
            lock (_lockObject)
            {
                if (_operationLogs.TryGetValue(operationId, out var logs))
                {
                    return new List<string>(logs);
                }
                
                return new List<string>();
            }
        }
        
        /// <summary>
        /// 操作のログエントリをクリアして、メモリを解放
        /// </summary>
        /// <param name="operationId">操作の一意識別子</param>
        public void ClearLogs(string operationId)
        {
            if (string.IsNullOrEmpty(operationId))
                return;
                
            lock (_lockObject)
            {
                _operationLogs.TryRemove(operationId, out _);
            }
        }
        
        /// <summary>
        /// 古い操作のログをクリーンアップ（定期実行推奨）
        /// </summary>
        /// <param name="maxAgeMinutes">保持する最大時間（分）</param>
        public void CleanupOldLogs(int maxAgeMinutes = 30)
        {
            lock (_lockObject)
            {
                // 実装を簡素化するため、すべてのログをクリア
                // 実際の運用では、タイムスタンプベースのクリーンアップが必要
                if (_operationLogs.Count > 1000)
                {
                    _operationLogs.Clear();
                }
            }
        }
    }
    
    /// <summary>
    /// OperationLogCollectorを使用するためのヘルパーメソッド
    /// </summary>
    public static class LogCollectorExtensions
    {
        private static readonly OperationLogCollector _collector = new();
        
        /// <summary>
        /// グローバルなログコレクターインスタンス
        /// </summary>
        public static OperationLogCollector Instance => _collector;
        
        /// <summary>
        /// ILoggerを拡張して、操作IDと共にログを記録
        /// </summary>
        /// <param name="logger">ロガーインスタンス</param>
        /// <param name="operationId">操作ID</param>
        /// <param name="logLevel">ログレベル</param>
        /// <param name="message">ログメッセージ</param>
        public static void LogWithOperation(this ILogger logger, string operationId, LogLevel logLevel, string message)
        {
            logger.Log(logLevel, message);
            _collector.AddLog(operationId, $"{logLevel}: {message}");
        }
        
        /// <summary>
        /// 情報ログを操作IDと共に記録
        /// </summary>
        public static void LogInformationWithOperation(this ILogger logger, string operationId, string message)
        {
            logger.LogInformation(message);
            _collector.AddLog(operationId, $"INFO: {message}");
        }
        
        /// <summary>
        /// エラーログを操作IDと共に記録
        /// </summary>
        public static void LogErrorWithOperation(this ILogger logger, string operationId, Exception ex, string message)
        {
            logger.LogError(ex, message);
            _collector.AddLog(operationId, $"ERROR: {message} - {ex.Message}");
        }
        
        /// <summary>
        /// 警告ログを操作IDと共に記録
        /// </summary>
        public static void LogWarningWithOperation(this ILogger logger, string operationId, string message)
        {
            logger.LogWarning(message);
            _collector.AddLog(operationId, $"WARN: {message}");
        }
    }
}