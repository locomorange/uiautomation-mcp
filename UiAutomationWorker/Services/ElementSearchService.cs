using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.Services
{
    /// <summary>
    /// 要素検索操作を実行するサービス
    /// </summary>
    public class ElementSearchService
    {
        private readonly ILogger<ElementSearchService> _logger;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public ElementSearchService(
            ILogger<ElementSearchService> logger,
            AutomationHelper automationHelper,
            ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        /// <summary>
        /// FindFirst操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteFindFirstAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[ElementSearchService] Executing FindFirst operation");

            try
            {
                // Set up timeout for the entire operation
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        // Get search root
                        var searchRoot = _automationHelper.GetSearchRoot(operation);
                        if (searchRoot == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to get search root element"
                            };
                        }

                        // Build condition
                        var condition = _automationHelper.BuildCondition(operation);
                        if (condition == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to build search condition"
                            };
                        }

                        // Parse scope
                        if (!Enum.TryParse<TreeScope>(
                            operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                            out var scope))
                        {
                            scope = TreeScope.Descendants;
                        }

                        _logger.LogInformation("[ElementSearchService] Calling FindFirst with scope: {Scope}", scope);

                        // This is the critical call that may hang
                        var element = searchRoot.FindFirst(scope, condition);

                        if (element != null)
                        {
                            // Extract element information instead of returning the element itself
                            // (AutomationElement cannot be serialized across processes)
                            var elementInfo = _elementInfoExtractor.ExtractElementInfo(element);
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = elementInfo
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = true,
                                Data = null
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[ElementSearchService] FindFirst execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"FindFirst failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[ElementSearchService] FindFirst operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindFirst operation timed out after {operation.Timeout} seconds. The target element may not be available or the UI may be unresponsive."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchService] FindFirst operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindFirst operation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// FindAll操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteFindAllAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[ElementSearchService] Executing FindAll operation");

            try
            {
                // Set up timeout for the entire operation
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));
                
                var result = await Task.Run(() =>
                {
                    try
                    {
                        // Get search root
                        var searchRoot = _automationHelper.GetSearchRoot(operation);
                        if (searchRoot == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to get search root element"
                            };
                        }

                        // Build condition
                        var condition = _automationHelper.BuildCondition(operation);
                        if (condition == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Failed to build search condition"
                            };
                        }

                        // Parse scope
                        if (!Enum.TryParse<TreeScope>(
                            operation.Parameters.GetValueOrDefault("Scope")?.ToString(), 
                            out var scope))
                        {
                            scope = TreeScope.Descendants;
                        }

                        _logger.LogInformation("[ElementSearchService] Calling FindAll with scope: {Scope}", scope);

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
                                    _logger.LogWarning(ex, "[ElementSearchService] Failed to extract info from element");
                                    continue;
                                }
                            }
                        }

                        _logger.LogInformation("[ElementSearchService] FindAll found {Count} elements", elementInfos.Count);

                        return new WorkerResult
                        {
                            Success = true,
                            Data = elementInfos
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[ElementSearchService] FindAll execution failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"FindAll failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[ElementSearchService] FindAll operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindAll operation timed out after {operation.Timeout} seconds. The search may be too broad or the UI may be unresponsive."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchService] FindAll operation failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindAll operation failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 要素のプロパティ情報を取得します
        /// </summary>
        public Task<WorkerResult> ExecuteGetPropertiesAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[ElementSearchService] Executing GetProperties operation");

            try
            {
                // GetPropertiesは通常FindFirstを使用して要素を見つけてから、その詳細プロパティを取得
                // ここでは簡略化してFindFirstと同じロジックを使用
                return ExecuteFindFirstAsync(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementSearchService] GetProperties operation failed");
                return Task.FromResult(new WorkerResult
                {
                    Success = false,
                    Error = $"GetProperties operation failed: {ex.Message}"
                });
            }
        }
    }
}
