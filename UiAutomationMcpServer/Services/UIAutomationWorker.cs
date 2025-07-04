using System.Diagnostics;
using System.Text.Json;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// Worker service for executing UI Automation operations in a separate process
    /// to prevent main process from hanging due to COM/native API blocking
    /// </summary>
    public interface IUIAutomationWorker
    {
        // Core subprocess execution methods
        Task<OperationResult<string>> ExecuteInProcessAsync(
            string operationJson,
            int timeoutSeconds = 10);

        // High-level UI Automation methods that use the subprocess worker
        Task<OperationResult<List<ElementInfo>>> FindAllAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 30);

        Task<OperationResult<ElementInfo?>> FindFirstAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 15);

        Task<OperationResult<string>> InvokeElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> SetElementValueAsync(
            string elementId,
            string value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> GetElementValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> ToggleElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> SelectElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // Layout Pattern methods
        Task<OperationResult<string>> ExpandCollapseElementAsync(
            string elementId,
            bool? expand = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> ScrollElementAsync(
            string elementId,
            string? direction = null,
            double? horizontal = null,
            double? vertical = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> ScrollElementIntoViewAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> TransformElementAsync(
            string elementId,
            string action,
            double? x = null,
            double? y = null,
            double? width = null,
            double? height = null,
            double? degrees = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> DockElementAsync(
            string elementId,
            string position,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // UIAutomationHelper から移植したメソッド
        Task<OperationResult<ElementInfo?>> FindElementSafelyAsync(
            string? elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 15);

        Task<OperationResult<T>> ExecuteWithTimeoutAsync<T>(
            Func<Task<OperationResult<T>>> operation,
            string operationName,
            int timeoutSeconds = 30);

        OperationResult<T> SafeExecute<T>(
            Func<T> operation,
            string operationName,
            T? defaultValue = default);
    }

    public class UIAutomationWorker : IUIAutomationWorker
    {
        private readonly ILogger<UIAutomationWorker> _logger;
        private readonly string _workerExecutablePath;

        public UIAutomationWorker(ILogger<UIAutomationWorker> logger)
        {
            _logger = logger;
            // Worker executable will be in the same directory as the main application
            _workerExecutablePath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                "UiAutomationWorker.exe");
        }

        public async Task<OperationResult<List<ElementInfo>>> FindAllAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            _logger.LogInformation("FindAllAsync via worker - WindowTitle: '{WindowTitle}', SearchText: '{SearchText}', ControlType: '{ControlType}'",
                windowTitle, searchText, controlType);

            try
            {
                var parameters = CreateSearchParameters(searchText, controlType, windowTitle, processId);
                var operation = new
                {
                    Operation = "findall",
                    Parameters = parameters,
                    Timeout = timeoutSeconds
                };

                var operationJson = JsonSerializer.Serialize(operation);
                var workerResult = await ExecuteInProcessAsync(operationJson, timeoutSeconds);

                if (!workerResult.Success)
                {
                    _logger.LogError("Worker findall failed: {Error}", workerResult.Error);
                    return new OperationResult<List<ElementInfo>>
                    {
                        Success = false,
                        Error = workerResult.Error ?? "Worker operation failed"
                    };
                }

                var elementInfos = ParseWorkerResultToElementInfoList(workerResult.Data);
                return new OperationResult<List<ElementInfo>>
                {
                    Success = true,
                    Data = elementInfos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindAllAsync failed");
                return new OperationResult<List<ElementInfo>>
                {
                    Success = false,
                    Error = $"FindAll operation failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<ElementInfo?>> FindFirstAsync(
            string? windowTitle = null,
            string? searchText = null,
            string? controlType = null,
            int? processId = null,
            int timeoutSeconds = 15)
        {
            _logger.LogInformation("FindFirstAsync via worker - WindowTitle: '{WindowTitle}', SearchText: '{SearchText}', ControlType: '{ControlType}'",
                windowTitle, searchText, controlType);

            try
            {
                var parameters = CreateSearchParameters(searchText, controlType, windowTitle, processId);
                var operation = new
                {
                    Operation = "findfirst",
                    Parameters = parameters,
                    Timeout = timeoutSeconds
                };

                var operationJson = JsonSerializer.Serialize(operation);
                var workerResult = await ExecuteInProcessAsync(operationJson, timeoutSeconds);

                if (!workerResult.Success)
                {
                    _logger.LogError("Worker findfirst failed: {Error}", workerResult.Error);
                    return new OperationResult<ElementInfo?>
                    {
                        Success = false,
                        Error = workerResult.Error ?? "Worker operation failed"
                    };
                }

                var elementInfo = ParseWorkerResultToElementInfo(workerResult.Data);
                return new OperationResult<ElementInfo?>
                {
                    Success = true,
                    Data = elementInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindFirstAsync failed");
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = $"FindFirst operation failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<string>> InvokeElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            return await ExecutePatternOperationAsync("invoke", elementId, null, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> SetElementValueAsync(
            string elementId,
            string value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var valueParameters = new Dictionary<string, object> { ["value"] = value };
            return await ExecutePatternOperationAsync("setvalue", elementId, valueParameters, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> GetElementValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            return await ExecutePatternOperationAsync("get_value", elementId, null, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> ToggleElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            return await ExecutePatternOperationAsync("toggle", elementId, null, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> SelectElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            return await ExecutePatternOperationAsync("select", elementId, null, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> ExpandCollapseElementAsync(
            string elementId,
            bool? expand = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new Dictionary<string, object>();
            if (expand.HasValue)
            {
                parameters["expand"] = expand.Value;
            }
            return await ExecutePatternOperationAsync("expandcollapse", elementId, parameters, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> ScrollElementAsync(
            string elementId,
            string? direction = null,
            double? horizontal = null,
            double? vertical = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(direction))
            {
                parameters["direction"] = direction;
            }
            if (horizontal.HasValue)
            {
                parameters["horizontal"] = horizontal.Value;
            }
            if (vertical.HasValue)
            {
                parameters["vertical"] = vertical.Value;
            }
            return await ExecutePatternOperationAsync("scroll", elementId, parameters, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> ScrollElementIntoViewAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            return await ExecutePatternOperationAsync("scrollintoview", elementId, null, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> TransformElementAsync(
            string elementId,
            string action,
            double? x = null,
            double? y = null,
            double? width = null,
            double? height = null,
            double? degrees = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new Dictionary<string, object>
            {
                ["action"] = action
            };
            if (x.HasValue) parameters["x"] = x.Value;
            if (y.HasValue) parameters["y"] = y.Value;
            if (width.HasValue) parameters["width"] = width.Value;
            if (height.HasValue) parameters["height"] = height.Value;
            if (degrees.HasValue) parameters["degrees"] = degrees.Value;
            
            return await ExecutePatternOperationAsync("transform", elementId, parameters, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> DockElementAsync(
            string elementId,
            string position,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new Dictionary<string, object>
            {
                ["position"] = position
            };
            return await ExecutePatternOperationAsync("dock", elementId, parameters, windowTitle, processId, timeoutSeconds);
        }

        public async Task<OperationResult<string>> ExecuteInProcessAsync(
            string operationJson,
            int timeoutSeconds = 10)
        {
            _logger.LogInformation("[UIAutomationWorker] Executing operation in subprocess, timeout: {Timeout}s", timeoutSeconds);

            try
            {
                return await ExecuteWorkerProcessAsync(operationJson, timeoutSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationWorker] ExecuteInProcess failed");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Subprocess execution failed: {ex.Message}"
                };
            }
        }

        private async Task<OperationResult<string>> ExecuteWorkerProcessAsync(
            string inputJson,
            int timeoutSeconds)
        {
            Process? workerProcess = null;
            
            try
            {
                // Check if worker executable exists
                if (!System.IO.File.Exists(_workerExecutablePath))
                {
                    _logger.LogError("[UIAutomationWorker] Worker executable not found: {Path}", _workerExecutablePath);
                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = $"Worker executable not found: {_workerExecutablePath}"
                    };
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _workerExecutablePath,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                workerProcess = new Process { StartInfo = processStartInfo };
                
                _logger.LogInformation("[UIAutomationWorker] Starting worker process: {Path}", _workerExecutablePath);
                workerProcess.Start();

                // Send input data
                await workerProcess.StandardInput.WriteLineAsync(inputJson);
                await workerProcess.StandardInput.FlushAsync();
                workerProcess.StandardInput.Close();

                // Wait for completion with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var processTask = workerProcess.WaitForExitAsync(cts.Token);
                
                bool completedInTime;
                try
                {
                    await processTask;
                    completedInTime = true;
                }
                catch (OperationCanceledException)
                {
                    completedInTime = false;
                }
                
                if (!completedInTime)
                {
                    _logger.LogWarning("[UIAutomationWorker] Worker process timeout after {Timeout}s, killing process", timeoutSeconds);
                    
                    try
                    {
                        workerProcess.Kill(entireProcessTree: true);
                    }
                    catch (Exception killEx)
                    {
                        _logger.LogError(killEx, "[UIAutomationWorker] Failed to kill worker process");
                    }

                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = $"Worker process timeout after {timeoutSeconds} seconds. UI Automation operation was forcibly terminated."
                    };
                }

                // Read results
                var output = await workerProcess.StandardOutput.ReadToEndAsync();
                var error = await workerProcess.StandardError.ReadToEndAsync();

                if (workerProcess.ExitCode != 0)
                {
                    _logger.LogError("[UIAutomationWorker] Worker process failed with exit code {ExitCode}. Error: {Error}", 
                        workerProcess.ExitCode, error);
                    
                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = $"Worker process failed (exit code {workerProcess.ExitCode}): {error}"
                    };
                }

                _logger.LogInformation("[UIAutomationWorker] Worker process completed successfully");
                return new OperationResult<string> { Success = true, Data = output };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationWorker] Worker process execution failed");
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Worker process execution failed: {ex.Message}"
                };
            }
            finally
            {
                try
                {
                    workerProcess?.Kill(entireProcessTree: true);
                    workerProcess?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[UIAutomationWorker] Error disposing worker process");
                }
            }
        }

        private Dictionary<string, object> CreateSearchParameters(string? searchText, string? controlType, string? windowTitle, int? processId)
        {
            var parameters = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(searchText))
            {
                parameters["Name"] = searchText;
                parameters["AutomationId"] = searchText;
            }
            
            if (!string.IsNullOrEmpty(controlType))
            {
                parameters["ControlType"] = controlType;
            }
            
            if (!string.IsNullOrEmpty(windowTitle))
            {
                parameters["WindowTitle"] = windowTitle;
            }
            
            if (processId.HasValue)
            {
                parameters["ProcessId"] = processId.Value;
            }

            return parameters;
        }

        private async Task<OperationResult<string>> ExecutePatternOperationAsync(
            string operation,
            string elementId,
            Dictionary<string, object>? additionalParameters,
            string? windowTitle,
            int? processId,
            int timeoutSeconds)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["ElementId"] = elementId
                };

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    parameters["WindowTitle"] = windowTitle;
                }

                if (processId.HasValue)
                {
                    parameters["ProcessId"] = processId.Value;
                }

                if (additionalParameters != null)
                {
                    foreach (var kvp in additionalParameters)
                    {
                        parameters[kvp.Key] = kvp.Value;
                    }
                }

                var operationData = new
                {
                    Operation = operation,
                    Parameters = parameters,
                    Timeout = timeoutSeconds
                };

                var operationJson = JsonSerializer.Serialize(operationData);
                var workerResult = await ExecuteInProcessAsync(operationJson, timeoutSeconds);

                if (!workerResult.Success)
                {
                    _logger.LogError("Worker {Operation} failed: {Error}", operation, workerResult.Error);
                    return new OperationResult<string>
                    {
                        Success = false,
                        Error = workerResult.Error ?? "Worker operation failed"
                    };
                }

                // Extract the result data from worker response
                string? result = ExtractResultFromWorkerResponse(workerResult.Data);
                return new OperationResult<string>
                {
                    Success = true,
                    Data = result ?? "Operation completed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecutePatternOperationAsync failed for operation {Operation}", operation);
                return new OperationResult<string>
                {
                    Success = false,
                    Error = $"Operation {operation} failed: {ex.Message}"
                };
            }
        }

        private List<ElementInfo> ParseWorkerResultToElementInfoList(string? workerData)
        {
            var elementInfos = new List<ElementInfo>();
            
            if (string.IsNullOrEmpty(workerData))
                return elementInfos;

            try
            {
                using var jsonDoc = JsonDocument.Parse(workerData);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("Success", out var successProp) && successProp.GetBoolean())
                {
                    if (root.TryGetProperty("Data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var elementJson in dataProp.EnumerateArray())
                        {
                            try
                            {
                                var elementInfo = JsonSerializer.Deserialize<ElementInfo>(elementJson.GetRawText());
                                if (elementInfo != null)
                                {
                                    elementInfos.Add(elementInfo);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Failed to deserialize element info: {Error}", ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse worker result JSON for element list");
            }

            return elementInfos;
        }

        private ElementInfo? ParseWorkerResultToElementInfo(string? workerData)
        {
            if (string.IsNullOrEmpty(workerData))
                return null;

            try
            {
                using var jsonDoc = JsonDocument.Parse(workerData);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("Success", out var successProp) && successProp.GetBoolean())
                {
                    if (root.TryGetProperty("Data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
                    {
                        return JsonSerializer.Deserialize<ElementInfo>(dataProp.GetRawText());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse worker result JSON for element info");
            }

            return null;
        }

        private string? ExtractResultFromWorkerResponse(string? workerData)
        {
            if (string.IsNullOrEmpty(workerData))
                return null;

            try
            {
                using var jsonDoc = JsonDocument.Parse(workerData);
                var root = jsonDoc.RootElement;
                
                if (root.TryGetProperty("Success", out var successProp) && successProp.GetBoolean())
                {
                    if (root.TryGetProperty("Data", out var dataProp))
                    {
                        return dataProp.ValueKind == JsonValueKind.String 
                            ? dataProp.GetString()
                            : dataProp.GetRawText();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract result from worker response");
            }

            return null;
        }

        public async Task<OperationResult<ElementInfo?>> FindElementSafelyAsync(
            string? elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 15)
        {
            try
            {
                if (string.IsNullOrEmpty(elementId))
                {
                    return new OperationResult<ElementInfo?>
                    {
                        Success = false,
                        Error = "Element ID is required"
                    };
                }

                // elementId を Name と AutomationId の両方で検索
                return await FindFirstAsync(
                    windowTitle: windowTitle,
                    searchText: elementId,
                    controlType: null,
                    processId: processId,
                    timeoutSeconds: timeoutSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FindElementSafelyAsync failed for elementId: {ElementId}", elementId);
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = $"Failed to find element: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<List<ElementInfo>>> FindAllElementsAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds = 60)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("FindAllElementsAsync started via worker - SearchRoot: {SearchRootName} (ProcessId: {ProcessId}), Scope: {Scope}, Timeout: {TimeoutSeconds}s",
                SafeGetElementName(searchRoot), SafeGetElementProcessId(searchRoot), scope, timeoutSeconds);
            
            try
            {
                // AutomationElementとConditionをWorkerに渡すためにシリアライズ可能な形式に変換
                var parameters = new Dictionary<string, object>
                {
                    ["SearchRootName"] = SafeGetElementName(searchRoot) ?? "",
                    ["SearchRootAutomationId"] = SafeGetElementAutomationId(searchRoot) ?? "",
                    ["SearchRootProcessId"] = SafeGetElementProcessId(searchRoot),
                    ["Scope"] = scope.ToString(),
                    ["ConditionType"] = condition.GetType().Name
                };

                // 条件の詳細を追加
                AddConditionDetails(condition, parameters);

                var operation = new
                {
                    Operation = "findall",
                    Parameters = parameters,
                    Timeout = timeoutSeconds
                };

                var operationJson = JsonSerializer.Serialize(operation);
                var workerResult = await ExecuteInProcessAsync(operationJson, timeoutSeconds);

                stopwatch.Stop();

                if (!workerResult.Success)
                {
                    _logger.LogWarning("FindAllElementsAsync failed after {ElapsedMs}ms: {Error}", 
                        stopwatch.ElapsedMilliseconds, workerResult.Error);
                    return new OperationResult<List<ElementInfo>>
                    {
                        Success = false,
                        Error = workerResult.Error ?? "Worker operation failed"
                    };
                }

                var elementInfos = ParseWorkerResultToElementInfoList(workerResult.Data);
                _logger.LogInformation("FindAllElementsAsync completed successfully in {ElapsedMs}ms, found {ElementCount} elements", 
                    stopwatch.ElapsedMilliseconds, elementInfos.Count);
                
                return new OperationResult<List<ElementInfo>>
                {
                    Success = true,
                    Data = elementInfos
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "FindAllElementsAsync operation failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                return new OperationResult<List<ElementInfo>>
                {
                    Success = false,
                    Error = $"FindAllElements operation failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<ElementInfo?>> FindFirstElementAsync(
            AutomationElement searchRoot,
            TreeScope scope,
            Condition condition,
            int timeoutSeconds = 15)
        {
            var startTime = DateTime.UtcNow;
            var searchRootName = SafeGetElementName(searchRoot);
            
            _logger.LogInformation("[UIAutomationWorker.FindFirstElementAsync] START: SearchRoot='{SearchRoot}', Scope={Scope}, Timeout={Timeout}s", 
                searchRootName, scope, timeoutSeconds);
            
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["SearchRootName"] = searchRootName ?? "",
                    ["SearchRootAutomationId"] = SafeGetElementAutomationId(searchRoot) ?? "",
                    ["SearchRootProcessId"] = SafeGetElementProcessId(searchRoot),
                    ["Scope"] = scope.ToString(),
                    ["ConditionType"] = condition.GetType().Name
                };

                AddConditionDetails(condition, parameters);

                var operation = new
                {
                    Operation = "findfirst",
                    Parameters = parameters,
                    Timeout = timeoutSeconds
                };

                var operationJson = JsonSerializer.Serialize(operation);
                var workerResult = await ExecuteInProcessAsync(operationJson, timeoutSeconds);

                var elapsed = DateTime.UtcNow - startTime;

                if (!workerResult.Success)
                {
                    _logger.LogWarning("[UIAutomationWorker.FindFirstElementAsync] Failed after {ElapsedMs}ms: {Error}", 
                        elapsed.TotalMilliseconds, workerResult.Error);
                    return new OperationResult<ElementInfo?>
                    {
                        Success = false,
                        Error = workerResult.Error ?? "Worker operation failed"
                    };
                }

                var elementInfo = ParseWorkerResultToElementInfo(workerResult.Data);
                _logger.LogInformation("[UIAutomationWorker.FindFirstElementAsync] COMPLETED in {ElapsedMs}ms. Found: {Found}", 
                    elapsed.TotalMilliseconds, elementInfo != null);
                
                return new OperationResult<ElementInfo?>
                {
                    Success = true,
                    Data = elementInfo
                };
            }
            catch (Exception ex)
            {
                var elapsed = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "[UIAutomationWorker.FindFirstElementAsync] ERROR after {ElapsedMs}ms", elapsed.TotalMilliseconds);
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = $"FindFirstElement operation failed: {ex.Message}"
                };
            }
        }

        public async Task<OperationResult<T>> ExecuteWithTimeoutAsync<T>(
            Func<Task<OperationResult<T>>> operation,
            string operationName,
            int timeoutSeconds = 30)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                
                var operationTask = operation();
                var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
                
                var completedTask = await Task.WhenAny(operationTask, timeoutTask);
                
                if (completedTask == operationTask)
                {
                    cts.Cancel();
                    return await operationTask;
                }
                else
                {
                    return new OperationResult<T>
                    {
                        Success = false,
                        Error = $"{operationName} timeout after {timeoutSeconds} seconds. Please try with more specific parameters or check if the target element is available."
                    };
                }
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<T>
                {
                    Success = false,
                    Error = $"{operationName} was cancelled after {timeoutSeconds} seconds."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{OperationName} failed", operationName);
                return new OperationResult<T>
                {
                    Success = false,
                    Error = $"{operationName} failed: {ex.Message}"
                };
            }
        }

        public OperationResult<T> SafeExecute<T>(
            Func<T> operation,
            string operationName,
            T? defaultValue = default)
        {
            try
            {
                var result = operation();
                return new OperationResult<T> { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{OperationName} failed, using default value", operationName);
                return new OperationResult<T> { Success = true, Data = defaultValue };
            }
        }

        private string SafeGetElementName(AutomationElement element)
        {
            try
            {
                return element?.Current.Name ?? "";
            }
            catch
            {
                return "";
            }
        }

        private string SafeGetElementAutomationId(AutomationElement element)
        {
            try
            {
                return element?.Current.AutomationId ?? "";
            }
            catch
            {
                return "";
            }
        }

        private int SafeGetElementProcessId(AutomationElement element)
        {
            try
            {
                return element?.Current.ProcessId ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private void AddConditionDetails(Condition condition, Dictionary<string, object> parameters)
        {
            try
            {
                switch (condition)
                {
                    case PropertyCondition propCondition:
                        parameters["PropertyName"] = propCondition.Property.ProgrammaticName;
                        parameters["PropertyValue"] = propCondition.Value?.ToString() ?? "";
                        break;
                    case AndCondition andCondition:
                        var andConditions = new List<Dictionary<string, object>>();
                        foreach (var subCondition in andCondition.GetConditions())
                        {
                            var subParams = new Dictionary<string, object>();
                            AddConditionDetails(subCondition, subParams);
                            andConditions.Add(subParams);
                        }
                        parameters["SubConditions"] = andConditions;
                        break;
                    case OrCondition orCondition:
                        var orConditions = new List<Dictionary<string, object>>();
                        foreach (var subCondition in orCondition.GetConditions())
                        {
                            var subParams = new Dictionary<string, object>();
                            AddConditionDetails(subCondition, subParams);
                            orConditions.Add(subParams);
                        }
                        parameters["SubConditions"] = orConditions;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add condition details");
            }
        }
    }

}
