using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// パターン実行クラスの基底クラス
    /// 共通のタイムアウト処理、エラーハンドリング、ユーティリティメソッドを提供
    /// </summary>
    public abstract class BasePatternExecutor
    {
        protected readonly ILogger _logger;
        protected readonly AutomationHelper _automationHelper;

        protected BasePatternExecutor(ILogger logger, AutomationHelper automationHelper)
        {
            _logger = logger;
            _automationHelper = automationHelper;
        }

        /// <summary>
        /// タイムアウト処理付きで操作を実行します
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="operation">ワーカー操作</param>
        /// <param name="action">実行する処理</param>
        /// <param name="operationName">操作名（ログ用）</param>
        /// <returns>ワーカー結果</returns>
        protected async Task<WorkerResult> ExecuteWithTimeoutAsync<T>(
            WorkerOperation operation,
            Func<CancellationToken, Task<T>> action,
            string operationName)
        {
            _logger.LogInformation("[{ExecutorName}] Executing {OperationName} operation", 
                GetType().Name, operationName);

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(async () =>
                {
                    try
                    {
                        return await action(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{ExecutorName}] {OperationName} operation failed", 
                            GetType().Name, operationName);
                        throw;
                    }
                }, cts.Token);

                return new WorkerResult
                {
                    Success = true,
                    Data = result
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[{ExecutorName}] {OperationName} operation timed out after {Timeout}s", 
                    GetType().Name, operationName, operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"{operationName} operation timed out after {operation.Timeout} seconds"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ExecutorName}] {OperationName} operation failed", 
                    GetType().Name, operationName);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"{operationName} operation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// タイムアウト処理付きで同期操作を実行します
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="operation">ワーカー操作</param>
        /// <param name="action">実行する処理</param>
        /// <param name="operationName">操作名（ログ用）</param>
        /// <returns>ワーカー結果</returns>
        protected async Task<WorkerResult> ExecuteWithTimeoutAsync<T>(
            WorkerOperation operation,
            Func<T> action,
            string operationName)
        {
            return await ExecuteWithTimeoutAsync(operation, _ => Task.FromResult(action()), operationName);
        }

        /// <summary>
        /// 操作からエレメントを取得します
        /// </summary>
        /// <param name="operation">ワーカー操作</param>
        /// <returns>見つかったエレメント、または null</returns>
        protected AutomationElement? FindElementFromOperation(WorkerOperation operation)
        {
            try
            {
                // Try both "elementId" and "ElementId" for compatibility
                var elementId = operation.Parameters.TryGetValue("elementId", out var elementIdObj) ? 
                    elementIdObj?.ToString() : 
                    operation.Parameters.TryGetValue("ElementId", out var elementIdObj2) ? 
                        elementIdObj2?.ToString() : null;
                
                var windowTitle = operation.Parameters.TryGetValue("windowTitle", out var windowTitleObj) ? 
                    windowTitleObj?.ToString() : null;
                var processId = operation.Parameters.TryGetValue("processId", out var processIdObj) ? 
                    Convert.ToInt32(processIdObj) : (int?)null;

                if (string.IsNullOrEmpty(elementId))
                {
                    // Try to build condition from other parameters
                    var conditionSearchRoot = _automationHelper.GetSearchRoot(operation) ?? AutomationElement.RootElement;
                    var condition = _automationHelper.BuildCondition(operation);
                    if (condition != null)
                    {
                        return conditionSearchRoot.FindFirst(TreeScope.Descendants, condition);
                    }
                    return null;
                }

                // 検索ルートを取得
                var searchOperation = new WorkerOperation
                {
                    Parameters = new Dictionary<string, object>
                    {
                        ["windowTitle"] = windowTitle ?? "",
                        ["processId"] = processId ?? 0
                    }
                };
                var searchRoot = _automationHelper.GetSearchRoot(searchOperation) ?? AutomationElement.RootElement;
                
                return _automationHelper.FindElementById(elementId, searchRoot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ExecutorName}] Error finding element from operation", GetType().Name);
                return null;
            }
        }

        /// <summary>
        /// エレメントの名前を安全に取得します
        /// </summary>
        /// <param name="element">エレメント</param>
        /// <returns>エレメント名</returns>
        protected string SafeGetElementName(AutomationElement element)
        {
            try
            {
                return element.Current.Name ?? element.Current.AutomationId ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// エレメントが見つからない場合のエラー結果を作成します
        /// </summary>
        /// <param name="operationName">操作名</param>
        /// <returns>エラー結果</returns>
        protected WorkerResult CreateElementNotFoundResult(string operationName)
        {
            return new WorkerResult
            {
                Success = false,
                Error = $"Target element not found for {operationName} operation"
            };
        }

        /// <summary>
        /// パターンがサポートされていない場合のエラー結果を作成します
        /// </summary>
        /// <param name="patternName">パターン名</param>
        /// <returns>エラー結果</returns>
        protected WorkerResult CreatePatternNotSupportedResult(string patternName)
        {
            return new WorkerResult
            {
                Success = false,
                Error = $"Element does not support {patternName}"
            };
        }

        /// <summary>
        /// パラメータが見つからない場合のエラー結果を作成します
        /// </summary>
        /// <param name="parameterName">パラメータ名</param>
        /// <param name="operationName">操作名</param>
        /// <returns>エラー結果</returns>
        protected WorkerResult CreateParameterMissingResult(string parameterName, string operationName)
        {
            return new WorkerResult
            {
                Success = false,
                Error = $"{parameterName} parameter is required for {operationName} operation"
            };
        }
    }
}