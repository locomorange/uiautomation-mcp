using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessLaunchResponse> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<ProcessLaunchResponse> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<ProcessLaunchResponse> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : IApplicationLauncher
    {
        private readonly ILogger<ApplicationLauncher> _logger;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger)
        {
            _logger = logger;
        }

        public async Task<ProcessLaunchResponse> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Launching Win32 application: {ApplicationPath}", applicationPath);

                var startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    return ProcessLaunchResponse.CreateError("Failed to start process");
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

                _logger.LogInformation("Win32 process started: ProcessId={ProcessId}, ProcessName={ProcessName}", processId, processName);

                await Task.Delay(500, cancellationToken);
                var hasExited = process.HasExited;

                return ProcessLaunchResponse.CreateSuccess(processId, processName, hasExited);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching Win32 application: {ApplicationPath}", applicationPath);
                return ProcessLaunchResponse.CreateError(ex.Message);
            }
        }

        public async Task<ProcessLaunchResponse> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Launching UWP application: {AppsFolderPath}", appsFolderPath);

                if (!appsFolderPath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessLaunchResponse.CreateError("Invalid UWP app path. Must start with 'shell:AppsFolder\\'");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"{appsFolderPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    return ProcessLaunchResponse.CreateError("Failed to start process");
                }

                var processId = process.Id;
                _logger.LogInformation("UWP launch process started: ProcessId={ProcessId}", processId);

                await Task.Delay(1000, cancellationToken); // UWPは起動に時間がかかる場合がある
                var hasExited = process.HasExited;

                return ProcessLaunchResponse.CreateSuccess(processId, "UWP App", hasExited);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching UWP application: {AppsFolderPath}", appsFolderPath);
                return ProcessLaunchResponse.CreateError(ex.Message);
            }
        }

        public async Task<ProcessLaunchResponse> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Launching application by name: {ApplicationName}", applicationName);

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
                    return ProcessLaunchResponse.CreateError("Failed to start PowerShell search process");
                }

                var searchOutput = await searchProcess.StandardOutput.ReadToEndAsync();
                var searchError = await searchProcess.StandardError.ReadToEndAsync();
                await searchProcess.WaitForExitAsync(cancellationToken);

                _logger.LogInformation("Search output: {Output}, Error: {Error}, ExitCode: {ExitCode}", searchOutput, searchError, searchProcess.ExitCode);

                if (searchProcess.ExitCode != 0 || !string.IsNullOrEmpty(searchError))
                {
                    return ProcessLaunchResponse.CreateError($"Application '{applicationName}' not found. Search output: {searchOutput}, Error: {searchError}");
                }

                var appId = searchOutput.Trim();
                if (string.IsNullOrEmpty(appId))
                {
                    return ProcessLaunchResponse.CreateError($"Application '{applicationName}' not found or AppID is empty");
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
                    return ProcessLaunchResponse.CreateError("Failed to start launch process");
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

                return ProcessLaunchResponse.CreateSuccess(processId, processName, false, targetProcess?.MainWindowTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application by name: {ApplicationName}", applicationName);
                return ProcessLaunchResponse.CreateError(ex.Message);
            }
        }

    }
}
