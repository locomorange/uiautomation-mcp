using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using System.Diagnostics;

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
            // applicationPath can be either a full path to an executable or just an application name (e.g., "notepad", "calc")
            if (string.IsNullOrWhiteSpace(applicationPath))
                return ProcessLaunchResponse.CreateError("ApplicationPath is required");

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                if (process == null)
                    return ProcessLaunchResponse.CreateError($"Failed to launch application: {applicationPath}");

                await Task.Delay(1000, cancellationToken);

                string windowTitle = "";
                try
                {
                    if (!process.HasExited && process.MainWindowHandle != IntPtr.Zero)
                        windowTitle = process.MainWindowTitle;
                }
                catch { /* Ignore window title errors */ }

                return ProcessLaunchResponse.CreateSuccess(process.Id, process.ProcessName, process.HasExited, windowTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching Win32 application '{ApplicationPath}'", applicationPath);
                return ProcessLaunchResponse.CreateError($"Error launching application '{applicationPath}': {ex.Message}");
            }
        }

        public async Task<ProcessLaunchResponse> LaunchUWPApplicationAsync(string appsFolderPath, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(appsFolderPath))
                return ProcessLaunchResponse.CreateError("AppsFolderPath is required");

            if (!appsFolderPath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase) && 
                !appsFolderPath.StartsWith("shell:AppsFolder/", StringComparison.OrdinalIgnoreCase))
                return ProcessLaunchResponse.CreateError("Invalid UWP app path. Must start with 'shell:AppsFolder\\'");

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = appsFolderPath,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                if (process == null)
                    return ProcessLaunchResponse.CreateError($"Failed to launch UWP application: {appsFolderPath}");

                await Task.Delay(500, cancellationToken);

                return ProcessLaunchResponse.CreateSuccess(process.Id, "UWP App", process.HasExited, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching UWP application '{AppsFolderPath}'", appsFolderPath);
                return ProcessLaunchResponse.CreateError($"Error launching UWP application '{appsFolderPath}': {ex.Message}");
            }
        }

        public async Task<ProcessLaunchResponse> LaunchApplicationByNameAsync(string applicationName, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                return ProcessLaunchResponse.CreateError("ApplicationName is required");

            try
            {
                // Use PowerShell to search for the application
                var searchScript = $"Get-StartApps | Where-Object {{ $_.Name -like '*{applicationName}*' }} | Select-Object -First 1 -ExpandProperty AppID";
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{searchScript}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var searchProcess = Process.Start(processInfo);
                if (searchProcess == null)
                    return ProcessLaunchResponse.CreateError("Failed to start PowerShell search");

                var appId = await searchProcess.StandardOutput.ReadToEndAsync();
                await searchProcess.WaitForExitAsync(cancellationToken);

                if (searchProcess.ExitCode != 0 || string.IsNullOrWhiteSpace(appId))
                    return ProcessLaunchResponse.CreateError($"Application '{applicationName}' not found");

                appId = appId.Trim();

                // Launch the application using the found AppID
                var launchInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"shell:AppsFolder\\{appId}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(launchInfo);
                if (process == null)
                    return ProcessLaunchResponse.CreateError($"Failed to launch application: {applicationName}");

                await Task.Delay(1000, cancellationToken);

                return ProcessLaunchResponse.CreateSuccess(process.Id, applicationName, process.HasExited, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application by name '{ApplicationName}'", applicationName);
                return ProcessLaunchResponse.CreateError($"Error launching application '{applicationName}': {ex.Message}");
            }
        }
    }
}