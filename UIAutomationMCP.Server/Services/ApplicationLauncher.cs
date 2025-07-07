using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessResult> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<ProcessResult> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
        Task<ProcessResult> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : IApplicationLauncher
    {
        private readonly ILogger<ApplicationLauncher> _logger;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger)
        {
            _logger = logger;
        }

        public async Task<ProcessResult> LaunchWin32ApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
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
                    return new ProcessResult { Success = false, Error = "Failed to start process" };
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

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = processId,
                    ProcessName = processName,
                    HasExited = hasExited
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching Win32 application: {ApplicationPath}", applicationPath);
                return new ProcessResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<ProcessResult> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Launching UWP application: {AppsFolderPath}", appsFolderPath);

                if (!appsFolderPath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase))
                {
                    return new ProcessResult { Success = false, Error = "Invalid UWP app path. Must start with 'shell:AppsFolder\\'" };
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
                    return new ProcessResult { Success = false, Error = "Failed to start process" };
                }

                var processId = process.Id;
                _logger.LogInformation("UWP launch process started: ProcessId={ProcessId}", processId);

                await Task.Delay(1000, cancellationToken); // UWPは起動に時間がかかる場合がある
                var hasExited = process.HasExited;

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = processId,
                    ProcessName = "UWP App",
                    HasExited = hasExited
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching UWP application: {AppsFolderPath}", appsFolderPath);
                return new ProcessResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<ProcessResult> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Launching application by name: {ApplicationName}", applicationName);

                // PowerShellを使用してアプリケーションを名前で検索・起動
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"$app = Get-StartApps | Where-Object {{ $_.Name -eq '{applicationName}' }} | Select-Object -First 1; if ($app) {{ Start-Process -FilePath 'shell:AppsFolder\\$($app.AppID)' }} else {{ Write-Error 'Application not found' }}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    return new ProcessResult { Success = false, Error = "Failed to start PowerShell process" };
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                {
                    return new ProcessResult { Success = false, Error = $"Application '{applicationName}' not found or failed to launch" };
                }

                _logger.LogInformation("Application launched by name: {ApplicationName}", applicationName);

                await Task.Delay(1000, cancellationToken); // アプリケーションが起動するまで待機

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = 0, // 実際のプロセスIDは取得困難
                    ProcessName = applicationName,
                    HasExited = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application by name: {ApplicationName}", applicationName);
                return new ProcessResult { Success = false, Error = ex.Message };
            }
        }

    }
}
