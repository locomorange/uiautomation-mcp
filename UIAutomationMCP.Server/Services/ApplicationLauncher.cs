using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using System.Diagnostics;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessLaunchResponse> LaunchApplicationAsync(string application, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);
    }

    public class ApplicationLauncher : BaseUIAutomationService<ApplicationLauncherMetadata>, IApplicationLauncher
    {
        public ApplicationLauncher(IProcessManager processManager, ILogger<ApplicationLauncher> logger)
            : base(processManager, logger)
        {
        }

        protected override string GetOperationType() => "applicationLauncher";

        #region UIAutomation-based window detection methods

        /// <summary>
        /// Capture current window snapshot using UIAutomation
        /// </summary>
        private async Task<List<WindowInfo>> CaptureWindowSnapshotAsync()
        {
            try
            {
                _logger.LogDebug("Capturing window snapshot with cache bypass enabled");
                var request = new SearchElementsRequest
                {
                    ControlType = "Window",
                    Scope = "children", // Desktop direct children only
                    MaxResults = 1000,
                    IncludeDetails = false,
                    BypassCache = true // Force real-time window detection
                };

                var result = await ExecuteServiceOperationAsync<SearchElementsRequest, SearchElementsResult>(
                    "SearchElements", request, nameof(CaptureWindowSnapshotAsync), 30);

                if (result.Success && result.Data?.Elements != null)
                {
                    return result.Data.Elements.Select(e => new WindowInfo
                    {
                        ProcessId = e.ProcessId,
                        Name = e.Name ?? "",
                        AutomationId = e.AutomationId ?? "",
                        BoundingRectangle = e.BoundingRectangle ?? new BoundingRectangle()
                    }).ToList();
                }

                return new List<WindowInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to capture window snapshot");
                return new List<WindowInfo>();
            }
        }

        /// <summary>
        /// Detect new windows by comparing before and after snapshots using unique window identification
        /// </summary>
        private List<WindowInfo> DetectNewWindows(List<WindowInfo> beforeWindows, List<WindowInfo> afterWindows)
        {
            // Create unique identifiers for windows using ProcessId + Name + BoundingRectangle
            var beforeWindowKeys = beforeWindows.Select(w => GetWindowUniqueKey(w)).ToHashSet();
            var newWindows = afterWindows.Where(w => !beforeWindowKeys.Contains(GetWindowUniqueKey(w))).ToList();
            
            // Detailed logging for debugging
            _logger.LogDebug("Window detection comparison:");
            _logger.LogDebug("  Before windows: {Count} windows: [{Windows}]", 
                beforeWindows.Count, string.Join(", ", beforeWindows.Select(w => $"{w.ProcessId}({w.Name})")));
            _logger.LogDebug("  After windows: {Count} windows: [{Windows}]", 
                afterWindows.Count, string.Join(", ", afterWindows.Select(w => $"{w.ProcessId}({w.Name})")));
            _logger.LogDebug("  New windows detected: {Count} windows: [{Windows}]", 
                newWindows.Count, string.Join(", ", newWindows.Select(w => $"{w.ProcessId}({w.Name})")));
            
            // Additional debugging: show unique keys for better understanding
            _logger.LogDebug("  Before window keys: [{Keys}]", 
                string.Join(", ", beforeWindowKeys.Take(3)));
            _logger.LogDebug("  New window keys: [{Keys}]", 
                string.Join(", ", newWindows.Take(3).Select(w => GetWindowUniqueKey(w))));
            
            return newWindows;
        }

        /// <summary>
        /// Generate unique key for window identification using ProcessId, Name, and Position
        /// </summary>
        private string GetWindowUniqueKey(WindowInfo window)
        {
            var rect = window.BoundingRectangle;
            return $"{window.ProcessId}|{window.Name ?? ""}|{rect.X},{rect.Y},{rect.Width},{rect.Height}";
        }

        /// <summary>
        /// Wait for window appearance with periodic checks
        /// </summary>
        private async Task<int> WaitForWindowAppearanceAsync(List<WindowInfo> beforeWindows, int? launchedProcessId, int maxWaitMs)
        {
            var stopwatch = Stopwatch.StartNew();
            var retries = 0;

            _logger.LogDebug("Starting window detection - launched PID: {LaunchedPID}, max wait: {MaxWait}ms", 
                launchedProcessId, maxWaitMs);

            while (stopwatch.ElapsedMilliseconds < maxWaitMs)
            {
                try
                {
                    retries++;
                    _logger.LogDebug("Window detection retry {Retry} at {ElapsedMs}ms", retries, stopwatch.ElapsedMilliseconds);
                    
                    var currentWindows = await CaptureWindowSnapshotAsync();
                    var newWindows = DetectNewWindows(beforeWindows, currentWindows);

                    if (newWindows.Any())
                    {
                        _logger.LogInformation("Found {Count} new windows after {Retries} retries and {ElapsedMs}ms", 
                            newWindows.Count, retries, stopwatch.ElapsedMilliseconds);
                        var selectedPid = SelectBestProcessId(newWindows, launchedProcessId);
                        _logger.LogInformation("Selected PID {SelectedPID} from new windows", selectedPid);
                        return selectedPid;
                    }
                    else
                    {
                        _logger.LogDebug("No new windows found in retry {Retry}", retries);
                    }

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during window detection retry {Retry}", retries);
                    await Task.Delay(500);
                }
            }

            _logger.LogWarning("Window detection timed out after {ElapsedMs}ms and {Retries} retries", stopwatch.ElapsedMilliseconds, retries);
            return 0;
        }

        /// <summary>
        /// Select best process ID from detected new windows
        /// </summary>
        private int SelectBestProcessId(List<WindowInfo> newWindows, int? launchedProcessId)
        {
            if (!newWindows.Any()) return 0;

            // 1. Prefer window matching launched process ID
            if (launchedProcessId.HasValue)
            {
                var matchingWindow = newWindows.FirstOrDefault(w => w.ProcessId == launchedProcessId.Value);
                if (matchingWindow != null)
                {
                    _logger.LogDebug("Found window for launched process {ProcessId}", launchedProcessId.Value);
                    return matchingWindow.ProcessId;
                }
            }

            // 2. Select most recent window (highest process ID)
            var latestWindow = newWindows.OrderByDescending(w => w.ProcessId).First();
            _logger.LogDebug("Selected latest window with ProcessId {ProcessId}", latestWindow.ProcessId);
            return latestWindow.ProcessId;
        }

        #endregion

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
            var detectionStopwatch = Stopwatch.StartNew();
            var appName = Path.GetFileNameWithoutExtension(executablePath);
            
            try
            {
                // 1. Capture window snapshot before launch
                var beforeWindows = await CaptureWindowSnapshotAsync();
                _logger.LogDebug("Captured {Count} windows before launching {Application}", beforeWindows.Count, appName);

                // 2. Start the process
                var processInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    UseShellExecute = false
                };

                var process = Process.Start(processInfo);
                var launchedProcessId = process?.Id;
                _logger.LogDebug("Started process {Application} with PID {ProcessId}", appName, launchedProcessId);

                // 3. Wait for window to appear using UIAutomation detection
                var targetProcessId = await WaitForWindowAppearanceAsync(beforeWindows, launchedProcessId, 5000);

                detectionStopwatch.Stop();

                if (targetProcessId > 0)
                {
                    _logger.LogInformation("Successfully detected window for {Application}, final PID: {ProcessId} (launched PID: {LaunchedPID})", 
                        appName, targetProcessId, launchedProcessId);
                    return ProcessLaunchResponse.CreateSuccess(targetProcessId, appName, false);
                }

                // Fallback: Return launched process ID if available
                if (launchedProcessId.HasValue)
                {
                    _logger.LogWarning("Window detection failed for {Application}, returning launched PID: {ProcessId}", 
                        appName, launchedProcessId.Value);
                    return ProcessLaunchResponse.CreateSuccess(launchedProcessId.Value, appName, false);
                }

                // Final fallback: Use old process diff method
                _logger.LogWarning("Both UIAutomation and direct process launch failed, trying legacy detection for {Application}", appName);
                var newProcessId = await FindNewProcess(appName, beforeProcesses, 2000, cancellationToken);
                if (newProcessId > 0)
                {
                    return ProcessLaunchResponse.CreateSuccess(newProcessId, appName, false);
                }

                return ProcessLaunchResponse.CreateError($"Failed to start or detect Win32 process for {appName}");
            }
            catch (Exception ex)
            {
                detectionStopwatch.Stop();
                _logger.LogError(ex, "Error during Win32 process launch for {Application}", appName);
                return ProcessLaunchResponse.CreateError($"Win32 launch failed: {ex.Message}");
            }
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

        protected override ApplicationLauncherMetadata CreateSuccessMetadata<TResult>(TResult data, IServiceContext context)
        {
            var metadata = base.CreateSuccessMetadata(data, context);
            
            if (data is ProcessLaunchResponse response)
            {
                metadata.ApplicationPath = response.ProcessName ?? "";
                metadata.ProcessId = response.ProcessId;
                metadata.LaunchSuccessful = response.Success;
                metadata.OperationSuccessful = response.Success;
                metadata.ProcessName = response.ProcessName ?? "";
                metadata.HasExited = response.HasExited;
                metadata.ActionPerformed = "applicationLaunched";
                metadata.UsedUIAutomationDetection = true;
            }
            
            return metadata;
        }
    }
}