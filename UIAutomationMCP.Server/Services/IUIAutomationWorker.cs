using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// UIAutomationワーカーのインターフェース
    /// サブプロセスへの操作委譲のみを担当
    /// </summary>
    public interface IUIAutomationWorker : IDisposable
    {
        /// <summary>
        /// 汎用的なワーカー操作実行
        /// </summary>
        Task<OperationResult<T>> ExecuteOperationAsync<T>(
            string operation,
            object parameters,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 型指定なしの操作実行（互換性のため）
        /// </summary>
        Task<OperationResult<object>> ExecuteOperationAsync(
            string operation,
            object parameters,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default);
    }
}