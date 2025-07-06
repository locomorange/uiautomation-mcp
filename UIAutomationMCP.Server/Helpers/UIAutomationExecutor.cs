using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// UIAutomation API呼び出しのための共通スレッドラッパー
    /// タイムアウト処理、キャンセレーション、例外処理を統一
    /// </summary>
    public class UIAutomationExecutor
    {
        private readonly ILogger<UIAutomationExecutor> _logger;

        public UIAutomationExecutor(ILogger<UIAutomationExecutor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// UIAutomation API呼び出しを指定されたタイムアウトでSTAスレッドで実行
        /// </summary>
        public async Task<T> ExecuteAsync<T>(
            Func<T> operation, 
            int timeoutSeconds, 
            string operationName,
            CancellationToken cancellationToken = default)
        {
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            combinedCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            _logger.LogDebug("[UIAutomationExecutor] Starting operation: {OperationName} (timeout: {TimeoutSeconds}s)", 
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
                    _logger.LogWarning("[UIAutomationExecutor] Operation timed out: {OperationName}", operationName);
                    
                    // スレッドを強制終了
                    try
                    {
                        if (thread.IsAlive)
                        {
                            thread.Interrupt();
                            thread.Join(1000); // 1秒待機（強制終了はサポートされていないため削除）
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[UIAutomationExecutor] Failed to terminate thread for operation: {OperationName}", operationName);
                    }

                    throw new TimeoutException($"UIAutomation operation '{operationName}' timed out after {timeoutSeconds} seconds");
                }

                var result = await tcs.Task;
                _logger.LogDebug("[UIAutomationExecutor] Operation completed successfully: {OperationName}", operationName);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[UIAutomationExecutor] Operation cancelled: {OperationName}", operationName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationExecutor] Operation failed: {OperationName}", operationName);
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

        /// <summary>
        /// AutomationElement検索操作の共通実装
        /// </summary>
        public async Task<AutomationElement?> FindElementAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                () =>
                {
                    try
                    {
                        return searchRoot.FindFirst(scope, condition);
                    }
                    catch (ElementNotAvailableException)
                    {
                        return null;
                    }
                },
                timeoutSeconds,
                $"FindElement_{operationName}",
                cancellationToken);
        }

        /// <summary>
        /// AutomationElement複数検索操作の共通実装
        /// </summary>
        public async Task<AutomationElementCollection> FindElementsAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                () =>
                {
                    try
                    {
                        return searchRoot.FindAll(scope, condition);
                    }
                    catch (ElementNotAvailableException)
                    {
                        // AutomationElementCollectionは空コンストラクタがないため、空のTreeScopeで検索する
                        return searchRoot.FindAll(TreeScope.Element, new PropertyCondition(AutomationElement.ProcessIdProperty, -1));
                    }
                },
                timeoutSeconds,
                $"FindElements_{operationName}",
                cancellationToken);
        }

        /// <summary>
        /// パターン取得操作の共通実装
        /// </summary>
        public async Task<TPattern?> GetPatternAsync<TPattern>(
            AutomationElement element,
            AutomationPattern pattern,
            int timeoutSeconds,
            string operationName,
            CancellationToken cancellationToken = default) where TPattern : class
        {
            return await ExecuteAsync(
                () =>
                {
                    try
                    {
                        if (element.TryGetCurrentPattern(pattern, out var patternObject))
                        {
                            return patternObject as TPattern;
                        }
                        return null;
                    }
                    catch (ElementNotAvailableException)
                    {
                        return null;
                    }
                },
                timeoutSeconds,
                $"GetPattern_{operationName}",
                cancellationToken);
        }

        /// <summary>
        /// プロパティ取得操作の共通実装
        /// </summary>
        public async Task<T?> GetPropertyAsync<T>(
            AutomationElement element,
            AutomationProperty property,
            int timeoutSeconds,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                () =>
                {
                    try
                    {
                        var value = element.GetCurrentPropertyValue(property);
                        return value is T typedValue ? typedValue : default;
                    }
                    catch (ElementNotAvailableException)
                    {
                        return default;
                    }
                },
                timeoutSeconds,
                $"GetProperty_{operationName}",
                cancellationToken);
        }

        /// <summary>
        /// ルート要素取得の共通実装
        /// </summary>
        public async Task<AutomationElement> GetRootElementAsync(
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                () => AutomationElement.RootElement,
                timeoutSeconds,
                "GetRootElement",
                cancellationToken);
        }

        /// <summary>
        /// デスクトップ上のすべてのウィンドウを取得
        /// </summary>
        public async Task<AutomationElementCollection> GetDesktopWindowsAsync(
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            var rootElement = await GetRootElementAsync(timeoutSeconds, cancellationToken);
            var windowCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
            
            return await FindElementsAsync(
                rootElement,
                TreeScope.Children,
                windowCondition,
                timeoutSeconds,
                "GetDesktopWindows",
                cancellationToken);
        }
    }
}