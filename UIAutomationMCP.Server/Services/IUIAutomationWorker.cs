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

        // Legacy method compatibility for tests
        Task<OperationResult<ElementInfo>> FindFirstElementAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> InvokeElementAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> SetElementValueAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> GetElementTreeAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<T>> ExecuteInProcessAsync<T>(string operation, object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> ToggleElementAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> SelectElementAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> ScrollElementAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> SetRangeValueAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> GetRangeValueAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<string>> GetTextAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<object>> SelectTextAsync(object parameters, int timeoutSeconds = 30);
        
        // Additional missing methods for tests
        Task<OperationResult<object>> GetWindowInfoAsync(object parameters, int timeoutSeconds = 30);
        Task<OperationResult<List<ElementInfo>>> FindAllAsync(object parameters, int timeoutSeconds = 30);
    }
}