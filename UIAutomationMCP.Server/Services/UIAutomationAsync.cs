using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// UIAutomation APIの最小粒度操作を非同期・キャンセル可能にラップするクラス
    /// 同期的なUIAutomation APIをTask.Runでラップし、適切なタイムアウト/キャンセル機能を提供
    /// </summary>
    public static class UIAutomationAsync
    {
        private static readonly ILogger Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("UIAutomationAsync");
        
        #region Element Discovery Operations (Priority 1)
        
        /// <summary>
        /// 非同期で要素を検索（FindFirst）
        /// </summary>
        public static async Task<AutomationElement?> FindFirstAsync(
            this AutomationElement element,
            TreeScope scope,
            Condition condition,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Starting FindFirstAsync for element: {ElementName}", element.Current.Name);
                
                var result = await Task.Run(() => element.FindFirst(scope, condition), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] FindFirstAsync completed. Result: {HasResult}", result != null);
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] FindFirstAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] FindFirstAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期で要素を複数検索（FindAll）
        /// </summary>
        public static async Task<AutomationElementCollection> FindAllAsync(
            this AutomationElement element,
            TreeScope scope,
            Condition condition,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Starting FindAllAsync for element: {ElementName}", element.Current.Name);
                
                var result = await Task.Run(() => element.FindAll(scope, condition), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] FindAllAsync completed. Count: {Count}", result.Count);
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] FindAllAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] FindAllAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でRootElement取得
        /// </summary>
        public static async Task<AutomationElement> GetRootElementAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Getting RootElement");
                
                var result = await Task.Run(() => AutomationElement.RootElement, cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] RootElement obtained successfully");
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] GetRootElementAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] GetRootElementAsync failed");
                throw;
            }
        }

        #endregion

        #region Pattern Acquisition Operations (Priority 2)

        /// <summary>
        /// 非同期でパターン取得
        /// </summary>
        public static async Task<(bool Success, T? Pattern)> TryGetCurrentPatternAsync<T>(
            this AutomationElement element,
            AutomationPattern pattern,
            CancellationToken cancellationToken = default)
            where T : class
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Getting pattern {PatternName} for element: {ElementName}", 
                    pattern.ProgrammaticName, element.Current.Name);
                
                var result = await Task.Run(() =>
                {
                    var success = element.TryGetCurrentPattern(pattern, out var patternObject);
                    return (success, patternObject as T);
                }, cancellationToken).ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Pattern acquisition completed. Success: {Success}", result.success);
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] TryGetCurrentPatternAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] TryGetCurrentPatternAsync failed");
                throw;
            }
        }

        #endregion

        #region Pattern Method Invocations (Priority 3)

        /// <summary>
        /// 非同期でInvokeパターン実行
        /// </summary>
        public static async Task InvokeAsync(
            this InvokePattern pattern,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Invoking pattern");
                
                await Task.Run(() => pattern.Invoke(), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Pattern invocation completed");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] InvokeAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] InvokeAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でValueパターン設定
        /// </summary>
        public static async Task SetValueAsync(
            this ValuePattern pattern,
            string value,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Setting value: {Value}", value);
                
                await Task.Run(() => pattern.SetValue(value), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Value setting completed");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] SetValueAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] SetValueAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でToggleパターン実行
        /// </summary>
        public static async Task ToggleAsync(
            this TogglePattern pattern,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Toggling pattern");
                
                await Task.Run(() => pattern.Toggle(), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Pattern toggle completed");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] ToggleAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] ToggleAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でSelectionItemパターン実行
        /// </summary>
        public static async Task SelectAsync(
            this SelectionItemPattern pattern,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Selecting item");
                
                await Task.Run(() => pattern.Select(), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Item selection completed");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] SelectAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] SelectAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でExpandCollapseパターン実行
        /// </summary>
        public static async Task ExpandAsync(
            this ExpandCollapsePattern pattern,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Expanding element");
                
                await Task.Run(() => pattern.Expand(), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Element expansion completed");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] ExpandAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] ExpandAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でExpandCollapseパターン実行（折りたたみ）
        /// </summary>
        public static async Task CollapseAsync(
            this ExpandCollapsePattern pattern,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Collapsing element");
                
                await Task.Run(() => pattern.Collapse(), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Element collapse completed");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] CollapseAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] CollapseAsync failed");
                throw;
            }
        }

        #endregion

        #region Property Access Operations (Priority 4)

        /// <summary>
        /// 非同期でプロパティ取得
        /// </summary>
        public static async Task<T> GetCurrentPropertyAsync<T>(
            this AutomationElement element,
            AutomationProperty property,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Getting property {PropertyName} for element: {ElementName}", 
                    property.ProgrammaticName, element.Current.Name);
                
                var result = await Task.Run(() => (T)element.GetCurrentPropertyValue(property), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Property retrieval completed");
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] GetCurrentPropertyAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] GetCurrentPropertyAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でBoundingRectangle取得
        /// </summary>
        public static async Task<System.Windows.Rect> GetBoundingRectangleAsync(
            this AutomationElement element,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Getting bounding rectangle for element: {ElementName}", element.Current.Name);
                
                var result = await Task.Run(() => element.Current.BoundingRectangle, cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Bounding rectangle retrieval completed");
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] GetBoundingRectangleAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] GetBoundingRectangleAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でName取得
        /// </summary>
        public static async Task<string> GetNameAsync(
            this AutomationElement element,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Getting name for element");
                
                var result = await Task.Run(() => element.Current.Name, cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Name retrieval completed: {Name}", result);
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] GetNameAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] GetNameAsync failed");
                throw;
            }
        }

        #endregion

        #region Text Pattern Operations

        /// <summary>
        /// 非同期でテキスト取得
        /// </summary>
        public static async Task<string> GetTextAsync(
            this TextPattern pattern,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Getting text from TextPattern");
                
                var result = await Task.Run(() => pattern.DocumentRange.GetText(-1), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Text retrieval completed. Length: {Length}", result.Length);
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] GetTextAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] GetTextAsync failed");
                throw;
            }
        }

        /// <summary>
        /// 非同期でテキスト検索
        /// </summary>
        public static async Task<TextPatternRange?> FindTextAsync(
            this TextPattern pattern,
            string searchText,
            bool backward = false,
            bool ignoreCase = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Finding text: {SearchText}", searchText);
                
                var result = await Task.Run(() => pattern.DocumentRange.FindText(searchText, backward, ignoreCase), cancellationToken)
                    .ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Text search completed. Found: {Found}", result != null);
                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] FindTextAsync was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] FindTextAsync failed");
                throw;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// タイムアウト付きで操作を実行
        /// </summary>
        public static async Task<T> ExecuteWithTimeoutAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Executing operation with timeout: {TimeoutSeconds}s", timeoutSeconds);
                
                var result = await operation(combinedCts.Token).ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Operation completed successfully");
                return result;
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                Logger.LogWarning("[UIAutomationAsync] Operation timed out after {TimeoutSeconds}s", timeoutSeconds);
                throw new TimeoutException($"Operation timed out after {timeoutSeconds} seconds");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] Operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] Operation failed");
                throw;
            }
        }

        /// <summary>
        /// タイムアウト付きで操作を実行（戻り値なし）
        /// </summary>
        public static async Task ExecuteWithTimeoutAsync(
            Func<CancellationToken, Task> operation,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            try
            {
                Logger.LogDebug("[UIAutomationAsync] Executing operation with timeout: {TimeoutSeconds}s", timeoutSeconds);
                
                await operation(combinedCts.Token).ConfigureAwait(false);
                
                Logger.LogDebug("[UIAutomationAsync] Operation completed successfully");
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                Logger.LogWarning("[UIAutomationAsync] Operation timed out after {TimeoutSeconds}s", timeoutSeconds);
                throw new TimeoutException($"Operation timed out after {timeoutSeconds} seconds");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[UIAutomationAsync] Operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[UIAutomationAsync] Operation failed");
                throw;
            }
        }

        #endregion
    }
}