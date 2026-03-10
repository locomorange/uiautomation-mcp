using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Abstractions;
using UIAutomationMCP.Server.Infrastructure;
using UIAutomationMCP.Server.Abstractions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UIAutomationMCP.Server.Services
{
    public interface IApplicationLauncher
    {
        Task<ProcessLaunchResponse> LaunchApplicationAsync(string application, string? arguments = null, string? workingDirectory = null, int timeoutSeconds = 60, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the process IDs of all applications launched by this instance.
        /// Uses a Windows Job Object for reliable tracking including child processes.
        /// </summary>
        int[] GetTrackedProcessIds();
    }

    public class ApplicationLauncher : BaseUIAutomationService<ApplicationLauncherMetadata>, IApplicationLauncher, IDisposable
    {
        private readonly WindowsJobObject? _launchedAppsJobObject;
        private bool _disposed;

        public ApplicationLauncher(IProcessManager processManager, ILogger<ApplicationLauncher> logger)
            : base(processManager, logger)
        {
            try
            {
                _launchedAppsJobObject = new WindowsJobObject(killOnClose: false, logger, "UIAutomationMCP_LaunchedApps");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create tracking Job Object for launched applications. Process tracking will be unavailable.");
                _launchedAppsJobObject = null;
            }
        }

        protected override string GetOperationType() => "applicationLauncher";

        #region Element-based detection methods

        /// <summary>
        /// Capture all UI elements using SearchElements
        /// </summary>
        private async Task<List<ElementInfo>> CaptureAllElements(int timeoutSeconds, CancellationToken cancellationToken, bool includeDetails = false)
        {
            try
            {
                _logger.LogDebug("Capturing all UI elements with scope 'children', includeDetails: {IncludeDetails}", includeDetails);

                var request = new SearchElementsRequest
                {
                    Scope = "children", // Search only direct children of desktop (windows)
                    MaxResults = 1000,
                    IncludeDetails = includeDetails, // Now configurable for focus detection
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
        /// Launch path専用のスナップショット取得。SearchElementsのタイムアウト時に段階的に再試行する。
        /// </summary>
        private async Task<List<ElementInfo>> CaptureLaunchSnapshotWithRetryAsync(
            string snapshotName,
            CancellationToken cancellationToken,
            bool includeDetails = true)
        {
            var timeoutPlanSeconds = new[] { 10, 14, 20 };

            for (var attempt = 0; attempt < timeoutPlanSeconds.Length; attempt++)
            {
                var timeoutSeconds = timeoutPlanSeconds[attempt];

                try
                {
                    var elements = await CaptureAllElements(timeoutSeconds, cancellationToken, includeDetails);

                    if (elements.Count > 0 || attempt == timeoutPlanSeconds.Length - 1)
                    {
                        if (attempt > 0)
                        {
                            _logger.LogInformation(
                                "Launch snapshot {SnapshotName} succeeded on retry {Attempt}/{TotalAttempts} with timeout {TimeoutSeconds}s (elements: {ElementCount})",
                                snapshotName,
                                attempt + 1,
                                timeoutPlanSeconds.Length,
                                timeoutSeconds,
                                elements.Count);
                        }

                        return elements;
                    }

                    _logger.LogWarning(
                        "Launch snapshot {SnapshotName} returned 0 elements on attempt {Attempt}/{TotalAttempts} (timeout {TimeoutSeconds}s); retrying",
                        snapshotName,
                        attempt + 1,
                        timeoutPlanSeconds.Length,
                        timeoutSeconds);
                }
                catch (TimeoutException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Launch snapshot {SnapshotName} timed out on attempt {Attempt}/{TotalAttempts} (timeout {TimeoutSeconds}s)",
                        snapshotName,
                        attempt + 1,
                        timeoutPlanSeconds.Length,
                        timeoutSeconds);
                }

                if (attempt < timeoutPlanSeconds.Length - 1)
                {
                    var delayMs = 250 * (attempt + 1);
                    await Task.Delay(delayMs, cancellationToken);
                }
            }

            return new List<ElementInfo>();
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
        /// Find elements that gained keyboard focus by comparing before and after states
        /// </summary>
        private List<ElementInfo> FindActivatedElements(List<ElementInfo> beforeElements, List<ElementInfo> afterElements)
        {
            try
            {
                // Only elements with details (HasKeyboardFocus) can be compared
                var beforeFocusElements = beforeElements
                    .Where(e => e.Details != null && e.WindowHandle.HasValue)
                    .ToList();

                var afterFocusElements = afterElements
                    .Where(e => e.Details != null && e.WindowHandle.HasValue)
                    .ToList();

                _logger.LogDebug("Checking focus changes: Before={BeforeCount} elements with details, After={AfterCount} elements with details",
                    beforeFocusElements.Count, afterFocusElements.Count);

                // Find elements that gained focus (HasKeyboardFocus changed from false to true)
                var activatedWindowHandles = new HashSet<long>();

                foreach (var afterElement in afterFocusElements)
                {
                    if (afterElement.Details!.HasKeyboardFocus)
                    {
                        // Find corresponding element in before state
                        var beforeElement = beforeFocusElements.FirstOrDefault(e =>
                            e.WindowHandle == afterElement.WindowHandle &&
                            e.AutomationId == afterElement.AutomationId &&
                            e.Name == afterElement.Name &&
                            e.ControlType == afterElement.ControlType);

                        // If element didn't have focus before, or didn't exist before, it's activated
                        if (beforeElement == null || !beforeElement.Details!.HasKeyboardFocus)
                        {
                            activatedWindowHandles.Add(afterElement.WindowHandle!.Value);
                            _logger.LogInformation("HWND 0x{Handle:X} gained focus - element: {Name} ({ControlType})",
                                afterElement.WindowHandle.Value, afterElement.Name, afterElement.ControlType);
                        }
                    }
                }

                if (activatedWindowHandles.Any())
                {
                    // Return all elements from windows that gained focus
                    var activatedElements = afterElements
                        .Where(e => e.WindowHandle.HasValue && activatedWindowHandles.Contains(e.WindowHandle.Value))
                        .ToList();

                    _logger.LogInformation("Found {Count} activated windows: [{Handles}], returning {ElementCount} elements",
                        activatedWindowHandles.Count,
                        string.Join(", ", activatedWindowHandles.Select(h => $"0x{h:X}")),
                        activatedElements.Count);

                    return activatedElements;
                }

                _logger.LogDebug("No focus changes detected");
                return new List<ElementInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting focus changes");
                return new List<ElementInfo>();
            }
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
                var currentElements = await CaptureAllElements(10, cancellationToken, includeDetails: true);
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
            string? sessionId = null;

            try
            {
                // Capture baseline UI elements (kept for fallback and focus detection)
                var beforeElements = await CaptureLaunchSnapshotWithRetryAsync(
                    "before-launch-baseline",
                    cancellationToken,
                    includeDetails: true);
                _logger.LogInformation("Baseline elements for {Application}: {Count} elements", application, beforeElements.Count);

                // Start monitoring for window.opened events before launch
                try
                {
                    var startRequest = new StartEventMonitoringRequest
                    {
                        EventTypes = new[] { "window.opened" }
                    };
                    var startResult = await ExecuteMonitorServiceOperationAsync<StartEventMonitoringRequest, EventMonitoringStartResult>(
                        "StartEventMonitoring", startRequest, "StartMonitoringForLaunch", 10);
                    
                    if (startResult.Success && startResult.Data != null)
                    {
                        sessionId = startResult.Data.SessionId;
                        _logger.LogInformation("Started window.opened monitoring session {SessionId} for {Application}", sessionId, application);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to start monitoring session for {Application}. Proceeding with fallback detection.", application);
                }

                // Step 1: Try Win32 application launch
                var win32LaunchResult = await TryLaunchWin32(application, arguments, workingDirectory, cancellationToken);
                if (win32LaunchResult.LaunchSucceeded)
                {
                    _logger.LogInformation("Win32 launch succeeded for {Application}, now detecting new elements", application);
                    var result = await WaitForElementsAndCreateResponse(beforeElements, application, "Win32", win32LaunchResult.LaunchedProcessId, sessionId, detectionStopwatch, cancellationToken);
                    if (result != null) return result;
                }

                // Step 2: Try UWP application launch
                var uwpLaunchResult = await TryLaunchUWP(application, cancellationToken);
                if (uwpLaunchResult.LaunchSucceeded)
                {
                    _logger.LogInformation("UWP launch succeeded for {Application}, now detecting new elements", application);
                    var result = await WaitForElementsAndCreateResponse(beforeElements, application, "UWP", null, sessionId, detectionStopwatch, cancellationToken);
                    if (result != null) return result;
                }

                // Step 3: Try Protocol URI launch (e.g., ms-settings:, mailto:, http:)
                 LaunchResult protocolLaunchResult = LaunchResult.Failure("Not attempted");
                if (IsProtocolUri(application))
                {
                    protocolLaunchResult = await TryLaunchProtocolUri(application, cancellationToken);
                    if (protocolLaunchResult.LaunchSucceeded)
                    {
                        _logger.LogInformation("Protocol URI launch succeeded for {Application}, now detecting new elements", application);
                        var result = await WaitForElementsAndCreateResponse(beforeElements, application, "Protocol URI", null, sessionId, detectionStopwatch, cancellationToken);
                        if (result != null) return result;
                    }
                }

                detectionStopwatch.Stop();
                _logger.LogWarning("All launch methods failed for application: {Application}. Win32 success: {Win32Success}, UWP success: {UwpSuccess}, Protocol success: {ProtocolSuccess}",
                    application, win32LaunchResult.LaunchSucceeded, uwpLaunchResult.LaunchSucceeded, protocolLaunchResult.LaunchSucceeded);

                var methods = IsProtocolUri(application) ? "Win32, UWP, and Protocol URI methods" : "Win32 and UWP methods";
                return ProcessLaunchResponse.CreateError($"Failed to launch application: {application}. All launch methods unsuccessful ({methods}).");
            }
            catch (Exception ex)
            {
                detectionStopwatch.Stop();
                _logger.LogError(ex, "Error launching application: {Application}", application);
                return ProcessLaunchResponse.CreateError($"Exception during launch: {ex.Message}");
            }
            finally
            {
                // Ensure monitoring session is stopped
                if (!string.IsNullOrEmpty(sessionId))
                {
                    try
                    {
                        var stopRequest = new StopEventMonitoringRequest { MonitorId = sessionId };
                        await ExecuteMonitorServiceOperationAsync<StopEventMonitoringRequest, EventMonitoringStopResult>(
                            "StopEventMonitoring", stopRequest, "StopMonitoringAfterLaunch", 10);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to stop monitoring session {SessionId}", sessionId);
                    }
                }
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

        private async Task<ProcessLaunchResponse> WaitForElementsAndCreateResponse(
            List<ElementInfo> beforeElements,
            string application,
            string launchType,
            int? launchedProcessId,
            string? monitoringSessionId,
            Stopwatch detectionStopwatch,
            CancellationToken cancellationToken)
        {
            var waitTimeMs = launchType == "Protocol URI" ? 15000 : 12000;
            const int pollIntervalMs = 100;
            const int stabilizationWindowMs = 700;
            var stopwatch = Stopwatch.StartNew();
            var allOpenedEvents = new List<TypedEventData>();
            TypedEventData? bestEvent = null;
            TypedEventData? lastEventWithHwnd = null;
            var bestScore = int.MinValue;
            long? firstCandidateSeenAtMs = null;
            var stabilizationExtended = false;

            // Event-driven detection phase (primary)
            if (!string.IsNullOrEmpty(monitoringSessionId))
            {
                while (stopwatch.ElapsedMilliseconds < waitTimeMs && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var logRequest = new GetEventLogRequest
                        {
                            MonitorId = monitoringSessionId,
                            MaxCount = 200,
                            PreserveEvents = true
                        };
                        var logResult = await ExecuteMonitorServiceOperationAsync<GetEventLogRequest, EventLogResult>(
                            "GetEventLog", logRequest, "ProcessEventsDuringLaunch", 10);

                        if (logResult.Success && logResult.Data?.Events != null)
                        {
                            var openedEvents = logResult.Data.Events
                                .Where(IsWindowOpenedEvent)
                                .ToList();

                            if (openedEvents.Count > 0)
                            {
                                allOpenedEvents.AddRange(openedEvents);

                                var candidateWithHwnd = openedEvents
                                    .Where(e => e.WindowHandle.HasValue)
                                    .OrderByDescending(e => ScoreWindowEvent(e, launchedProcessId, application))
                                    .ThenByDescending(e => e.Timestamp)
                                    .FirstOrDefault();

                                if (candidateWithHwnd != null)
                                {
                                    lastEventWithHwnd = candidateWithHwnd;
                                }

                                var candidate = SelectBestWindowEvent(allOpenedEvents, launchedProcessId, application);
                                if (candidate != null)
                                {
                                    var candidateScore = ScoreWindowEvent(candidate, launchedProcessId, application);
                                    if (candidateScore > bestScore ||
                                        (candidateScore == bestScore &&
                                         (bestEvent == null || candidate.Timestamp > bestEvent.Timestamp)))
                                    {
                                        bestEvent = candidate;
                                        bestScore = candidateScore;
                                    }

                                    if (!firstCandidateSeenAtMs.HasValue)
                                    {
                                        firstCandidateSeenAtMs = stopwatch.ElapsedMilliseconds;
                                        _logger.LogDebug(
                                            "First window.opened candidate seen for {Application}; starting stabilization window of {StabilizationWindowMs}ms",
                                            application,
                                            stabilizationWindowMs);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during event-based detection for {Application}", application);
                    }

                    if (firstCandidateSeenAtMs.HasValue &&
                        stopwatch.ElapsedMilliseconds - firstCandidateSeenAtMs.Value >= stabilizationWindowMs)
                    {
                        if (bestEvent?.WindowHandle.HasValue == true)
                        {
                            detectionStopwatch.Stop();
                            _logger.LogInformation(
                                "Detected window via stabilized window.opened event: 0x{Handle:X}, PID: {Pid}, Score: {Score}",
                                bestEvent.WindowHandle.Value,
                                bestEvent.ProcessId,
                                bestScore);

                            var responseProcessId = bestEvent.ProcessId != 0
                                ? bestEvent.ProcessId
                                : launchedProcessId.GetValueOrDefault();

                            return ProcessLaunchResponse.CreateSuccess(
                                responseProcessId,
                                application,
                                false,
                                bestEvent.SourceElement,
                                bestEvent.WindowHandle.Value,
                                usedEventBasedDetection: true);
                        }

                        if (!stabilizationExtended)
                        {
                            stabilizationExtended = true;
                            firstCandidateSeenAtMs = stopwatch.ElapsedMilliseconds - (stabilizationWindowMs - 300);
                            _logger.LogDebug(
                                "Window.opened candidate found for {Application} but no HWND yet; extending stabilization window by 300ms",
                                application);
                        }
                        else if (lastEventWithHwnd?.WindowHandle.HasValue == true)
                        {
                            detectionStopwatch.Stop();
                            var responseProcessId = lastEventWithHwnd.ProcessId != 0
                                ? lastEventWithHwnd.ProcessId
                                : launchedProcessId.GetValueOrDefault();

                            _logger.LogInformation(
                                "Detected window via last window.opened event with HWND after stabilization: 0x{Handle:X}, PID: {Pid}, Score: {Score}",
                                lastEventWithHwnd.WindowHandle.Value,
                                responseProcessId,
                                ScoreWindowEvent(lastEventWithHwnd, launchedProcessId, application));

                            return ProcessLaunchResponse.CreateSuccess(
                                responseProcessId,
                                application,
                                false,
                                lastEventWithHwnd.SourceElement,
                                lastEventWithHwnd.WindowHandle.Value,
                                usedEventBasedDetection: true);
                        }

                        _logger.LogDebug(
                            "Window.opened candidate found for {Application} but no HWND yet; continuing event polling",
                            application);
                    }

                    await Task.Delay(pollIntervalMs, cancellationToken);
                }
            }
            else
            {
                _logger.LogDebug("No monitoring session available for {Application}; skipping event-based detection phase", application);
            }

            // Final safety net: focus-change detection.
            var currentElements = await CaptureAllElements(10, cancellationToken, includeDetails: true);
            var activatedElements = FindActivatedElements(beforeElements, currentElements);

            if (launchedProcessId.HasValue && launchedProcessId.Value > 0)
            {
                activatedElements = activatedElements
                    .Where(e => e.ProcessId == launchedProcessId.Value)
                    .ToList();
            }

            if (activatedElements.Any())
            {
                detectionStopwatch.Stop();
                _logger.LogInformation("Detected window via focus change for {Application}", application);
                return CreateSuccessResponseFromElements(activatedElements, application, $"{launchType} (Activated)", detectionStopwatch.ElapsedMilliseconds);
            }

            // Final event snapshot before giving up.
            if (!string.IsNullOrEmpty(monitoringSessionId))
            {
                try
                {
                    // First, try resolving from best in-memory candidate collected during polling.
                    if (bestEvent != null)
                    {
                        var inMemoryResolved = await TryResolveWindowHandleFromSourceElementAsync(bestEvent, launchedProcessId, application, cancellationToken);
                        if (inMemoryResolved.HasValue)
                        {
                            detectionStopwatch.Stop();
                            var responseProcessId = bestEvent.ProcessId != 0
                                ? bestEvent.ProcessId
                                : launchedProcessId.GetValueOrDefault();

                            _logger.LogInformation(
                                "Resolved HWND from in-memory window.opened candidate: 0x{Handle:X}, PID: {Pid}",
                                inMemoryResolved.Value,
                                responseProcessId);

                            return ProcessLaunchResponse.CreateSuccess(
                                responseProcessId,
                                application,
                                false,
                                bestEvent.SourceElement,
                                inMemoryResolved.Value,
                                usedEventBasedDetection: true);
                        }
                    }

                    var finalLogRequest = new GetEventLogRequest
                    {
                        MonitorId = monitoringSessionId,
                        MaxCount = 200,
                        PreserveEvents = false
                    };
                    var finalLogResult = await ExecuteMonitorServiceOperationAsync<GetEventLogRequest, EventLogResult>(
                        "GetEventLog", finalLogRequest, "FinalEventSnapshotDuringLaunch", 10);

                    if (finalLogResult.Success && finalLogResult.Data?.Events != null)
                    {
                        var openedEvents = finalLogResult.Data.Events
                            .Where(IsWindowOpenedEvent)
                            .ToList();

                        var candidate = SelectBestWindowEvent(openedEvents, launchedProcessId, application);
                        if (candidate?.WindowHandle.HasValue == true)
                        {
                            detectionStopwatch.Stop();
                            var responseProcessId = candidate.ProcessId != 0
                                ? candidate.ProcessId
                                : launchedProcessId.GetValueOrDefault();

                            _logger.LogInformation(
                                "Detected window from final window.opened snapshot: 0x{Handle:X}, PID: {Pid}",
                                candidate.WindowHandle.Value,
                                responseProcessId);

                            return ProcessLaunchResponse.CreateSuccess(
                                responseProcessId,
                                application,
                                false,
                                candidate.SourceElement,
                                candidate.WindowHandle.Value,
                                usedEventBasedDetection: true);
                        }

                        var sourceResolved = await TryResolveWindowHandleFromSourceElementAsync(candidate, launchedProcessId, application, cancellationToken);
                        if (sourceResolved.HasValue)
                        {
                            detectionStopwatch.Stop();
                            var responseProcessId = candidate!.ProcessId != 0
                                ? candidate.ProcessId
                                : launchedProcessId.GetValueOrDefault();

                            _logger.LogInformation(
                                "Resolved HWND from final window.opened snapshot source: 0x{Handle:X}, PID: {Pid}",
                                sourceResolved.Value,
                                responseProcessId);

                            return ProcessLaunchResponse.CreateSuccess(
                                responseProcessId,
                                application,
                                false,
                                candidate.SourceElement,
                                sourceResolved.Value,
                                usedEventBasedDetection: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Final event snapshot failed for {Application}", application);
                }
            }

            // Win32 helper fallback: if launch PID is known, try main window handle directly.
            if (launchedProcessId.HasValue && launchedProcessId.Value > 0)
            {
                var processWindowHandle = await TryGetMainWindowHandleAsync(launchedProcessId.Value, cancellationToken);
                if (processWindowHandle.HasValue)
                {
                    detectionStopwatch.Stop();
                    _logger.LogInformation(
                        "Detected window via process main window fallback: 0x{Handle:X}, PID: {Pid}",
                        processWindowHandle.Value,
                        launchedProcessId.Value);

                    return ProcessLaunchResponse.CreateSuccess(
                        launchedProcessId.Value,
                        application,
                        false,
                        "Application launched",
                        processWindowHandle.Value,
                        usedEventBasedDetection: false);
                }
            }

            _logger.LogWarning("Window detection timed out for {Application} after {Elapsed}ms", application, stopwatch.ElapsedMilliseconds);
            return ProcessLaunchResponse.CreateError($"Application launched successfully but window not detected: {application}.");
        }

        private static async Task<long?> TryGetMainWindowHandleAsync(int processId, CancellationToken cancellationToken)
        {
            try
            {
                using var process = Process.GetProcessById(processId);

                for (var i = 0; i < 10 && !cancellationToken.IsCancellationRequested; i++)
                {
                    process.Refresh();
                    var handle = process.MainWindowHandle;
                    if (handle != IntPtr.Zero)
                    {
                        return handle.ToInt64();
                    }

                    await Task.Delay(200, cancellationToken);
                }
            }
            catch
            {
                // Best-effort fallback only.
            }

            return null;
        }

        private async Task<long?> TryResolveWindowHandleFromSourceElementAsync(TypedEventData? eventData, int? launchedProcessId, string application, CancellationToken cancellationToken)
        {
            if (eventData == null)
            {
                return null;
            }

            // Source format is typically: "System.Windows.Automation.ControlType 'Title' (AutomationId: ...)"
            var title = string.Empty;
            if (!string.IsNullOrWhiteSpace(eventData.SourceElement))
            {
                var match = Regex.Match(eventData.SourceElement, "'(?<title>[^']+)'", RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    title = match.Groups["title"].Value;
                }
            }

            var cleanAppName = Path.GetFileNameWithoutExtension(application);

            try
            {
                var searchResult = await ExecuteServiceOperationAsync<SearchElementsRequest, SearchElementsResult>(
                    "SearchElements",
                    new SearchElementsRequest
                    {
                        ControlType = "Window",
                        Scope = "children",
                        MaxResults = 20,
                        BypassCache = true
                    },
                    nameof(TryResolveWindowHandleFromSourceElementAsync),
                    10);

                if (searchResult.Success && searchResult.Data?.Elements != null)
                {
                    var matched = searchResult.Data.Elements
                        .Where(e => e.WindowHandle.HasValue)
                        .OrderByDescending(e =>
                        {
                            var score = 0;

                            if (!string.IsNullOrWhiteSpace(title) &&
                                !string.IsNullOrWhiteSpace(e.Name) &&
                                e.Name.Contains(title, StringComparison.OrdinalIgnoreCase))
                            {
                                score += 80;
                            }

                            if (!string.IsNullOrWhiteSpace(e.Name) &&
                                (e.Name.Contains(cleanAppName, StringComparison.OrdinalIgnoreCase) ||
                                 e.Name.Contains(application, StringComparison.OrdinalIgnoreCase)))
                            {
                                score += 40;
                            }

                            if (launchedProcessId.HasValue && launchedProcessId.Value > 0 && e.ProcessId == launchedProcessId.Value)
                            {
                                score += 60;
                            }

                            return score;
                        })
                        .ThenByDescending(e => e.WindowHandle)
                        .FirstOrDefault();

                    if (matched?.WindowHandle.HasValue == true)
                    {
                        return matched.WindowHandle.Value;
                    }
                }
            }
            catch
            {
                // Best effort only.
            }

            return null;
        }

        private TypedEventData? SelectBestWindowEvent(List<TypedEventData> events, int? launchedProcessId, string appName)
        {
            return events
                .OrderByDescending(e => ScoreWindowEvent(e, launchedProcessId, appName))
                .ThenByDescending(e => e.Timestamp)
                .FirstOrDefault();
        }

        private static int ScoreWindowEvent(TypedEventData eventData, int? launchedProcessId, string appName)
        {
            var score = 0;
            var cleanAppName = Path.GetFileNameWithoutExtension(appName);
            var launchedProcessIdValue = launchedProcessId.GetValueOrDefault();
            var launchedProcessKnown = launchedProcessIdValue > 0;

            if (launchedProcessKnown && eventData.ProcessId == launchedProcessIdValue)
            {
                score += 100;
            }

            if (eventData.ProcessId <= 0)
            {
                score -= 15;
            }

            if (eventData.WindowHandle.HasValue)
            {
                score += 40;
            }

            if (!string.IsNullOrWhiteSpace(eventData.SourceElement) &&
                (eventData.SourceElement.Contains(cleanAppName, StringComparison.OrdinalIgnoreCase) ||
                 eventData.SourceElement.Contains(appName, StringComparison.OrdinalIgnoreCase)))
            {
                score += 25;
            }

            if (!launchedProcessKnown &&
                !string.IsNullOrWhiteSpace(eventData.SourceElement) &&
                eventData.SourceElement.Contains(cleanAppName, StringComparison.OrdinalIgnoreCase))
            {
                score += 20;
            }

            if (eventData.EventType.Equals("window.opened", StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }

            return score;
        }

        private static bool IsWindowOpenedEvent(TypedEventData eventData)
        {
            if (eventData.EventType.Equals("window.opened", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var eventId = eventData switch
            {
                GenericEventData generic => generic.EventId,
                InvokeEventData invoke => invoke.EventId,
                SelectionEventData selection => selection.EventId,
                TextChangedEventData text => text.EventId,
                _ => string.Empty
            };

            return !string.IsNullOrWhiteSpace(eventId) &&
                   eventId.Contains("WindowOpened", StringComparison.OrdinalIgnoreCase);
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

                // Track the launched process via Job Object (D-plan: tracking only, no auto-kill)
                if (process != null && _launchedAppsJobObject != null)
                {
                    try
                    {
                        _launchedAppsJobObject.AssignProcess(process);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to track launched process PID {ProcessId} in Job Object", launchedProcessId);
                    }
                }

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

        /// <summary>
        /// Try to launch a protocol URI (e.g., ms-settings:, mailto:, http:)
        /// </summary>
        private Task<LaunchResult> TryLaunchProtocolUri(string protocolUri, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting Protocol URI launch for {ProtocolUri}", protocolUri);

                var processInfo = new ProcessStartInfo
                {
                    FileName = protocolUri,
                    UseShellExecute = true, // Required for protocol URIs
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                _logger.LogInformation("Started protocol URI {ProtocolUri}", protocolUri);

                // Return success with process info (process may be null for protocol URIs)
                return Task.FromResult(LaunchResult.Success(process?.Id ?? 0, protocolUri));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Protocol URI launch failed for {ProtocolUri}", protocolUri);
                return Task.FromResult(LaunchResult.Failure($"Protocol URI launch failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Try to launch application as UWP app
        /// </summary>
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
                metadata.UsedUIAutomationDetection = true; 
                metadata.UsedEventBasedDetection = response.UsedEventBasedDetection;
            }

            return metadata;
        }

        /// <inheritdoc />
        public int[] GetTrackedProcessIds()
        {
            if (_disposed || _launchedAppsJobObject == null)
                return Array.Empty<int>();

            return _launchedAppsJobObject.GetProcessIds();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _launchedAppsJobObject?.Dispose();
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
