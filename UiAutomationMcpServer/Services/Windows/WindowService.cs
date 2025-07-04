using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Windows
{
    public interface IWindowService
    {
        Task<OperationResult> GetWindowInfoAsync();
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null);
        AutomationElement? FindWindowByTitle(string title, int? processId = null);
    }

    public class WindowService : IWindowService
    {
        private readonly ILogger<WindowService> _logger;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public WindowService(ILogger<WindowService> logger, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> GetWindowInfoAsync()
        {
            try
            {
                var windows = new List<WindowInfo>();
                
                var windowElementsResult = await _uiAutomationHelper.FindAllAsync(
                    AutomationElement.RootElement,
                    TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                    30);
                
                if (!windowElementsResult.Success)
                {
                    return new OperationResult { Success = false, Error = windowElementsResult.Error ?? "Failed to find windows" };
                }
                
                var windowElements = windowElementsResult.Data;

                if (windowElements == null)
                {
                    return new OperationResult { Success = true, Data = windows };
                }

                foreach (AutomationElement window in windowElements)
                {
                    try
                    {
                        var name = window?.Current.Name ?? "";
                        if (!string.IsNullOrEmpty(name) && window != null)
                        {
                            windows.Add(new WindowInfo
                            {
                                Name = name,
                                AutomationId = window.Current.AutomationId ?? "",
                                ProcessId = window.Current.ProcessId,
                                ClassName = window.Current.ClassName ?? "",
                                BoundingRectangle = new BoundingRectangle
                                {
                                    X = double.IsInfinity(window.Current.BoundingRectangle.X) ? 0 : window.Current.BoundingRectangle.X,
                                    Y = double.IsInfinity(window.Current.BoundingRectangle.Y) ? 0 : window.Current.BoundingRectangle.Y,
                                    Width = double.IsInfinity(window.Current.BoundingRectangle.Width) ? 0 : window.Current.BoundingRectangle.Width,
                                    Height = double.IsInfinity(window.Current.BoundingRectangle.Height) ? 0 : window.Current.BoundingRectangle.Height
                                },
                                IsEnabled = window.Current.IsEnabled,
                                IsVisible = !window.Current.IsOffscreen
                            });
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                return new OperationResult { Success = true, Data = windows };
            }
            catch (Exception ex)
            {
                return new OperationResult { Success = false, Error = $"GetWindowInfo failed: {ex.Message}" };
            }
        }

        public async Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null)
        {
            try
            {
                _logger.LogInformation("Launching application: {ApplicationPath} with arguments: {Arguments}", 
                    applicationPath, arguments);

                // Check if this is a UWP app or URI scheme
                var isUwpOrUri = applicationPath.StartsWith("ms-") || 
                                applicationPath.Contains("shell:appsFolder") ||
                                applicationPath.EndsWith(".appx") ||
                                applicationPath.Contains("WindowsApps");

                ProcessStartInfo startInfo;
                
                if (isUwpOrUri)
                {
                    // Use PowerShell to launch UWP apps or URI schemes
                    var command = applicationPath.StartsWith("ms-") 
                        ? $"Start-Process '{applicationPath}'"
                        : $"Start-Process '{applicationPath}' {(string.IsNullOrEmpty(arguments) ? "" : $"-ArgumentList '{arguments}'")}";
                    
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-WindowStyle Hidden -Command \"{command}\"",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                }
                else
                {
                    // Traditional exe launch
                    startInfo = new ProcessStartInfo
                    {
                        FileName = applicationPath,
                        Arguments = arguments ?? "",
                        WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(applicationPath) ?? "",
                        UseShellExecute = true
                    };
                }

                var process = Process.Start(startInfo);
                if (process == null)
                    return new ProcessResult { Success = false, Error = "Failed to start process" };

                // Wait longer for UWP apps to initialize
                var waitTime = isUwpOrUri ? 3000 : 1000;
                await Task.Delay(waitTime);

                // Try to get process information safely
                int processId = 0;
                string processName = "";
                bool hasExited = false;

                try
                {
                    processId = process.Id;
                    processName = process.ProcessName;
                    hasExited = process.HasExited;
                }
                catch (InvalidOperationException)
                {
                    // Process information not available (common with UWP apps)
                    _logger.LogInformation("Process information not available - likely UWP app launched successfully");
                    
                    // For UWP apps, try to find the actual app process by window title
                    if (isUwpOrUri)
                    {
                        // Wait a bit more for the app window to appear
                        await Task.Delay(2000);
                        
                        // Try to find common UWP app processes
                        var commonAppNames = new[] { "Calculator", "WindowsAlarms", "Microsoft.Windows.Alarms", "ApplicationFrameHost" };
                        foreach (var appName in commonAppNames)
                        {
                            try
                            {
                                var processes = Process.GetProcessesByName(appName);
                                if (processes.Length > 0)
                                {
                                    var latestProcess = processes.OrderByDescending(p => p.StartTime).FirstOrDefault();
                                    if (latestProcess != null)
                                    {
                                        processId = latestProcess.Id;
                                        processName = latestProcess.ProcessName;
                                        hasExited = latestProcess.HasExited;
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug("Could not check process {AppName}: {Error}", appName, ex.Message);
                            }
                        }
                    }
                }

                _logger.LogInformation("Application launched - PID: {ProcessId}, Name: {ProcessName}, HasExited: {HasExited}", 
                    processId, processName, hasExited);

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
                _logger.LogError(ex, "Error launching application: {ApplicationPath}", applicationPath);
                return new ProcessResult { Success = false, Error = ex.Message };
            }
        }

        public AutomationElement? FindWindowByTitle(string title, int? processId = null)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            
            try
            {
                _logger.LogDebug("FindWindowByTitle called with title: '{Title}', processId: {ProcessId}", title, processId);
                
                var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                
                _logger.LogDebug("Found {WindowCount} total windows", windows.Count);
                
                var matchingWindows = new List<AutomationElement>();
                
                foreach (AutomationElement window in windows)
                {
                    try
                    {
                        var name = window.Current.Name;
                        var windowProcessId = window.Current.ProcessId;
                        _logger.LogDebug("Checking window: '{Name}' (ProcessId: {ProcessId})", name, windowProcessId);
                        
                        if (processId.HasValue && windowProcessId != processId.Value)
                        {
                            _logger.LogDebug("Skipping window with ProcessId {WindowProcessId} (looking for {TargetProcessId})", windowProcessId, processId.Value);
                            continue;
                        }
                        
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (string.Equals(name.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Exact match found: '{Name}' (ProcessId: {ProcessId})", name, windowProcessId);
                                matchingWindows.Add(window);
                            }
                            else if (name.Contains(title, StringComparison.OrdinalIgnoreCase) || 
                                     title.Contains(name, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogDebug("Partial match found: '{Name}' (ProcessId: {ProcessId})", name, windowProcessId);
                                matchingWindows.Add(window);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error reading window properties");
                        continue;
                    }
                }
                
                _logger.LogDebug("Found {MatchingCount} matching windows", matchingWindows.Count);
                
                if (matchingWindows.Count == 0)
                {
                    _logger.LogDebug("No matching windows found for title: '{Title}'", title);
                    return null;
                }
                
                if (processId.HasValue)
                {
                    var selectedWindow = matchingWindows[0];
                    _logger.LogDebug("Selected window for processId {ProcessId}: '{Name}'", processId.Value, selectedWindow.Current.Name);
                    return selectedWindow;
                }
                
                foreach (var window in matchingWindows)
                {
                    try
                    {
                        if (window.Current.IsEnabled && !window.Current.IsOffscreen)
                        {
                            _logger.LogDebug("Returning first visible/enabled window: '{Name}' (ProcessId: {ProcessId})", window.Current.Name, window.Current.ProcessId);
                            return window;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking window state");
                        continue;
                    }
                }
                
                var firstWindow = matchingWindows[0];
                _logger.LogDebug("Returning first matching window: '{Name}' (ProcessId: {ProcessId})", firstWindow.Current.Name, firstWindow.Current.ProcessId);
                return firstWindow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in window search for title: '{Title}'", title);
                return null;
            }
        }
    }
}