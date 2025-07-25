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
                var processInfo = new ProcessStartInfo
                {
                    FileName = applicationName,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                if (process == null)
                    return ProcessLaunchResponse.CreateError($"Failed to launch application: {applicationName}");

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
                _logger.LogError(ex, "Error launching application '{ApplicationName}'", applicationName);
                return ProcessLaunchResponse.CreateError($"Error launching application '{applicationName}': {ex.Message}");
            }
        }
    }
}