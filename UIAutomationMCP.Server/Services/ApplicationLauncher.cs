using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<object> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<object> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<object> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : IApplicationLauncher
    {
        private readonly ILogger<ApplicationLauncher> _logger;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger)
        {
            _logger = logger;
        }

        public async Task<object> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting LaunchWin32Application with Path={applicationPath}, Arguments={arguments}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    UseShellExecute = true
                };

                _logger.LogInformationWithOperation(operationId, "Starting process");
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start process - Process.Start returned null");
                }

                var processId = process.Id;
                string processName;
                try
                {
                    processName = process.ProcessName;
                }
                catch (InvalidOperationException)
                {
                    processName = Path.GetFileNameWithoutExtension(applicationPath);
                }

                _logger.LogInformationWithOperation(operationId, $"Process started successfully: ProcessId={processId}, ProcessName={processName}");

                await Task.Delay(500, cancellationToken);
                var hasExited = process.HasExited;

                stopwatch.Stop();
                
                var response = ProcessLaunchResponse.CreateSuccess(processId, processName, hasExited);
                var serverResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = response.Success,
                    Data = response,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["processId"] = processId,
                            ["processName"] = processName,
                            ["hasExited"] = hasExited,
                            ["applicationPath"] = applicationPath,
                            ["arguments"] = arguments ?? "",
                            ["workingDirectory"] = workingDirectory ?? ""
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "LaunchWin32Application",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["applicationPath"] = applicationPath,
                            ["arguments"] = arguments ?? "",
                            ["workingDirectory"] = workingDirectory ?? "",
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in LaunchWin32Application operation");
                
                var errorResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["applicationPath"] = applicationPath
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "LaunchWin32Application",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["applicationPath"] = applicationPath,
                            ["arguments"] = arguments ?? "",
                            ["workingDirectory"] = workingDirectory ?? "",
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }

        public async Task<object> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting LaunchUWPApplication with Path={appsFolderPath}");

                if (!appsFolderPath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Invalid UWP app path. Must start with 'shell:AppsFolder\\'");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"{appsFolderPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _logger.LogInformationWithOperation(operationId, "Starting UWP launch process");
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start process - Process.Start returned null");
                }

                var processId = process.Id;
                _logger.LogInformationWithOperation(operationId, $"UWP launch process started: ProcessId={processId}");

                await Task.Delay(1000, cancellationToken); // UWPは起動に時間がかかる場合がある
                var hasExited = process.HasExited;

                stopwatch.Stop();
                
                var response = ProcessLaunchResponse.CreateSuccess(processId, "UWP App", hasExited);
                var serverResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = response.Success,
                    Data = response,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["processId"] = processId,
                            ["processName"] = "UWP App",
                            ["hasExited"] = hasExited,
                            ["appsFolderPath"] = appsFolderPath,
                            ["uwpLaunchDelay"] = "1000ms"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "LaunchUWPApplication",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["appsFolderPath"] = appsFolderPath,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in LaunchUWPApplication operation");
                
                var errorResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["appsFolderPath"] = appsFolderPath
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "LaunchUWPApplication",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["appsFolderPath"] = appsFolderPath,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }

        public async Task<object> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                _logger.LogInformationWithOperation(operationId, $"Starting LaunchApplicationByName with ApplicationName={applicationName}");

                // Step 1: アプリケーションを検索
                var searchStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"$app = Get-StartApps | Where-Object {{ $_.Name -eq '{applicationName}' }} | Select-Object -First 1; if ($app) {{ Write-Output $app.AppID }} else {{ Write-Error 'Application not found' }}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var searchProcess = Process.Start(searchStartInfo);
                if (searchProcess == null)
                {
                    throw new InvalidOperationException("Failed to start PowerShell search process");
                }

                var searchOutput = await searchProcess.StandardOutput.ReadToEndAsync();
                var searchError = await searchProcess.StandardError.ReadToEndAsync();
                await searchProcess.WaitForExitAsync(cancellationToken);

                _logger.LogInformation("Search output: {Output}, Error: {Error}, ExitCode: {ExitCode}", searchOutput, searchError, searchProcess.ExitCode);

                if (searchProcess.ExitCode != 0 || !string.IsNullOrEmpty(searchError))
                {
                    throw new InvalidOperationException($"Application '{applicationName}' not found. Search output: {searchOutput}, Error: {searchError}");
                }

                var appId = searchOutput.Trim();
                if (string.IsNullOrEmpty(appId))
                {
                    throw new InvalidOperationException($"Application '{applicationName}' not found or AppID is empty");
                }

                _logger.LogInformation("Found application: {ApplicationName} with AppID: {AppID}", applicationName, appId);

                // Step 2: アプリケーションを起動
                var launchStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"start 'shell:AppsFolder\\{appId}'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var launchProcess = Process.Start(launchStartInfo);
                if (launchProcess == null)
                {
                    throw new InvalidOperationException("Failed to start launch process");
                }

                _logger.LogInformation("Application launched by name: {ApplicationName}", applicationName);

                await Task.Delay(2000, cancellationToken); // アプリケーションが起動するまで待機

                // 起動したプロセスを検索
                var processId = 0;
                var processName = applicationName;
                Process? targetProcess = null;
                
                try
                {
                    // アプリケーション名の一部でプロセスを検索
                    var processes = Process.GetProcesses();
                    var searchTerms = applicationName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    
                    // より良いマッチングのために優先順位を付けて検索
                    targetProcess = processes
                        .Where(p => 
                        {
                            try
                            {
                                return searchTerms.Any(term => 
                                    p.ProcessName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                    (!string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowTitle.Contains(term, StringComparison.OrdinalIgnoreCase)));
                            }
                            catch
                            {
                                return false;
                            }
                        })
                        .OrderByDescending(p => 
                        {
                            try
                            {
                                var score = 0;
                                
                                // ウィンドウタイトルに完全なアプリケーション名が含まれるものを最優先
                                if (!string.IsNullOrEmpty(p.MainWindowTitle) && 
                                    p.MainWindowTitle.Contains(applicationName, StringComparison.OrdinalIgnoreCase))
                                    score += 100;
                                
                                // 検索語数が多くマッチするほど高スコア
                                var matchedTerms = searchTerms.Count(term => 
                                    p.ProcessName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                    (!string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowTitle.Contains(term, StringComparison.OrdinalIgnoreCase)));
                                score += matchedTerms * 10;
                                
                                // プロセス名に検索語が含まれるものを優先
                                if (searchTerms.Any(term => p.ProcessName.Contains(term, StringComparison.OrdinalIgnoreCase)))
                                    score += 5;
                                
                                return score;
                            }
                            catch
                            {
                                return 0;
                            }
                        })
                        .FirstOrDefault();
                    
                    if (targetProcess != null)
                    {
                        processId = targetProcess.Id;
                        processName = targetProcess.ProcessName;
                        _logger.LogInformation("Found launched process: {ProcessName} with ID: {ProcessId}, WindowTitle: {WindowTitle}", 
                            processName, processId, targetProcess.MainWindowTitle ?? "N/A");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to find launched process for {ApplicationName}", applicationName);
                }

                stopwatch.Stop();
                
                var response = ProcessLaunchResponse.CreateSuccess(processId, processName, false, targetProcess?.MainWindowTitle);
                var serverResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = response.Success,
                    Data = response,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["processId"] = processId,
                            ["processName"] = processName,
                            ["hasExited"] = false,
                            ["applicationName"] = applicationName,
                            ["windowTitle"] = targetProcess?.MainWindowTitle ?? "N/A",
                            ["searchDelay"] = "2000ms"
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "LaunchApplicationByName",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["applicationName"] = applicationName,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };

                var jsonString = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(serverResponse);
                
                _logger.LogInformationWithOperation(operationId, $"Successfully serialized enhanced response (length: {jsonString.Length})");
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return jsonString;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogErrorWithOperation(operationId, ex, "Error in LaunchApplicationByName operation");
                
                var errorResponse = new ServerEnhancedResponse<ProcessLaunchResponse>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExecutionInfo = new ServerExecutionInfo
                    {
                        ServerProcessingTime = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff"),
                        OperationId = operationId,
                        ServerLogs = LogCollectorExtensions.Instance.GetLogs(operationId),
                        AdditionalInfo = new Dictionary<string, object>
                        {
                            ["exceptionType"] = ex.GetType().Name,
                            ["stackTrace"] = ex.StackTrace ?? "",
                            ["applicationName"] = applicationName
                        }
                    },
                    RequestMetadata = new RequestMetadata
                    {
                        RequestedMethod = "LaunchApplicationByName",
                        RequestParameters = new Dictionary<string, object>
                        {
                            ["applicationName"] = applicationName,
                            ["timeoutSeconds"] = timeoutSeconds
                        },
                        TimeoutSeconds = timeoutSeconds
                    }
                };
                
                var errorJson = UIAutomationMCP.Shared.Serialization.JsonSerializationHelper.Serialize(errorResponse);
                
                LogCollectorExtensions.Instance.ClearLogs(operationId);
                
                return errorJson;
            }
        }

    }
}
