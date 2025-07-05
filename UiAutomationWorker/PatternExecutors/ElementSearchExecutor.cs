using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// 要素検索操作を実行するエグゼキューター
    /// </summary>
    public class ElementSearchExecutor : BasePatternExecutor
    {
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public ElementSearchExecutor(
            ILogger<ElementSearchExecutor> logger,
            AutomationHelper automationHelper,
            ElementInfoExtractor elementInfoExtractor)
            : base(logger, automationHelper)
        {
            _elementInfoExtractor = elementInfoExtractor;
        }

        /// <summary>
        /// FindFirst操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteFindFirstAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync<object?>(operation, () =>
            {
                // Get search root
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    throw new InvalidOperationException("Failed to get search root element");
                }

                // Build condition
                var condition = _automationHelper.BuildCondition(operation);
                if (condition == null)
                {
                    throw new InvalidOperationException("Failed to build search condition");
                }

                // Parse scope
                if (!Enum.TryParse<TreeScope>(
                    operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                    out var scope))
                {
                    scope = TreeScope.Descendants;
                }

                _logger.LogInformation("[ElementSearchExecutor] Calling FindFirst with scope: {Scope}", scope);

                // This is the critical call that may hang
                var element = searchRoot.FindFirst(scope, condition);

                if (element != null)
                {
                    // Extract element information instead of returning the element itself
                    // (AutomationElement cannot be serialized across processes)
                    var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
                    return elementInfo;
                }
                else
                {
                    return null;
                }
            }, "FindFirst");
        }

        /// <summary>
        /// FindAll操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteFindAllAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync<List<Dictionary<string, object>>>(operation, () =>
            {
                // Get search root
                var searchRoot = _automationHelper.GetSearchRoot(operation);
                if (searchRoot == null)
                {
                    throw new InvalidOperationException("Failed to get search root element");
                }

                // Build condition
                var condition = _automationHelper.BuildCondition(operation);
                if (condition == null)
                {
                    throw new InvalidOperationException("Failed to build search condition");
                }

                // Parse scope
                if (!Enum.TryParse<TreeScope>(
                    operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                    out var scope))
                {
                    scope = TreeScope.Descendants;
                }

                _logger.LogInformation("[ElementSearchExecutor] Calling FindAll with scope: {Scope}", scope);

                // This is the critical call that may hang
                var elements = searchRoot.FindAll(scope, condition);

                var elementInfos = new List<Dictionary<string, object>>();
                
                if (elements != null && elements.Count > 0)
                {
                    foreach (AutomationElement element in elements)
                    {
                        try
                        {
                            var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
                            elementInfos.Add(elementInfo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[ElementSearchExecutor] Failed to extract info from element");
                            continue;
                        }
                    }
                }

                _logger.LogInformation("[ElementSearchExecutor] FindAll found {Count} elements", elementInfos.Count);

                return elementInfos;
            }, "FindAll");
        }

        /// <summary>
        /// 要素のプロパティ情報を取得します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetPropertiesAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[ElementSearchExecutor] Executing GetProperties operation");

            try
            {
                // GetPropertiesは通常FindFirstを使用して要素を見つけてから、その詳細プロパティを取得
                // ここでは簡略化してFindFirstと同じロジックを使用
                return await ExecuteFindFirstAsync(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchExecutor] GetProperties operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetProperties operation failed: {ex.Message}"
                };
            }
        }
    }
}