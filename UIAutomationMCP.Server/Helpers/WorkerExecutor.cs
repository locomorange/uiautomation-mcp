using Microsoft.Extensions.Logging;
using UIAutomationMCP.Worker.Operations;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// Worker API呼び出しのための共通実行ラッパー
    /// タイムアウト処理、スレッド管理、例外処理を統一
    /// </summary>
    public class WorkerExecutor
    {
        private readonly ILogger<WorkerExecutor> _logger;
        private readonly InvokeOperations _invokeOperations;
        private readonly ValueOperations _valueOperations;
        private readonly ElementSearchOperations _searchOperations;
        private readonly ElementPropertyOperations _propertyOperations;

        public WorkerExecutor(
            ILogger<WorkerExecutor> logger,
            InvokeOperations invokeOperations,
            ValueOperations valueOperations,
            ElementSearchOperations searchOperations,
            ElementPropertyOperations propertyOperations)
        {
            _logger = logger;
            _invokeOperations = invokeOperations;
            _valueOperations = valueOperations;
            _searchOperations = searchOperations;
            _propertyOperations = propertyOperations;
        }

        /// <summary>
        /// Worker API呼び出しを指定されたタイムアウトでSTAスレッドで実行
        /// </summary>
        public async Task<T> ExecuteAsync<T>(
            Func<T> operation, 
            int timeoutSeconds, 
            string operationName,
            CancellationToken cancellationToken = default)
        {
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            combinedCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            _logger.LogDebug("[WorkerExecutor] Starting operation: {OperationName} (timeout: {TimeoutSeconds}s)", 
                operationName, timeoutSeconds);

            try
            {
                var tcs = new TaskCompletionSource<T>();

                var thread = new Thread(() =>
                {
                    try
                    {
                        var result = operation();
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                // タイムアウトまたはキャンセレーションの監視
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), combinedCts.Token);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _logger.LogWarning("[WorkerExecutor] Operation timed out: {OperationName}", operationName);
                    
                    // スレッドを強制終了
                    try
                    {
                        if (thread.IsAlive)
                        {
                            thread.Interrupt();
                            thread.Join(1000); // 1秒待機
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[WorkerExecutor] Failed to terminate thread for operation: {OperationName}", operationName);
                    }

                    throw new TimeoutException($"Worker operation '{operationName}' timed out after {timeoutSeconds} seconds");
                }

                var result = await tcs.Task;
                _logger.LogDebug("[WorkerExecutor] Operation completed successfully: {OperationName}", operationName);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[WorkerExecutor] Operation cancelled: {OperationName}", operationName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WorkerExecutor] Operation failed: {OperationName}", operationName);
                throw;
            }
        }

        /// <summary>
        /// void操作用のヘルパーメソッド
        /// </summary>
        public async Task ExecuteAsync(
            Action operation, 
            int timeoutSeconds, 
            string operationName,
            CancellationToken cancellationToken = default)
        {
            await ExecuteAsync(() =>
            {
                operation();
                return true; // dummy return value
            }, timeoutSeconds, operationName, cancellationToken);
        }

        // Worker Operations へのアクセサ
        public InvokeOperations Invoke => _invokeOperations;
        public ValueOperations Value => _valueOperations;
        public ElementSearchOperations Search => _searchOperations;
        public ElementPropertyOperations Property => _propertyOperations;
    }
}