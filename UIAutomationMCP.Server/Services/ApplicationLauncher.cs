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

        #region Element-based detection methods

        /// <summary>
        /// Capture all UI elements using SearchElements
        /// </summary>
        private async Task<List<ElementInfo>> CaptureAllElements(int timeoutSeconds, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Capturing all UI elements with scope 'children'");
                
                var request = new SearchElementsRequest 
                { 
                    Scope = "children", // Search only direct children of desktop (windows)
                    MaxResults = 1000, 
                    IncludeDetails = false, 
                    BypassCache = true // Force fresh data - essential for detecting new elements
                };
                
                var result = await ExecuteServiceOperationAsync<SearchElementsRequest, SearchElementsResult>(
                    "SearchElements", request, nameof(CaptureAllElements), timeoutSeconds);

                if (result.Success && result.Data?.Elements != null)
                {
                    _logger.LogDebug("Captured {Count} UI elements", result.Data.Elements.Length);
                    return result.Data.Elements.ToList();
                }

                _logger.LogWarning("Failed to capture UI elements");
                return new List<ElementInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing UI elements");
                return new List<ElementInfo>();
            }
        }

        /// <summary>
        /// Find new window handles by comparing HWND lists
        /// </summary>
        private List<ElementInfo> FindNewElements(List<ElementInfo> beforeElements, List<ElementInfo> afterElements)
        {
            // Get WindowHandle lists (filter out null handles)
            var beforeHandles = beforeElements.Where(e => e.WindowHandle.HasValue).Select(e => e.WindowHandle!.Value).ToList();
            var afterHandles = afterElements.Where(e => e.WindowHandle.HasValue).Select(e => e.WindowHandle!.Value).ToList();
            
            _logger.LogDebug("Before HWNDs: [{BeforeHandles}]", string.Join(", ", beforeHandles.Take(10).Select(h => $"0x{h:X}")));
            _logger.LogDebug("After HWNDs: [{AfterHandles}]", string.Join(", ", afterHandles.Take(10).Select(h => $"0x{h:X}")));
            
            // Find new WindowHandles that appeared after launch
            var newHandles = new HashSet<long>();
            
            // Simple approach: if a WindowHandle appears more times in after than before, it's new
            var beforeHandleCounts = beforeHandles.GroupBy(h => h).ToDictionary(g => g.Key, g => g.Count());
            var afterHandleCounts = afterHandles.GroupBy(h => h).ToDictionary(g => g.Key, g => g.Count());
            
            foreach (var (handle, afterCount) in afterHandleCounts)
            {
                var beforeCount = beforeHandleCounts.GetValueOrDefault(handle, 0);
                if (afterCount > beforeCount)
                {
                    newHandles.Add(handle);
                    _logger.LogInformation("HWND 0x{Handle:X} increased from {Before} to {After} windows - marking as new", 
                        handle, beforeCount, afterCount);
                }
            }
            
            // Return all elements with new WindowHandles
            var newElements = afterElements.Where(e => e.WindowHandle.HasValue && newHandles.Contains(e.WindowHandle.Value)).ToList();
            
            _logger.LogInformation("Before: {BeforeCount} windows, After: {AfterCount} windows, New HWNDs: [{NewHandles}], New elements: {NewCount}", 
                beforeElements.Count, afterElements.Count, string.Join(", ", newHandles.Select(h => $"0x{h:X}")), newElements.Count);
            
            return newElements;
        }

        /// <summary>
        /// Wait for new elements to appear after application launch
        /// </summary>
        private async Task<List<ElementInfo>> WaitForNewElements(
            List<ElementInfo> beforeElements, 
            string appName, 
            int maxWaitMs, 
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var allNewElements = new List<ElementInfo>();

            _logger.LogDebug("Waiting for new elements for {AppName}, max wait: {MaxWait}ms", appName, maxWaitMs);

            while (stopwatch.ElapsedMilliseconds < maxWaitMs && !cancellationToken.IsCancellationRequested)
            {
                var currentElements = await CaptureAllElements(10, cancellationToken);
                var newElements = FindNewElements(beforeElements, currentElements);

                if (newElements.Any())
                {
                    // Merge new elements, avoiding duplicates by WindowHandle
                    var existingWindowHandles = new HashSet<long>(allNewElements.Where(e => e.WindowHandle.HasValue).Select(e => e.WindowHandle!.Value));
                    var uniqueNewElements = newElements.Where(e => e.WindowHandle.HasValue && !existingWindowHandles.Contains(e.WindowHandle.Value)).ToList();
                    
                    allNewElements.AddRange(uniqueNewElements);
                    _logger.LogDebug("Found {NewCount} new unique elements, total: {TotalCount}", 
                        uniqueNewElements.Count, allNewElements.Count);

                    // If we found elements, wait a bit more for additional elements to appear
                    if (allNewElements.Any() && stopwatch.ElapsedMilliseconds < maxWaitMs - 1000)
                    {
                        await Task.Delay(500, cancellationToken);
                        continue;
                    }
                }

                if (allNewElements.Any())
                {
                    break; // We have elements, stop waiting
                }

                await Task.Delay(500, cancellationToken); // Longer delay to allow UI elements to be created
            }

            _logger.LogInformation("Element detection completed for {AppName}: found {Count} new elements in {ElapsedMs}ms", 
                appName, allNewElements.Count, stopwatch.ElapsedMilliseconds);
            
            return allNewElements;
        }



        #endregion

        public async Task<ProcessLaunchResponse> LaunchApplicationAsync(string application, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(application))
                return ProcessLaunchResponse.CreateError("Application is required");

            var detectionStopwatch = Stopwatch.StartNew();
            
            try
            {
                // Capture baseline UI elements before launching (needed for all launch types)
                var beforeElements = await CaptureAllElements(timeoutSeconds, cancellationToken);
                _logger.LogInformation("Baseline elements for {Application}: {Count} elements", application, beforeElements.Count);

                // Check if this is a protocol URI (e.g., ms-settings:, mailto:, http:)
                if (IsProtocolUri(application))
                {
                    _logger.LogInformation("Detected protocol URI: {Application}, launching directly", application);
                    return await LaunchProtocolUri(application, beforeElements, timeoutSeconds, cancellationToken);
                }
                
                // Step 1: Try Win32 application launch
                var win32LaunchResult = await TryLaunchWin32(application, arguments, workingDirectory, cancellationToken);
                if (win32LaunchResult.LaunchSucceeded)
                {
                    _logger.LogInformation("Win32 launch succeeded for {Application}, now detecting new elements", application);
                    // Wait a moment for the application to create its UI elements
                    await Task.Delay(1000, cancellationToken);
                    
                    var result = await WaitForElementsAndCreateResponse(beforeElements, application, "Win32", detectionStopwatch, cancellationToken);
                    if (result != null) return result;
                }

                // Step 2: Try UWP application launch
                var uwpLaunchResult = await TryLaunchUWP(application, cancellationToken);
                if (uwpLaunchResult.LaunchSucceeded)
                {
                    _logger.LogInformation("UWP launch succeeded for {Application}, now detecting new elements", application);
                    // Wait a moment for the application to create its UI elements
                    await Task.Delay(1000, cancellationToken);
                    
                    var result = await WaitForElementsAndCreateResponse(beforeElements, application, "UWP", detectionStopwatch, cancellationToken);
                    if (result != null) return result;
                }

                detectionStopwatch.Stop();
                _logger.LogWarning("Failed to launch application: {Application}. Win32 success: {Win32Success}, UWP success: {UwpSuccess}", 
                    application, win32LaunchResult.LaunchSucceeded, uwpLaunchResult.LaunchSucceeded);
                return ProcessLaunchResponse.CreateError($"Failed to launch application: {application}. Tried Win32 and UWP methods.");
            }
            catch (Exception ex)
            {
                detectionStopwatch.Stop();
                _logger.LogError(ex, "Error launching application: {Application}", application);
                return ProcessLaunchResponse.CreateError($"Exception during launch: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if the application string is a protocol URI
        /// </summary>
        private bool IsProtocolUri(string application)
        {
            // Protocol URIs typically have the format "protocol:" 
            // Common examples: ms-settings:, mailto:, http:, https:, tel:, etc.
            return application.Contains(':') && 
                   !Path.IsPathFullyQualified(application) && // Not a file path like C:\Program Files\...
                   !application.StartsWith("\\\\"); // Not a UNC path
        }

        /// <summary>
        /// Launch a protocol URI directly using Process.Start
        /// </summary>
        private async Task<ProcessLaunchResponse> LaunchProtocolUri(string protocolUri, List<ElementInfo> beforeElements, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var detectionStopwatch = Stopwatch.StartNew();
            
            try
            {

                // Launch the protocol URI directly
                var processInfo = new ProcessStartInfo
                {
                    FileName = protocolUri,
                    UseShellExecute = true, // Required for protocol URIs
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                _logger.LogInformation("Started protocol URI {ProtocolUri}", protocolUri);

                // Wait for the application to create its UI elements
                await Task.Delay(2000, cancellationToken);
                
                var result = await WaitForElementsAndCreateResponse(beforeElements, protocolUri, "Protocol URI", detectionStopwatch, cancellationToken);
                if (result != null) return result;

                // Even if no new elements are detected, the protocol launch might have succeeded
                // (e.g., it opened an existing window or a system dialog)
                detectionStopwatch.Stop();
                _logger.LogInformation("Protocol URI {ProtocolUri} launched successfully, but no new UI elements detected in {ElapsedMs}ms", 
                    protocolUri, detectionStopwatch.ElapsedMilliseconds);
                return ProcessLaunchResponse.CreateSuccess(process?.Id ?? 0, protocolUri, false, "Protocol launched", null);
            }
            catch (Exception ex)
            {
                detectionStopwatch.Stop();
                _logger.LogError(ex, "Error launching protocol URI: {ProtocolUri}", protocolUri);
                return ProcessLaunchResponse.CreateError($"Failed to launch protocol URI: {protocolUri}. Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Wait for new UI elements and create success response after launch
        /// </summary>
        private async Task<ProcessLaunchResponse> WaitForElementsAndCreateResponse(
            List<ElementInfo> beforeElements,
            string application,
            string launchType,
            Stopwatch detectionStopwatch,
            CancellationToken cancellationToken)
        {
            // Wait for new elements to appear - protocol URIs might need slightly longer
            var waitTimeMs = launchType == "Protocol URI" ? 10000 : 8000;
            var newElements = await WaitForNewElements(beforeElements, application, waitTimeMs, cancellationToken);
            _logger.LogInformation("Found {Count} new elements after {LaunchType} launch", newElements.Count, launchType);

            if (newElements.Any())
            {
                detectionStopwatch.Stop();
                return CreateSuccessResponseFromElements(newElements, application, launchType, detectionStopwatch.ElapsedMilliseconds);
            }

            return null; // No new elements found, caller should handle fallback
        }

        /// <summary>
        /// Process new elements and create success response with most relevant window
        /// </summary>
        private ProcessLaunchResponse CreateSuccessResponseFromElements(
            List<ElementInfo> newElements, 
            string application, 
            string launchType,
            long elapsedMs)
        {
            if (!newElements.Any())
            {
                return ProcessLaunchResponse.CreateSuccess(0, application, false, "Application launched", null);
            }

            // Group elements by WindowHandle and find the most relevant window
            var elementsByWindow = newElements.Where(e => e.WindowHandle.HasValue).GroupBy(e => e.WindowHandle!.Value).ToList();
            
            if (elementsByWindow.Any())
            {
                // Simply select the window with the most elements
                var mostRelevantGroup = elementsByWindow
                    .OrderByDescending(g => g.Count())
                    .First();
                
                var windowHandle = mostRelevantGroup.Key;
                var relevantElements = mostRelevantGroup.ToList();
                var processId = relevantElements.FirstOrDefault()?.ProcessId ?? 0;
                
                _logger.LogInformation("Successfully launched {LaunchType} application: {Application}, HWND: 0x{WindowHandle:X}, PID: {ProcessId}, found {Count} elements for this window (total new: {TotalCount}) in {ElapsedMs}ms", 
                    launchType, application, windowHandle, processId, relevantElements.Count, newElements.Count, elapsedMs);
                return ProcessLaunchResponse.CreateSuccess(processId, application, false, relevantElements.FirstOrDefault()?.Name, windowHandle);
            }

            // Fallback if no windows found (shouldn't happen if newElements is not empty)
            var firstElement = newElements.First();
            return ProcessLaunchResponse.CreateSuccess(firstElement.ProcessId, application, false, firstElement.Name, firstElement.WindowHandle);
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

        private async Task<LaunchResult> TryLaunchWin32(string application, string? arguments, string? workingDirectory, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting Win32 launch for {Application}", application);
                
                // Check if it's a full path
                if (Path.IsPathFullyQualified(application) && File.Exists(application))
                {
                    _logger.LogDebug("Using full path: {Path}", application);
                    return await LaunchWin32Process(application, arguments, workingDirectory, cancellationToken);
                }

                // Try to find executable using 'where' command
                var executablePath = await FindExecutablePath(application, cancellationToken);
                _logger.LogDebug("Found executable path: {Path}", executablePath ?? "null");
                if (!string.IsNullOrEmpty(executablePath))
                {
                    return await LaunchWin32Process(executablePath, arguments, workingDirectory, cancellationToken);
                }

                _logger.LogWarning("Win32 executable not found for {Application}", application);
                return LaunchResult.Failure("Win32 executable not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Win32 launch failed for {Application}", application);
                return LaunchResult.Failure($"Win32 launch failed: {ex.Message}");
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

        private Task<LaunchResult> LaunchWin32Process(string executablePath, string? arguments, string? workingDirectory, CancellationToken cancellationToken)
        {
            var appName = Path.GetFileNameWithoutExtension(executablePath);
            
            try
            {
                _logger.LogDebug("Starting Win32 process launch for {Application}", appName);

                // Start the process
                var processInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? "",
                    UseShellExecute = false
                };

                var process = Process.Start(processInfo);
                var launchedProcessId = process?.Id;
                _logger.LogInformation("Started Win32 process {Application} with PID {ProcessId}", appName, launchedProcessId);
                
                // Verify the process is still running
                if (process != null && !process.HasExited)
                {
                    _logger.LogInformation("Process {ProcessId} is running with name: {ProcessName}", 
                        process.Id, process.ProcessName);
                }
                else
                {
                    _logger.LogWarning("Process {ProcessId} has already exited or failed to start", launchedProcessId);
                }

                return Task.FromResult(LaunchResult.Success(launchedProcessId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Win32 process launch for {Application}", appName);
                return Task.FromResult(LaunchResult.Failure($"Win32 launch failed: {ex.Message}"));
            }
        }

        private async Task<LaunchResult> TryLaunchUWP(string application, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting UWP application launch for {Application}", application);

                // Use Get-StartApps to find UWP application
                var script = $@"
                    Get-StartApps | Where-Object {{ 
                        $_.Name -like '*{application}*' -or 
                        $_.AppID -like '*{application}*' 
                    }} | Select-Object -First 1 -ExpandProperty AppID";

                var appId = await RunPowerShellScript(script, cancellationToken);
                if (string.IsNullOrWhiteSpace(appId))
                {
                    return LaunchResult.Failure("UWP application not found");
                }

                _logger.LogDebug("Found UWP application {Application} with AppID: {AppId}", application, appId);

                // Launch via shell:AppsFolder
                var explorerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"shell:AppsFolder\\{appId}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                });

                _logger.LogDebug("Started UWP application {Application} via explorer", application);

                return LaunchResult.Success(null, appId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UWP application launch for {Application}", application);
                return LaunchResult.Failure($"UWP launch failed: {ex.Message}");
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
                metadata.UsedUIAutomationDetection = true; // Now using element-based detection with SearchElements
            }
            
            return metadata;
        }
    }


    /// <summary>
    /// Result of attempting to launch an application (before process detection)
    /// </summary>
    internal class LaunchResult
    {
        public bool LaunchSucceeded { get; set; }
        public int? LaunchedProcessId { get; set; }
        public string? AppId { get; set; }
        public string? ErrorMessage { get; set; }

        public static LaunchResult Success(int? processId = null, string? appId = null) => 
            new() { LaunchSucceeded = true, LaunchedProcessId = processId, AppId = appId };

        public static LaunchResult Failure(string error) => 
            new() { LaunchSucceeded = false, ErrorMessage = error };
    }
}