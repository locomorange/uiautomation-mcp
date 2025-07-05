using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationMcpServer.Services
{
    /// <summary>
    /// Worker service for executing UI Automation operations in a separate process
    /// to prevent main process from hanging due to COM/native API blocking
    /// </summary>
    public interface IUIAutomationWorker : IDisposable
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

        // 新しいパラメータベースのメソッド（UIAutomation依存なし）
        Task<OperationResult<List<ElementInfo>>> FindAllElementsAsync(
            ElementSearchParameters searchParams,
            int timeoutSeconds = 60);

        Task<OperationResult<ElementInfo?>> FindFirstElementAsync(
            ElementSearchParameters searchParams,
            int timeoutSeconds = 15);

        Task<OperationResult<Dictionary<string, object>>> ExecuteAdvancedOperationAsync(
            AdvancedOperationParameters operationParams);

        // Range Value Pattern methods
        Task<OperationResult<string>> SetRangeValueAsync(
            string elementId,
            double value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<Dictionary<string, object>>> GetRangeValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // Text Pattern methods
        Task<OperationResult<string>> GetTextAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<string>> SelectTextAsync(
            string elementId,
            int startIndex,
            int length,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // Tree Operations
        Task<OperationResult<Dictionary<string, object>>> GetElementTreeAsync(
            string? windowTitle = null,
            int? processId = null,
            int maxDepth = 3,
            int timeoutSeconds = 30);

        Task<OperationResult<List<Dictionary<string, object>>>> GetElementChildrenAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // Window state operations  
        Task<OperationResult<string>> SetWindowStateAsync(
            string elementId,
            string state,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // Additional Text Pattern methods
        Task<OperationResult<Dictionary<string, object>>> FindTextAsync(
            string elementId,
            string searchText,
            bool backward = false,
            bool ignoreCase = false,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        Task<OperationResult<List<Dictionary<string, object>>>> GetTextSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20);

        // Window service methods
        Task<OperationResult<List<WindowInfo>>> GetWindowsAsync(
            int timeoutSeconds = 30);
        
        Task<OperationResult<List<WindowInfo>>> GetWindowInfoAsync(
            int timeoutSeconds = 30);
            
        Task<OperationResult<ElementInfo?>> FindWindowByTitleAsync(
            string title,
            int? processId = null,
            int timeoutSeconds = 30);
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

        // Missing basic methods implementation
        public async Task<OperationResult<string>> SetElementValueAsync(
            string elementId,
            string value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "setelementvalue",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> { ["Value"] = value }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element value set to: {value}" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> GetElementValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "getelementvalue",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success && result.Data != null ? result.Data.ToString() : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> InvokeElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "invokeelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element invoked successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> ToggleElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "toggleelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element toggled successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> SelectElementAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "selectelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element selected successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> ExpandCollapseElementAsync(
            string elementId,
            bool? expand = null,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "expandcollapseelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> { ["Expand"] = expand ?? true }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element {(expand == true ? "expanded" : "collapsed")} successfully" : null,
                Error = result.Error
            };
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
            var parameters = new AdvancedOperationParameters
            {
                Operation = "scrollelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> 
                { 
                    ["Direction"] = direction ?? "down",
                    ["Horizontal"] = horizontal ?? 0.0,
                    ["Vertical"] = vertical ?? 0.0
                }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element scrolled successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> ScrollElementIntoViewAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "scrollelementintoview",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Element scrolled into view successfully" : null,
                Error = result.Error
            };
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
            var parameters = new AdvancedOperationParameters
            {
                Operation = "transformelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> 
                { 
                    ["Action"] = action,
                    ["X"] = x ?? 0,
                    ["Y"] = y ?? 0,
                    ["Width"] = width ?? 0,
                    ["Height"] = height ?? 0,
                    ["Degrees"] = degrees ?? 0
                }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element transformed ({action}) successfully" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> DockElementAsync(
            string elementId,
            string position,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "dockelement",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> { ["Position"] = position }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Element docked to {position}" : null,
                Error = result.Error
            };
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
                    // Only kill if process is still running and didn't exit normally
                    if (workerProcess != null && !workerProcess.HasExited)
                    {
                        _logger.LogWarning("[UIAutomationWorker] Worker process still running, terminating");
                        workerProcess.Kill(entireProcessTree: true);
                    }
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

        // 新しいパラメータベースのメソッド（UIAutomation依存なし）
        public async Task<OperationResult<List<ElementInfo>>> FindAllElementsAsync(
            ElementSearchParameters searchParams,
            int timeoutSeconds = 60)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("FindAllElementsAsync started with parameters - ElementId: {ElementId}, WindowTitle: {WindowTitle}, ProcessId: {ProcessId}, Timeout: {TimeoutSeconds}s",
                searchParams.ElementId, searchParams.WindowTitle, searchParams.ProcessId, timeoutSeconds);
            
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["ElementId"] = searchParams.ElementId ?? "",
                    ["WindowTitle"] = searchParams.WindowTitle ?? "",
                    ["ProcessId"] = searchParams.ProcessId ?? 0,
                    ["ControlType"] = searchParams.ControlType ?? "",
                    ["TreeScope"] = searchParams.TreeScope ?? "descendants",
                    ["SearchRootId"] = searchParams.SearchRootId ?? "",
                    ["SearchText"] = searchParams.SearchText ?? "",
                    ["Conditions"] = searchParams.Conditions ?? new Dictionary<string, object>()
                };

                var operation = new
                {
                    Operation = "findall_advanced",
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
            ElementSearchParameters searchParams,
            int timeoutSeconds = 15)
        {
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation("[UIAutomationWorker.FindFirstElementAsync] START: ElementId='{ElementId}', WindowTitle='{WindowTitle}', Timeout={Timeout}s", 
                searchParams.ElementId, searchParams.WindowTitle, timeoutSeconds);
            
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["ElementId"] = searchParams.ElementId ?? "",
                    ["WindowTitle"] = searchParams.WindowTitle ?? "",
                    ["ProcessId"] = searchParams.ProcessId ?? 0,
                    ["ControlType"] = searchParams.ControlType ?? "",
                    ["TreeScope"] = searchParams.TreeScope ?? "descendants",
                    ["SearchRootId"] = searchParams.SearchRootId ?? "",
                    ["SearchText"] = searchParams.SearchText ?? "",
                    ["Conditions"] = searchParams.Conditions ?? new Dictionary<string, object>()
                };

                var operation = new
                {
                    Operation = "findfirst_advanced",
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

        // 高度な操作を実行する汎用メソッド
        public async Task<OperationResult<Dictionary<string, object>>> ExecuteAdvancedOperationAsync(
            AdvancedOperationParameters operationParams)
        {
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation("[UIAutomationWorker.ExecuteAdvancedOperationAsync] START: Operation='{Operation}', ElementId='{ElementId}', WindowTitle='{WindowTitle}'", 
                operationParams.Operation, operationParams.ElementId, operationParams.WindowTitle);
            
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["ElementId"] = operationParams.ElementId ?? "",
                    ["WindowTitle"] = operationParams.WindowTitle ?? "",
                    ["ProcessId"] = operationParams.ProcessId ?? 0
                };

                // 追加パラメータをマージ
                foreach (var param in operationParams.Parameters)
                {
                    parameters[param.Key] = param.Value;
                }

                var operation = new
                {
                    Operation = operationParams.Operation,
                    Parameters = parameters,
                    Timeout = operationParams.TimeoutSeconds
                };

                var operationJson = JsonSerializer.Serialize(operation);
                var workerResult = await ExecuteInProcessAsync(operationJson, operationParams.TimeoutSeconds);

                var elapsed = DateTime.UtcNow - startTime;

                if (!workerResult.Success)
                {
                    _logger.LogWarning("[UIAutomationWorker.ExecuteAdvancedOperationAsync] FAILED after {ElapsedMs}ms: {Error}", 
                        elapsed.TotalMilliseconds, workerResult.Error);
                    return new OperationResult<Dictionary<string, object>>
                    {
                        Success = false,
                        Error = workerResult.Error ?? "Worker operation failed"
                    };
                }

                var resultData = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(workerResult.Data))
                {
                    try
                    {
                        var parsedData = JsonSerializer.Deserialize<Dictionary<string, object>>(workerResult.Data);
                        if (parsedData != null)
                        {
                            resultData = parsedData;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse worker result data as Dictionary");
                        resultData["rawData"] = workerResult.Data;
                    }
                }

                _logger.LogInformation("[UIAutomationWorker.ExecuteAdvancedOperationAsync] SUCCESS after {ElapsedMs}ms",
                    elapsed.TotalMilliseconds);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = true,
                    Data = resultData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UIAutomationWorker.ExecuteAdvancedOperationAsync] EXCEPTION: {Message}", ex.Message);
                return new OperationResult<Dictionary<string, object>>
                {
                    Success = false,
                    Error = $"Worker execution failed: {ex.Message}"
                };
            }
        }

        // Range Value Pattern methods implementation
        public async Task<OperationResult<string>> SetRangeValueAsync(
            string elementId,
            double value,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "setrangevalue",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> { ["Value"] = value }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Range value set to {value}" : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<Dictionary<string, object>>> GetRangeValueAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "getrangevalue",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            return await ExecuteAdvancedOperationAsync(parameters);
        }

        // Text Pattern methods implementation
        public async Task<OperationResult<string>> GetTextAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "gettext",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success && result.Data != null ? result.Data.ToString() : null,
                Error = result.Error
            };
        }

        public async Task<OperationResult<string>> SelectTextAsync(
            string elementId,
            int startIndex,
            int length,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "selecttext",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> 
                { 
                    ["StartIndex"] = startIndex,
                    ["Length"] = length
                }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? "Text selected successfully" : null,
                Error = result.Error
            };
        }

        // Tree Operations implementation
        public async Task<OperationResult<Dictionary<string, object>>> GetElementTreeAsync(
            string? windowTitle = null,
            int? processId = null,
            int maxDepth = 3,
            int timeoutSeconds = 30)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "getelementtree",
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> { ["MaxDepth"] = maxDepth }
            };

            return await ExecuteAdvancedOperationAsync(parameters);
        }

        public async Task<OperationResult<List<Dictionary<string, object>>>> GetElementChildrenAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "getelementchildren",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<List<Dictionary<string, object>>>
            {
                Success = result.Success,
                Data = result.Success ? [result.Data ?? new Dictionary<string, object>()] : null,
                Error = result.Error
            };
        }

        // Window state operations implementation
        public async Task<OperationResult<string>> SetWindowStateAsync(
            string elementId,
            string state,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "setwindowstate",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> { ["State"] = state }
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<string>
            {
                Success = result.Success,
                Data = result.Success ? $"Window state set to {state}" : null,
                Error = result.Error
            };
        }

        // Additional Text Pattern methods implementation
        public async Task<OperationResult<Dictionary<string, object>>> FindTextAsync(
            string elementId,
            string searchText,
            bool backward = false,
            bool ignoreCase = false,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "findtext",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object> 
                { 
                    ["SearchText"] = searchText,
                    ["Backward"] = backward,
                    ["IgnoreCase"] = ignoreCase
                }
            };

            return await ExecuteAdvancedOperationAsync(parameters);
        }

        public async Task<OperationResult<List<Dictionary<string, object>>>> GetTextSelectionAsync(
            string elementId,
            string? windowTitle = null,
            int? processId = null,
            int timeoutSeconds = 20)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "gettextselection",
                ElementId = elementId,
                WindowTitle = windowTitle,
                ProcessId = processId,
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            return new OperationResult<List<Dictionary<string, object>>>
            {
                Success = result.Success,
                Data = result.Success ? [result.Data ?? new Dictionary<string, object>()] : null,
                Error = result.Error
            };
        }

        // Window service methods implementation
        public async Task<OperationResult<List<WindowInfo>>> GetWindowsAsync(
            int timeoutSeconds = 30)
        {
            var parameters = new AdvancedOperationParameters
            {
                Operation = "getwindows",
                TimeoutSeconds = timeoutSeconds,
                Parameters = new Dictionary<string, object>()
            };

            var result = await ExecuteAdvancedOperationAsync(parameters);
            if (result.Success && result.Data != null)
            {
                try
                {
                    var windowsList = JsonSerializer.Deserialize<List<WindowInfo>>(
                        JsonSerializer.Serialize(result.Data)) ?? new List<WindowInfo>();
                    
                    return new OperationResult<List<WindowInfo>>
                    {
                        Success = true,
                        Data = windowsList
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize windows data");
                    return new OperationResult<List<WindowInfo>>
                    {
                        Success = false,
                        Error = $"Failed to parse windows data: {ex.Message}"
                    };
                }
            }

            return new OperationResult<List<WindowInfo>>
            {
                Success = false,
                Error = result.Error ?? "Unknown error getting windows"
            };
        }

        public async Task<OperationResult<List<WindowInfo>>> GetWindowInfoAsync(
            int timeoutSeconds = 30)
        {
            var windowElementsResult = await FindAllAsync(
                windowTitle: null,
                searchText: null,
                controlType: "Window",
                processId: null,
                timeoutSeconds: timeoutSeconds);
            
            if (!windowElementsResult.Success)
            {
                return new OperationResult<List<WindowInfo>>
                { 
                    Success = false, 
                    Error = windowElementsResult.Error ?? "Failed to find windows" 
                };
            }
            
            var elementInfos = windowElementsResult.Data ?? new List<ElementInfo>();
            var windows = new List<WindowInfo>();
            
            foreach (var elementInfo in elementInfos)
            {
                var windowInfo = new WindowInfo
                {
                    Name = elementInfo.Name,
                    AutomationId = elementInfo.AutomationId,
                    ProcessId = elementInfo.ProcessId,
                    ClassName = elementInfo.ClassName,
                    BoundingRectangle = elementInfo.BoundingRectangle,
                    IsEnabled = elementInfo.IsEnabled,
                    IsVisible = elementInfo.IsVisible
                };
                
                windows.Add(windowInfo);
            }
            
            return new OperationResult<List<WindowInfo>> 
            { 
                Success = true, 
                Data = windows 
            };
        }

        public async Task<OperationResult<ElementInfo?>> FindWindowByTitleAsync(
            string title,
            int? processId = null,
            int timeoutSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = "Window title cannot be empty"
                };
            }
            
            var searchParams = new ElementSearchParameters
            {
                SearchText = title,
                ControlType = "Window",
                ProcessId = processId,
                TreeScope = "children"
            };

            var result = await FindFirstElementAsync(searchParams, timeoutSeconds: 15);
            
            if (!result.Success || result.Data == null)
            {
                var allWindowsResult = await FindAllElementsAsync(new ElementSearchParameters
                {
                    ControlType = "Window",
                    ProcessId = processId,
                    TreeScope = "children"
                }, timeoutSeconds: 15);
                
                if (allWindowsResult.Success && allWindowsResult.Data != null)
                {
                    var matchingWindow = allWindowsResult.Data
                        .Where(w => !string.IsNullOrEmpty(w.Name))
                        .FirstOrDefault(w => 
                            string.Equals(w.Name.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase) ||
                            w.Name.Contains(title, StringComparison.OrdinalIgnoreCase) ||
                            title.Contains(w.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingWindow != null)
                    {
                        return new OperationResult<ElementInfo?>
                        {
                            Success = true,
                            Data = matchingWindow
                        };
                    }
                }
                
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = $"Window with title '{title}' not found"
                };
            }
            
            return result;
        }

        #region IDisposable Implementation

        private bool _disposed = false;

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
                    // Cleanup managed resources if any
                    _logger?.LogDebug("UIAutomationWorker disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
