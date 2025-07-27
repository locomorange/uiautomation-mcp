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

        #region Process-based detection methods with SearchElements validation

        /// <summary>
        /// Find all new processes that match the application name
        /// </summary>
        private async Task<List<int>> FindAllNewProcesses(string appName, HashSet<int> beforeProcesses, int maxWaitMs, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var foundProcesses = new HashSet<int>();

            _logger.LogDebug("Starting process detection for {AppName}, max wait: {MaxWait}ms", appName, maxWaitMs);

            while (stopwatch.ElapsedMilliseconds < maxWaitMs && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentProcesses = Process.GetProcesses()
                        .Where(p => IsRelevantProcess(p, appName))
                        .ToList();

                    var newProcesses = currentProcesses.Where(p => !beforeProcesses.Contains(p.Id)).ToList();

                    foreach (var newProcess in newProcesses)
                    {
                        if (!foundProcesses.Contains(newProcess.Id))
                        {
                            foundProcesses.Add(newProcess.Id);
                            _logger.LogDebug("Found new process candidate: {ProcessId} ({ProcessName}) for {AppName}", 
                                newProcess.Id, newProcess.ProcessName, appName);
                        }
                    }

                    // Dispose all processes
                    foreach (var p in currentProcesses)
                        p.Dispose();

                    // Wait a bit longer to capture processes that might launch with delay (like calc.exe -> CalculatorApp.exe)
                    if (foundProcesses.Any() && stopwatch.ElapsedMilliseconds > 1000)
                    {
                        _logger.LogDebug("Found {Count} processes after {ElapsedMs}ms, continuing for potential delayed launches", 
                            foundProcesses.Count, stopwatch.ElapsedMilliseconds);
                    }

                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error during process search for {AppName}", appName);
                    await Task.Delay(500, cancellationToken);
                }
            }

            var result = foundProcesses.ToList();
            _logger.LogInformation("Process detection completed for {AppName}: found {Count} candidates in {ElapsedMs}ms: [{Processes}]", 
                appName, result.Count, stopwatch.ElapsedMilliseconds, string.Join(", ", result));
            
            return result;
        }

        /// <summary>
        /// Validate process candidates using SearchElements to check for valid application windows
        /// </summary>
        private async Task<List<ValidatedProcess>> ValidateProcessCandidatesAsync(List<int> candidateProcesses, string appName)
        {
            var validatedProcesses = new List<ValidatedProcess>();
            
            foreach (var processId in candidateProcesses)
            {
                try
                {
                    _logger.LogDebug("Validating process {ProcessId} for {AppName} using SearchElements", processId, appName);
                    
                    // Try multiple search strategies for better process validation
                    var searchStrategies = new[]
                    {
                        new SearchElementsRequest { ProcessId = processId, ControlType = "", Scope = "subtree", MaxResults = 20, IncludeDetails = false, BypassCache = true }, // Any elements
                        new SearchElementsRequest { ProcessId = processId, ControlType = "Window", Scope = "subtree", MaxResults = 20, IncludeDetails = false, BypassCache = true }, // Windows
                        new SearchElementsRequest { ProcessId = processId, ControlType = "Pane", Scope = "subtree", MaxResults = 20, IncludeDetails = false, BypassCache = true }, // Panes
                        new SearchElementsRequest { ProcessId = processId, ControlType = "Button", Scope = "subtree", MaxResults = 20, IncludeDetails = false, BypassCache = true }, // Buttons
                        new SearchElementsRequest { ProcessId = processId, ControlType = "Document", Scope = "subtree", MaxResults = 20, IncludeDetails = false, BypassCache = true } // Documents
                    };

                    SearchElementsResult? validResult = null;
                    string usedStrategy = "";
                    
                    foreach (var (request, index) in searchStrategies.Select((req, i) => (req, i)))
                    {
                        try
                        {
                            _logger.LogDebug("Trying strategy {Index} for PID {ProcessId}: ControlType='{ControlType}', Scope='{Scope}'", 
                                index, processId, request.ControlType, request.Scope);
                            
                            var result = await ExecuteServiceOperationAsync<SearchElementsRequest, SearchElementsResult>(
                                "SearchElements", request, nameof(ValidateProcessCandidatesAsync), 5); // Short timeout for validation

                            if (result.Success && result.Data?.Elements != null && result.Data.Elements.Any())
                            {
                                validResult = result.Data;
                                usedStrategy = $"Strategy{index}(ControlType='{request.ControlType}')";
                                _logger.LogInformation("Strategy {Index} succeeded for PID {ProcessId}: found {Count} elements with ControlType='{ControlType}'", 
                                    index, processId, result.Data.Elements.Length, request.ControlType);
                                break;
                            }
                            else
                            {
                                _logger.LogDebug("Strategy {Index} failed for PID {ProcessId}: {Reason}", 
                                    index, processId, result.Success ? "no elements found" : "operation failed");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Strategy {Index} exception for PID {ProcessId}", index, processId);
                        }
                    }

                    if (validResult != null)
                    {
                        var elements = validResult.Elements.ToList();
                        var validElements = elements.Where(IsValidApplicationElement).ToList();
                        
                        if (validElements.Any())
                        {
                            var validated = new ValidatedProcess
                            {
                                ProcessId = processId,
                                WindowCount = elements.Count,
                                ValidWindowCount = validElements.Count,
                                WindowNames = validElements.Select(w => w.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList(),
                                HasMainWindow = validElements.Any(w => IsMainApplicationElement(w, appName))
                            };
                            
                            validatedProcesses.Add(validated);
                            _logger.LogInformation("Process {ProcessId} validated using {Strategy}: {ElementCount} elements, {ValidCount} valid, HasMain: {HasMain}, Names: [{Names}]", 
                                processId, usedStrategy, validated.WindowCount, validated.ValidWindowCount, validated.HasMainWindow,
                                string.Join(", ", validated.WindowNames));
                        }
                        else
                        {
                            _logger.LogDebug("Process {ProcessId} has {ElementCount} elements but no valid application elements", 
                                processId, elements.Count);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Process {ProcessId} validation failed: all search strategies unsuccessful", processId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error validating process {ProcessId} for {AppName}", processId, appName);
                }
            }

            return validatedProcesses;
        }

        /// <summary>
        /// Check if an element represents a valid application element (broader than just windows)
        /// </summary>
        private bool IsValidApplicationElement(BasicElementInfo element)
        {
            // Filter out system and shell elements
            var name = element.Name ?? "";
            var className = element.ClassName ?? "";
            var controlType = element.ControlType ?? "";
            
            // Exclude common system elements
            if (name.Equals("Program Manager", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Shell_TrayWnd", StringComparison.OrdinalIgnoreCase) ||
                className.Equals("Shell_TrayWnd", StringComparison.OrdinalIgnoreCase) ||
                className.Equals("Progman", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Desktop", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Accept various UI elements that indicate an active application
            var validControlTypes = new[] { "Window", "Pane", "Button", "Document", "Edit", "Text", "Group" };
            if (!string.IsNullOrEmpty(controlType) && !validControlTypes.Contains(controlType, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            // Must be enabled for user interaction (allow offscreen elements for now)
            return element.IsEnabled;
        }

        /// <summary>
        /// Check if an element is likely from the main application (broader than just windows)
        /// </summary>
        private bool IsMainApplicationElement(BasicElementInfo element, string appName)
        {
            var name = element.Name ?? "";
            var cleanAppName = Path.GetFileNameWithoutExtension(appName).ToLowerInvariant();
            
            // Check if element name contains the application name or common calculator terms
            return name.ToLowerInvariant().Contains(cleanAppName) ||
                   name.ToLowerInvariant().Contains(appName.ToLowerInvariant()) ||
                   (cleanAppName == "calc" && (name.ToLowerInvariant().Contains("calculator") || 
                                               name.ToLowerInvariant().Contains("計算") ||
                                               name.ToLowerInvariant().Contains("電卓")));
        }

        /// <summary>
        /// Select the best validated process based on selection criteria
        /// </summary>
        private int SelectBestValidatedProcess(List<ValidatedProcess> validatedProcesses, int? launchedProcessId, string appName)
        {
            if (!validatedProcesses.Any())
            {
                _logger.LogDebug("No validated processes available for selection");
                return 0;
            }

            _logger.LogDebug("Selecting best process from {Count} validated candidates for {AppName}", 
                validatedProcesses.Count, appName);

            // 1. Prefer processes with main application window
            var mainWindowProcesses = validatedProcesses.Where(p => p.HasMainWindow).ToList();
            if (mainWindowProcesses.Any())
            {
                var selected = mainWindowProcesses.OrderByDescending(p => p.ProcessId).First();
                _logger.LogInformation("Selected process {ProcessId} with main window (Names: [{Names}])", 
                    selected.ProcessId, string.Join(", ", selected.WindowNames));
                return selected.ProcessId;
            }

            // 2. Prefer processes that match the launched process ID if available
            if (launchedProcessId.HasValue)
            {
                var matchingProcess = validatedProcesses.FirstOrDefault(p => p.ProcessId == launchedProcessId.Value);
                if (matchingProcess != null)
                {
                    _logger.LogInformation("Selected launched process {ProcessId} (Names: [{Names}])", 
                        matchingProcess.ProcessId, string.Join(", ", matchingProcess.WindowNames));
                    return matchingProcess.ProcessId;
                }
            }

            // 3. Prefer processes with more valid windows
            var bestByWindowCount = validatedProcesses.OrderByDescending(p => p.ValidWindowCount).ThenByDescending(p => p.ProcessId).First();
            _logger.LogInformation("Selected process {ProcessId} with {ValidCount} valid windows (Names: [{Names}])", 
                bestByWindowCount.ProcessId, bestByWindowCount.ValidWindowCount, string.Join(", ", bestByWindowCount.WindowNames));
            return bestByWindowCount.ProcessId;
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
                _logger.LogDebug("Starting Win32 process launch for {Application}", appName);

                // 1. Start the process
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

                // 2. Find all new processes using process difference detection
                var candidateProcesses = await FindAllNewProcesses(appName, beforeProcesses, 3000, cancellationToken);
                _logger.LogDebug("Found {Count} candidate processes for {Application}: [{Candidates}]", 
                    candidateProcesses.Count, appName, string.Join(", ", candidateProcesses));

                if (candidateProcesses.Any())
                {
                    // 3. Validate candidates using SearchElements requests
                    var validatedProcesses = await ValidateProcessCandidatesAsync(candidateProcesses, appName);
                    _logger.LogDebug("Validated {Count} processes with windows for {Application}: [{Validated}]", 
                        validatedProcesses.Count, appName, string.Join(", ", validatedProcesses.Select(p => p.ProcessId)));

                    // 4. Select the best validated process or return multiple candidates
                    var selectedProcessId = SelectBestValidatedProcess(validatedProcesses, launchedProcessId, appName);
                    
                    detectionStopwatch.Stop();
                    
                    if (selectedProcessId > 0)
                    {
                        _logger.LogInformation("Successfully detected process for {Application}, final PID: {ProcessId} (launched PID: {LaunchedPID}) in {ElapsedMs}ms", 
                            appName, selectedProcessId, launchedProcessId, detectionStopwatch.ElapsedMilliseconds);
                        return ProcessLaunchResponse.CreateSuccess(selectedProcessId, appName, false);
                    }
                    else if (validatedProcesses.Any())
                    {
                        // Fallback: return the best validated process even if no main window found
                        var fallbackProcess = validatedProcesses.OrderByDescending(p => p.ValidWindowCount).ThenByDescending(p => p.ProcessId).First();
                        _logger.LogInformation("No ideal process found, using fallback validated PID: {ProcessId} with {WindowCount} windows for {Application}", 
                            fallbackProcess.ProcessId, fallbackProcess.ValidWindowCount, appName);
                        return ProcessLaunchResponse.CreateSuccess(fallbackProcess.ProcessId, appName, false);
                    }
                    else
                    {
                        // Fallback: For modern applications that don't have UIAutomation-accessible windows,
                        // return the newest process (highest PID) as a reasonable default
                        var fallbackProcessId = candidateProcesses.OrderByDescending(pid => pid).First();
                        _logger.LogWarning("SearchElements validation failed for all candidates for {Application}: [{Candidates}]. Using fallback strategy: selecting newest process PID {FallbackPID}", 
                            appName, string.Join(", ", candidateProcesses), fallbackProcessId);
                        
                        return ProcessLaunchResponse.CreateSuccess(fallbackProcessId, appName, false);
                    }
                }

                // Fallback: Return launched process ID if available
                if (launchedProcessId.HasValue)
                {
                    _logger.LogWarning("Process validation failed for {Application}, returning launched PID: {ProcessId}", 
                        appName, launchedProcessId.Value);
                    return ProcessLaunchResponse.CreateSuccess(launchedProcessId.Value, appName, false);
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
                metadata.UsedUIAutomationDetection = false; // Now using process detection + SearchElements validation
            }
            
            return metadata;
        }
    }

    /// <summary>
    /// Represents a process that has been validated using SearchElements
    /// </summary>
    internal class ValidatedProcess
    {
        public int ProcessId { get; set; }
        public int WindowCount { get; set; }
        public int ValidWindowCount { get; set; }
        public List<string> WindowNames { get; set; } = new();
        public bool HasMainWindow { get; set; }
    }
}