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
        Task<ProcessLaunchResponse> LaunchApplicationAsync(string application, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
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
                // Extract executable name from UWP path
                var executableName = await ExtractExecutableNameAsync(appsFolderPath, cancellationToken);
                if (string.IsNullOrWhiteSpace(executableName))
                    return ProcessLaunchResponse.CreateError($"Could not extract executable name from UWP path: {appsFolderPath}");

                var processInfo = new ProcessStartInfo
                {
                    FileName = executableName,
                    Arguments = "",
                    WorkingDirectory = "",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                if (process == null)
                    return ProcessLaunchResponse.CreateError($"Failed to launch UWP application: {executableName}");

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

                // Extract executable name from the found AppID
                var appsFolderPath = $"shell:AppsFolder\\{appId}";
                var executableName = await ExtractExecutableNameAsync(appsFolderPath, cancellationToken);
                if (string.IsNullOrWhiteSpace(executableName))
                    return ProcessLaunchResponse.CreateError($"Could not extract executable name for application: {applicationName}");

                var launchInfo = new ProcessStartInfo
                {
                    FileName = executableName,
                    Arguments = "",
                    WorkingDirectory = "",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                var process = Process.Start(launchInfo);
                if (process == null)
                    return ProcessLaunchResponse.CreateError($"Failed to launch application: {executableName}");

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
                _logger.LogError(ex, "Error launching application by name '{ApplicationName}'", applicationName);
                return ProcessLaunchResponse.CreateError($"Error launching application '{applicationName}': {ex.Message}");
            }
        }

        private async Task<string> GetUWPExecutableAsync(string packageName, CancellationToken cancellationToken = default)
        {
            try
            {
                var script = $@"
                    $pkg = Get-AppxPackage '{packageName}';
                    if($pkg) {{
                        $manifest = Get-AppxPackageManifest $pkg;
                        $app = $manifest.Package.Applications.Application;
                        if($app.Executable) {{
                            Write-Output $app.Executable
                        }}
                    }}
                ";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    _logger.LogWarning("Failed to start PowerShell to get UWP executable for package: {PackageName}", packageName);
                    return string.Empty;
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    var executable = output.Trim();
                    // Remove .exe extension if present for consistency
                    if (executable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        executable = executable.Substring(0, executable.Length - 4);
                    }
                    
                    _logger.LogDebug("Retrieved UWP executable for {PackageName}: {Executable}", packageName, executable);
                    return executable;
                }

                _logger.LogWarning("Failed to retrieve UWP executable for package: {PackageName}", packageName);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UWP executable for package: {PackageName}", packageName);
                return string.Empty;
            }
        }

        private async Task<string> ExtractExecutableNameAsync(string appsFolderPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(appsFolderPath))
                return string.Empty;

            // Extract from shell:AppsFolder\PackageName!AppId format
            // Examples:
            // shell:AppsFolder\Microsoft.WindowsCalculator_8wekyb3d8bbwe!App -> CalculatorApp
            // shell:AppsFolder\Microsoft.Windows.Photos_8wekyb3d8bbwe!App -> Photos
            
            var appsFolderPrefix = "shell:AppsFolder\\";
            if (!appsFolderPath.StartsWith(appsFolderPrefix, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var packageInfo = appsFolderPath.Substring(appsFolderPrefix.Length);
            var exclamationIndex = packageInfo.IndexOf('!');
            
            if (exclamationIndex == -1)
                return string.Empty;

            var packageName = packageInfo.Substring(0, exclamationIndex);
            
            // Try to get the executable name dynamically from UWP package manifest
            var executableName = await GetUWPExecutableAsync(packageName, cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(executableName))
            {
                return executableName;
            }

            // Fallback: extract a reasonable name from the package identifier
            var parts = packageName.Split('.');
            if (parts.Length >= 2)
            {
                // Use the last part, removing version suffixes
                var appName = parts[^1];
                var underscoreIndex = appName.IndexOf('_');
                if (underscoreIndex > 0)
                {
                    appName = appName.Substring(0, underscoreIndex);
                }
                return appName;
            }

            return packageName;
        }

        public async Task<ProcessLaunchResponse> LaunchApplicationAsync(string application, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(application))
                return ProcessLaunchResponse.CreateError("Application is required");

            try
            {
                // Get baseline processes before launching
                var beforeProcesses = GetRelevantProcesses(application);
                
                // Step 1: Try Win32 application launch
                var win32Result = await TryLaunchWin32(application, arguments, workingDirectory, beforeProcesses, cancellationToken);
                if (win32Result.Success)
                {
                    _logger.LogInformation("Successfully launched Win32 application: {Application}, PID: {ProcessId}", application, win32Result.ProcessId);
                    return win32Result;
                }

                // Step 2: Try UWP application launch
                var uwpResult = await TryLaunchUWP(application, beforeProcesses, cancellationToken);
                if (uwpResult.Success)
                {
                    _logger.LogInformation("Successfully launched UWP application: {Application}, PID: {ProcessId}", application, uwpResult.ProcessId);
                    return uwpResult;
                }

                return ProcessLaunchResponse.CreateError($"Failed to launch application: {application}. Tried Win32 and UWP methods.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application: {Application}", application);
                return ProcessLaunchResponse.CreateError($"Exception during launch: {ex.Message}");
            }
        }

        private HashSet<int> GetRelevantProcesses(string appName)
        {
            try
            {
                var relevantProcesses = new HashSet<int>();
                var processes = Process.GetProcesses();
                
                foreach (var process in processes)
                {
                    try
                    {
                        if (IsRelevantProcess(process, appName))
                        {
                            relevantProcesses.Add(process.Id);
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
                
                return relevantProcesses;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get baseline processes for {AppName}", appName);
                return new HashSet<int>();
            }
        }

        private bool IsRelevantProcess(Process process, string appName)
        {
            try
            {
                if (process.HasExited) return false;
                
                var processName = process.ProcessName;
                var cleanAppName = Path.GetFileNameWithoutExtension(appName);
                
                return processName.Contains(cleanAppName, StringComparison.OrdinalIgnoreCase) ||
                       processName.Contains(appName, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private async Task<ProcessLaunchResponse> TryLaunchWin32(string application, string? arguments, string? workingDirectory, HashSet<int> beforeProcesses, CancellationToken cancellationToken)
        {
            try
            {
                // Check if it's a full path
                if (Path.IsPathFullyQualified(application) && File.Exists(application))
                {
                    return await LaunchWin32Process(application, arguments, workingDirectory, beforeProcesses, cancellationToken);
                }

                // Try to find executable using 'where' command
                var executablePath = await FindExecutablePath(application, cancellationToken);
                if (!string.IsNullOrEmpty(executablePath))
                {
                    return await LaunchWin32Process(executablePath, arguments, workingDirectory, beforeProcesses, cancellationToken);
                }

                return ProcessLaunchResponse.CreateError("Win32 executable not found");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Win32 launch failed for {Application}", application);
                return ProcessLaunchResponse.CreateError($"Win32 launch failed: {ex.Message}");
            }
        }

        private async Task<string> FindExecutablePath(string application, CancellationToken cancellationToken)
        {
            try
            {
                // Common system32 applications
                var commonPaths = new[]
                {
                    $"C:\\Windows\\System32\\{application}.exe",
                    $"C:\\Windows\\System32\\{application}",
                    $"C:\\Windows\\{application}.exe",
                    $"C:\\Windows\\{application}"
                };

                foreach (var path in commonPaths)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

                // Try using cmd.exe with proper path
                var processInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Windows\\System32\\cmd.exe",
                    Arguments = $"/c where {application}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return string.Empty;

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    var firstPath = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                    if (!string.IsNullOrEmpty(firstPath) && File.Exists(firstPath))
                    {
                        return firstPath;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to find executable path for {Application}", application);
                return string.Empty;
            }
        }

        private async Task<ProcessLaunchResponse> LaunchWin32Process(string executablePath, string? arguments, string? workingDirectory, HashSet<int> beforeProcesses, CancellationToken cancellationToken)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments ?? "",
                WorkingDirectory = workingDirectory ?? "",
                UseShellExecute = false
            };

            var process = Process.Start(processInfo);
            if (process != null)
            {
                return ProcessLaunchResponse.CreateSuccess(process.Id, Path.GetFileNameWithoutExtension(executablePath), process.HasExited);
            }

            // Fallback: Find new process using diff
            var appName = Path.GetFileNameWithoutExtension(executablePath);
            var newProcessId = await FindNewProcess(appName, beforeProcesses, 5000, cancellationToken);
            if (newProcessId > 0)
            {
                return ProcessLaunchResponse.CreateSuccess(newProcessId, appName, false);
            }

            return ProcessLaunchResponse.CreateError("Failed to start Win32 process");
        }

        private async Task<ProcessLaunchResponse> TryLaunchUWP(string application, HashSet<int> beforeProcesses, CancellationToken cancellationToken)
        {
            try
            {
                // Use Get-StartApps to find UWP application
                var script = $@"
                    Get-StartApps | Where-Object {{ 
                        $_.Name -like '*{application}*' -or 
                        $_.AppID -like '*{application}*' 
                    }} | Select-Object -First 1 -ExpandProperty AppID";

                var appId = await RunPowerShellScript(script, cancellationToken);
                if (string.IsNullOrWhiteSpace(appId))
                {
                    return ProcessLaunchResponse.CreateError("UWP application not found");
                }

                // Launch via shell:AppsFolder
                var explorerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"shell:AppsFolder\\{appId}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                });

                // Find the actual UWP process using diff
                var newProcessId = await FindNewProcess(application, beforeProcesses, 8000, cancellationToken);
                if (newProcessId > 0)
                {
                    return ProcessLaunchResponse.CreateSuccess(newProcessId, application, false, appId);
                }

                return ProcessLaunchResponse.CreateError("UWP application launched but process not found");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "UWP launch failed for {Application}", application);
                return ProcessLaunchResponse.CreateError($"UWP launch failed: {ex.Message}");
            }
        }

        private async Task<string> RunPowerShellScript(string script, CancellationToken cancellationToken)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return string.Empty;

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    return output.Trim();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "PowerShell script execution failed");
                return string.Empty;
            }
        }

        private async Task<int> FindNewProcess(string appName, HashSet<int> beforeProcesses, int maxWaitMs, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds < maxWaitMs && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentProcesses = Process.GetProcesses()
                        .Where(p => IsRelevantProcess(p, appName))
                        .ToList();

                    var newProcesses = currentProcesses.Where(p => !beforeProcesses.Contains(p.Id)).ToList();

                    if (newProcesses.Any())
                    {
                        var newProcess = newProcesses.First();
                        var processId = newProcess.Id;
                        
                        // Dispose all processes
                        foreach (var p in currentProcesses)
                            p.Dispose();

                        return processId;
                    }

                    // Dispose all processes
                    foreach (var p in currentProcesses)
                        p.Dispose();

                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error during process search for {AppName}", appName);
                    await Task.Delay(500, cancellationToken);
                }
            }

            return 0;
        }
    }
}