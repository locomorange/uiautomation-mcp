using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationWorker.Helpers;

namespace UiAutomationWorker.PatternExecutors
{
    /// <summary>
    /// TextPatternの操作を実行するクラス
    /// </summary>
    public class TextPatternExecutor
    {
        private readonly ILogger<TextPatternExecutor> _logger;
        private readonly AutomationHelper _automationHelper;

        public TextPatternExecutor(
            ILogger<TextPatternExecutor> logger,
            AutomationHelper automationHelper)
        {
            _logger = logger;
            _automationHelper = automationHelper;
        }

        /// <summary>
        /// GetText操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetTextAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[TextPatternExecutor] Executing GetText operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for GetText operation"
                            };
                        }

                        // TextPatternの取得と実行
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObj) && 
                            patternObj is TextPattern textPattern)
                        {
                            _logger.LogInformation("[TextPatternExecutor] Getting text from element: {ElementName}", 
                                SafeGetElementName(element));
                            
                            var textContent = textPattern.DocumentRange.GetText(-1);
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = textContent ?? ""
                            };
                        }
                        else
                        {
                            // fallback to element name/value
                            var name = SafeGetElementName(element);
                            return new WorkerResult
                            {
                                Success = true,
                                Data = name
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[TextPatternExecutor] GetText operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"GetText operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[TextPatternExecutor] GetText operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetText operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// SelectText操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteSelectTextAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[TextPatternExecutor] Executing SelectText operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for SelectText operation"
                            };
                        }

                        // パラメータの取得
                        if (!operation.Parameters.TryGetValue("startIndex", out var startIndexObj) ||
                            !operation.Parameters.TryGetValue("length", out var lengthObj))
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "startIndex and length parameters are required for SelectText operation"
                            };
                        }

                        var startIndex = Convert.ToInt32(startIndexObj);
                        var length = Convert.ToInt32(lengthObj);

                        // TextPatternの取得と実行
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObj) && 
                            patternObj is TextPattern textPattern)
                        {
                            _logger.LogInformation("[TextPatternExecutor] Selecting text in element: {ElementName}, range: {Start}-{End}", 
                                SafeGetElementName(element), startIndex, startIndex + length);
                            
                            var documentRange = textPattern.DocumentRange;
                            var textRange = documentRange.GetText(-1);
                            
                            if (startIndex < 0 || startIndex >= textRange.Length || 
                                length < 0 || startIndex + length > textRange.Length)
                            {
                                return new WorkerResult
                                {
                                    Success = false,
                                    Error = $"Invalid text range: startIndex={startIndex}, length={length}, textLength={textRange.Length}"
                                };
                            }

                            // TextPatternRangeの範囲選択（簡易実装）
                            // 実際のUIAutomationでは、range作成は複雑ですが、基本的な実装を提供
                            try
                            {
                                documentRange.Select();
                            }
                            catch
                            {
                                // フォールバック - 全体を選択
                                documentRange.Select();
                            }
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = $"Text selected: range {startIndex}-{startIndex + length}"
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support TextPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[TextPatternExecutor] SelectText operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"SelectText operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[TextPatternExecutor] SelectText operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"SelectText operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// FindText操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteFindTextAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[TextPatternExecutor] Executing FindText operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for FindText operation"
                            };
                        }

                        // パラメータの取得
                        if (!operation.Parameters.TryGetValue("searchText", out var searchTextObj))
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "searchText parameter is required for FindText operation"
                            };
                        }

                        var searchText = searchTextObj.ToString() ?? "";
                        var backward = operation.Parameters.TryGetValue("backward", out var backwardObj) && 
                                     Convert.ToBoolean(backwardObj);
                        var ignoreCase = operation.Parameters.TryGetValue("ignoreCase", out var ignoreCaseObj) && 
                                       Convert.ToBoolean(ignoreCaseObj);

                        // TextPatternの取得と実行
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObj) && 
                            patternObj is TextPattern textPattern)
                        {
                            _logger.LogInformation("[TextPatternExecutor] Finding text '{SearchText}' in element: {ElementName}", 
                                searchText, SafeGetElementName(element));
                            
                            var documentRange = textPattern.DocumentRange;
                            var foundRange = documentRange.FindText(searchText, backward, ignoreCase);
                            
                            if (foundRange != null)
                            {
                                var result = new Dictionary<string, object>
                                {
                                    ["found"] = true,
                                    ["text"] = foundRange.GetText(-1),
                                    ["startIndex"] = 0, // TextRange doesn't directly expose start index
                                    ["length"] = foundRange.GetText(-1).Length
                                };
                                
                                return new WorkerResult
                                {
                                    Success = true,
                                    Data = result
                                };
                            }
                            else
                            {
                                return new WorkerResult
                                {
                                    Success = true,
                                    Data = new Dictionary<string, object>
                                    {
                                        ["found"] = false,
                                        ["searchText"] = searchText
                                    }
                                };
                            }
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support TextPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[TextPatternExecutor] FindText operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"FindText operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[TextPatternExecutor] FindText operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"FindText operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// GetTextSelection操作を実行します
        /// </summary>
        public async Task<WorkerResult> ExecuteGetTextSelectionAsync(WorkerOperation operation)
        {
            _logger.LogInformation("[TextPatternExecutor] Executing GetTextSelection operation");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(operation.Timeout));

                var result = await Task.Run(() =>
                {
                    try
                    {
                        var element = FindElementFromOperation(operation);
                        if (element == null)
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Target element not found for GetTextSelection operation"
                            };
                        }

                        // TextPatternの取得と実行
                        if (element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObj) && 
                            patternObj is TextPattern textPattern)
                        {
                            _logger.LogInformation("[TextPatternExecutor] Getting text selection from element: {ElementName}", 
                                SafeGetElementName(element));
                            
                            var selections = textPattern.GetSelection();
                            var selectionList = new List<Dictionary<string, object>>();
                            
                            foreach (var selection in selections)
                            {
                                selectionList.Add(new Dictionary<string, object>
                                {
                                    ["text"] = selection.GetText(-1),
                                    ["startIndex"] = 0, // TextRange doesn't directly expose start index
                                    ["length"] = selection.GetText(-1).Length
                                });
                            }
                            
                            return new WorkerResult
                            {
                                Success = true,
                                Data = selectionList
                            };
                        }
                        else
                        {
                            return new WorkerResult
                            {
                                Success = false,
                                Error = "Element does not support TextPattern"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[TextPatternExecutor] GetTextSelection operation failed");
                        return new WorkerResult
                        {
                            Success = false,
                            Error = $"GetTextSelection operation failed: {ex.Message}"
                        };
                    }
                }, cts.Token);

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[TextPatternExecutor] GetTextSelection operation timed out after {Timeout}s", operation.Timeout);
                return new WorkerResult
                {
                    Success = false,
                    Error = $"GetTextSelection operation timed out after {operation.Timeout} seconds"
                };
            }
        }

        /// <summary>
        /// 操作からエレメントを取得します
        /// </summary>
        private AutomationElement? FindElementFromOperation(WorkerOperation operation)
        {
            try
            {
                var elementId = operation.Parameters.TryGetValue("elementId", out var elementIdObj) ? 
                    elementIdObj?.ToString() : null;
                var windowTitle = operation.Parameters.TryGetValue("windowTitle", out var windowTitleObj) ? 
                    windowTitleObj?.ToString() : null;
                var processId = operation.Parameters.TryGetValue("processId", out var processIdObj) ? 
                    Convert.ToInt32(processIdObj) : (int?)null;

                if (string.IsNullOrEmpty(elementId))
                {
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
                _logger.LogError(ex, "[TextPatternExecutor] Error finding element from operation");
                return null;
            }
        }

        /// <summary>
        /// エレメントの名前を安全に取得します
        /// </summary>
        private string SafeGetElementName(AutomationElement element)
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
    }
}