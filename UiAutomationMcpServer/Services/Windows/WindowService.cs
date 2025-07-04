using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;

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

        public WindowService(ILogger<WindowService> logger)
        {
            _logger = logger;
        }

        public Task<OperationResult> GetWindowInfoAsync()
        {
            try
            {
                var windows = new List<WindowInfo>();
                AutomationElementCollection? windowElements = null;
                
                try
                {
                    windowElements = AutomationElement.RootElement.FindAll(TreeScope.Children,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Failed to find windows: {ex.Message}" });
                }

                if (windowElements == null)
                {
                    return Task.FromResult(new OperationResult { Success = true, Data = windows });
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

                return Task.FromResult(new OperationResult { Success = true, Data = windows });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"GetWindowInfo failed: {ex.Message}" });
            }
        }

        public async Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null)
        {
            try
            {
                _logger.LogInformation("Launching application: {ApplicationPath} with arguments: {Arguments}", 
                    applicationPath, arguments);

                var startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(applicationPath) ?? "",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                    return new ProcessResult { Success = false, Error = "Failed to start process" };

                await Task.Delay(1000);

                _logger.LogInformation("Application launched with PID: {ProcessId}", process.Id);

                return new ProcessResult
                {
                    Success = true,
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    HasExited = process.HasExited
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching application");
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