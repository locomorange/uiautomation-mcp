using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Windows.Automation;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.ElementTree;
using UiAutomationMcpServer.Patterns.Interaction;
using UiAutomationMcpServer.Patterns.Layout;
using UiAutomationMcpServer.Patterns.Selection;
using UiAutomationMcpServer.Patterns.Text;
using UiAutomationMcpServer.Patterns.Window;
using UiAutomationMcpServer.Configuration;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// In-process UI Automation worker implementation
    /// Executes operations directly using pattern handlers instead of external process
    /// </summary>
    public class InProcessUIAutomationWorker : IUIAutomationWorker
    {
        private readonly ILogger<InProcessUIAutomationWorker> _logger;
        private readonly ElementSearchHandler _elementSearchHandler;
        private readonly TreeNavigationHandler _treeNavigationHandler;
        private readonly InvokePatternHandler _invokePatternHandler;
        private readonly ValuePatternHandler _valuePatternHandler;
        private readonly TogglePatternHandler _togglePatternHandler;
        private readonly SelectionItemPatternHandler _selectionItemPatternHandler;
        private readonly LayoutPatternHandler _layoutPatternHandler;
        private readonly TextPatternHandler _textPatternHandler;
        private readonly WindowPatternHandler _windowPatternHandler;
        private bool _disposed;

        public InProcessUIAutomationWorker(
            ILogger<InProcessUIAutomationWorker> logger,
            ElementSearchHandler elementSearchHandler,
            TreeNavigationHandler treeNavigationHandler,
            InvokePatternHandler invokePatternHandler,
            ValuePatternHandler valuePatternHandler,
            TogglePatternHandler togglePatternHandler,
            SelectionItemPatternHandler selectionItemPatternHandler,
            LayoutPatternHandler layoutPatternHandler,
            TextPatternHandler textPatternHandler,
            WindowPatternHandler windowPatternHandler)
        {
            _logger = logger;
            _elementSearchHandler = elementSearchHandler;
            _treeNavigationHandler = treeNavigationHandler;
            _invokePatternHandler = invokePatternHandler;
            _valuePatternHandler = valuePatternHandler;
            _togglePatternHandler = togglePatternHandler;
            _selectionItemPatternHandler = selectionItemPatternHandler;
            _layoutPatternHandler = layoutPatternHandler;
            _textPatternHandler = textPatternHandler;
            _windowPatternHandler = windowPatternHandler;
        }

        public async Task<OperationResult<T>> ExecuteOperationAsync<T>(
            string operation,
            object parameters,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            var result = await ExecuteOperationAsync(operation, parameters, timeoutSeconds, cancellationToken);
            
            if (!result.Success)
            {
                return TimeoutHelper.CreateErrorResult<T>(result.Error ?? "Operation failed", result.ExecutionSeconds);
            }

            try
            {
                if (result.Data == null)
                {
                    return TimeoutHelper.CreateSuccessResult<T>(default, result.ExecutionSeconds);
                }

                // Type conversion
                if (result.Data is T directCast)
                {
                    return TimeoutHelper.CreateSuccessResult(directCast, result.ExecutionSeconds);
                }

                // Special handling for List<ElementInfo> conversion
                if (typeof(T) == typeof(List<ElementInfo>) && result.Data is List<Dictionary<string, object>> dictList)
                {
                    var elementInfoList = new List<ElementInfo>();
                    foreach (var dict in dictList)
                    {
                        var elementInfo = ConvertDictionaryToElementInfo(dict);
                        if (elementInfo != null)
                        {
                            elementInfoList.Add(elementInfo);
                        }
                    }
                    return TimeoutHelper.CreateSuccessResult((T)(object)elementInfoList, result.ExecutionSeconds);
                }

                // Special handling for ElementInfo conversion
                if (typeof(T) == typeof(ElementInfo) && result.Data is Dictionary<string, object> singleDict)
                {
                    var elementInfo = ConvertDictionaryToElementInfo(singleDict);
                    if (elementInfo != null)
                    {
                        return TimeoutHelper.CreateSuccessResult((T)(object)elementInfo, result.ExecutionSeconds);
                    }
                }

                // JSON serialization/deserialization for type conversion with proper options
                var options = JsonSerializationConfig.GetOptions();
                var json = JsonSerializer.Serialize(result.Data, options);
                var typedData = JsonSerializer.Deserialize<T>(json, options);
                
                return TimeoutHelper.CreateSuccessResult(typedData, result.ExecutionSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert operation result to type {Type}", typeof(T).Name);
                return TimeoutHelper.CreateErrorResult<T>($"Failed to convert result: {ex.Message}", result.ExecutionSeconds);
            }
        }

        public async Task<OperationResult<object>> ExecuteOperationAsync(
            string operation,
            object parameters,
            int timeoutSeconds = 30,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation(
                "[InProcessWorker] Executing operation '{Operation}' with timeout {Timeout}s",
                operation, timeoutSeconds);

            try
            {
                // Convert parameters to WorkerOperation
                var workerOperation = CreateWorkerOperation(operation, parameters, timeoutSeconds);
                
                // Execute operation based on type
                WorkerResult result = operation.ToLowerInvariant() switch
                {
                    "findfirstelement" => await _elementSearchHandler.ExecuteFindFirstAsync(workerOperation),
                    "findall" or "findallelements" => await _elementSearchHandler.ExecuteFindAllAsync(workerOperation),
                    "getproperties" => await _elementSearchHandler.ExecuteGetPropertiesAsync(workerOperation),
                    "gettree" or "getelementtree" => await _treeNavigationHandler.ExecuteGetTreeAsync(workerOperation),
                    "getchildren" => await _treeNavigationHandler.ExecuteGetChildrenAsync(workerOperation),
                    "invoke" or "invokeelement" => await _invokePatternHandler.ExecuteInvokeAsync(workerOperation),
                    "setvalue" or "setelementvalue" => await _valuePatternHandler.ExecuteSetValueAsync(workerOperation),
                    "getvalue" or "getelementvalue" => await _valuePatternHandler.ExecuteGetValueAsync(workerOperation),
                    "toggle" or "toggleelement" => await _togglePatternHandler.ExecuteToggleAsync(workerOperation),
                    "select" or "selectelement" => await _selectionItemPatternHandler.ExecuteSelectAsync(workerOperation),
                    "scroll" or "scrollelement" => await _layoutPatternHandler.ExecuteScrollAsync(workerOperation),
                    "gettext" => await _textPatternHandler.ExecuteGetTextAsync(workerOperation),
                    "selecttext" => await _textPatternHandler.ExecuteSelectTextAsync(workerOperation),
                    "findtext" => await _textPatternHandler.ExecuteFindTextAsync(workerOperation),
                    "gettextselection" => await _textPatternHandler.ExecuteGetTextSelectionAsync(workerOperation),
                    "setwindowstate" => await _windowPatternHandler.ExecuteSetWindowStateAsync(workerOperation),
                    "getwindows" => await ExecuteGetWindowsAsync(workerOperation),
                    _ => new WorkerResult { Success = false, Error = $"Unknown operation: {operation}" }
                };

                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "[InProcessWorker] Operation '{Operation}' failed after {Elapsed:F1}s: {Error}",
                        operation, elapsed, result.Error);
                    
                    return TimeoutHelper.CreateErrorResult<object>(
                        result.Error ?? "Operation failed", 
                        elapsed);
                }

                _logger.LogInformation(
                    "[InProcessWorker] Operation '{Operation}' completed successfully in {Elapsed:F1}s",
                    operation, elapsed);
                
                return TimeoutHelper.CreateSuccessResult<object>(result.Data, elapsed);
            }
            catch (OperationCanceledException)
            {
                var elapsed = Math.Round((DateTime.UtcNow - startTime).TotalSeconds, 1);
                return TimeoutHelper.CreateTimeoutResult<object>(operation, timeoutSeconds, elapsed);
            }
            catch (Exception ex)
            {
                var elapsed = Math.Round((DateTime.UtcNow - startTime).TotalSeconds, 1);
                _logger.LogError(ex, "[InProcessWorker] Operation '{Operation}' failed with exception", operation);
                return TimeoutHelper.CreateErrorResult<object>($"Operation failed: {ex.Message}", elapsed);
            }
        }

        private WorkerOperation CreateWorkerOperation(string operation, object parameters, int timeoutSeconds)
        {
            var paramDict = new Dictionary<string, object>();
            
            // Convert parameters object to dictionary
            if (parameters != null)
            {
                var options = JsonSerializationConfig.GetOptions();
                var json = JsonSerializer.Serialize(parameters, options);
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                if (dict != null)
                {
                    paramDict = dict;
                }
            }

            return new WorkerOperation
            {
                Operation = operation,
                Parameters = paramDict,
                Timeout = timeoutSeconds
            };
        }

        // Legacy method implementations for compatibility
        public async Task<OperationResult<ElementInfo>> FindFirstElementAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync<ElementInfo>("FindFirstElement", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> InvokeElementAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("InvokeElement", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> SetElementValueAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("SetElementValue", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> GetElementTreeAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("GetElementTree", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<T>> ExecuteInProcessAsync<T>(string operation, object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync<T>(operation, parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> ToggleElementAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("ToggleElement", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> SelectElementAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("SelectElement", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> ScrollElementAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("ScrollElement", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> SetRangeValueAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("SetRangeValue", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> GetRangeValueAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("GetRangeValue", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<string>> GetTextAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync<string>("GetText", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> SelectTextAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("SelectText", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<object>> GetWindowInfoAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync("GetWindowInfo", parameters, timeoutSeconds);
        }

        public async Task<OperationResult<List<ElementInfo>>> FindAllAsync(object parameters, int timeoutSeconds = 30)
        {
            return await ExecuteOperationAsync<List<ElementInfo>>("FindAllElements", parameters, timeoutSeconds);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _logger?.LogDebug("[InProcessWorker] Disposing");
                }
                _disposed = true;
            }
        }

        private ElementInfo? ConvertDictionaryToElementInfo(Dictionary<string, object> dict)
        {
            try
            {
                var elementInfo = new ElementInfo
                {
                    Name = dict.TryGetValue("Name", out var name) ? name?.ToString() ?? "" : "",
                    AutomationId = dict.TryGetValue("AutomationId", out var automationId) ? automationId?.ToString() ?? "" : "",
                    ClassName = dict.TryGetValue("ClassName", out var className) ? className?.ToString() ?? "" : "",
                    ControlType = dict.TryGetValue("ControlType", out var controlType) ? controlType?.ToString() ?? "" : "",
                    ProcessId = dict.TryGetValue("ProcessId", out var processId) ? Convert.ToInt32(processId) : 0,
                    IsEnabled = dict.TryGetValue("IsEnabled", out var isEnabled) ? Convert.ToBoolean(isEnabled) : false,
                    IsVisible = dict.TryGetValue("IsVisible", out var isVisible) ? Convert.ToBoolean(isVisible) : false,
                    HelpText = dict.TryGetValue("HelpText", out var helpText) ? helpText?.ToString() ?? "" : ""
                };

                // Handle BoundingRectangle
                if (dict.TryGetValue("BoundingRectangle", out var boundingRectObj) && 
                    boundingRectObj is Dictionary<string, object> boundingRectDict)
                {
                    elementInfo.BoundingRectangle = new BoundingRectangle
                    {
                        X = boundingRectDict.TryGetValue("X", out var x) ? Convert.ToDouble(x) : 0.0,
                        Y = boundingRectDict.TryGetValue("Y", out var y) ? Convert.ToDouble(y) : 0.0,
                        Width = boundingRectDict.TryGetValue("Width", out var width) ? Convert.ToDouble(width) : 0.0,
                        Height = boundingRectDict.TryGetValue("Height", out var height) ? Convert.ToDouble(height) : 0.0
                    };
                }
                else
                {
                    elementInfo.BoundingRectangle = new BoundingRectangle { X = 0, Y = 0, Width = 0, Height = 0 };
                }

                return elementInfo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert dictionary to ElementInfo");
                return null;
            }
        }

        /// <summary>
        /// GetWindows操作を実行します
        /// </summary>
        private async Task<WorkerResult> ExecuteGetWindowsAsync(WorkerOperation operation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("[InProcessWorker] Executing GetWindows operation");

                    var windows = new List<WindowInfo>();
                    var desktopElement = System.Windows.Automation.AutomationElement.RootElement;
                    
                    // すべての子ウィンドウを取得
                    var condition = new PropertyCondition(
                        System.Windows.Automation.AutomationElement.ControlTypeProperty, 
                        ControlType.Window);
                    
                    var windowElements = desktopElement.FindAll(TreeScope.Children, condition);
                    
                    foreach (System.Windows.Automation.AutomationElement windowElement in windowElements)
                    {
                        try
                        {
                            var windowInfo = new WindowInfo
                            {
                                ProcessId = windowElement.Current.ProcessId,
                                Title = windowElement.Current.Name ?? "",
                                Name = windowElement.Current.Name ?? "",
                                AutomationId = windowElement.Current.AutomationId ?? "",
                                ClassName = windowElement.Current.ClassName ?? "",
                                BoundingRectangle = new BoundingRectangle
                                {
                                    X = (int)windowElement.Current.BoundingRectangle.X,
                                    Y = (int)windowElement.Current.BoundingRectangle.Y,
                                    Width = (int)windowElement.Current.BoundingRectangle.Width,
                                    Height = (int)windowElement.Current.BoundingRectangle.Height
                                },
                                IsVisible = !windowElement.Current.IsOffscreen,
                                IsEnabled = windowElement.Current.IsEnabled
                            };
                            
                            // 空のタイトルやサイズ0のウィンドウは除外
                            if (!string.IsNullOrEmpty(windowInfo.Title) && 
                                windowInfo.BoundingRectangle.Width > 0 && 
                                windowInfo.BoundingRectangle.Height > 0)
                            {
                                windows.Add(windowInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "[InProcessWorker] Failed to extract window info for a window");
                        }
                    }

                    _logger.LogInformation("[InProcessWorker] Found {Count} windows", windows.Count);

                    return new WorkerResult
                    {
                        Success = true,
                        Data = windows
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[InProcessWorker] GetWindows operation failed");
                    return new WorkerResult
                    {
                        Success = false,
                        Error = $"GetWindows operation failed: {ex.Message}"
                    };
                }
            });
        }
    }
}