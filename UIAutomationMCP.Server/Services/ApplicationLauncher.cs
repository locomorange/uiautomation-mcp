using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessLaunchResponse> LaunchApplicationAsync(string application, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : IApplicationLauncher
    {
        private readonly ILogger<ApplicationLauncher> _logger;

        public ApplicationLauncher(ILogger<ApplicationLauncher> logger)
        {
            _logger = logger;
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