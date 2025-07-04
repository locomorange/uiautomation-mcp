using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Windows
{
    public interface IWindowService
    {
        Task<OperationResult> GetWindowInfoAsync();
        Task<OperationResult<List<WindowInfo>>> GetWindowsAsync();
        Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null);
        Task<OperationResult<ElementInfo?>> FindWindowByTitleAsync(string title, int? processId = null);
    }

    public class WindowService : IWindowService
    {
        private readonly ILogger<WindowService> _logger;
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public WindowService(ILogger<WindowService> logger, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _uiAutomationWorker = uiAutomationWorker;
        }

        public async Task<OperationResult> GetWindowInfoAsync()
        {
            try
            {
                _logger.LogInformation("Getting window info via UIAutomationWorker");
                
                var windowElementsResult = await _uiAutomationWorker.FindAllAsync(
                    windowTitle: null,
                    searchText: null,
                    controlType: "Window",
                    processId: null,
                    timeoutSeconds: 30);
                
                if (!windowElementsResult.Success)
                {
                    _logger.LogError("Failed to get window elements: {Error}", windowElementsResult.Error);
                    return new OperationResult { Success = false, Error = windowElementsResult.Error ?? "Failed to find windows" };
                }
                
                var elementInfos = windowElementsResult.Data ?? new List<ElementInfo>();
                var windows = new List<WindowInfo>();
                
                foreach (var elementInfo in elementInfos)
                {
                    var windowInfo = new WindowInfo
                    {
                        Name = elementInfo.Name,
                        AutomationId = elementInfo.AutomationId,
                        ProcessId = elementInfo.ProcessId,
                        ClassName = elementInfo.ClassName,
                        BoundingRectangle = elementInfo.BoundingRectangle,
                        IsEnabled = elementInfo.IsEnabled,
                        IsVisible = elementInfo.IsVisible
                    };
                    
                    windows.Add(windowInfo);
                }
                
                _logger.LogInformation("Found {WindowCount} windows", windows.Count);
                return new OperationResult { Success = true, Data = windows };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting window info");
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult<List<WindowInfo>>> GetWindowsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all windows");
                
                var result = await _uiAutomationWorker.GetWindowsAsync();
                
                if (result.Success && result.Data != null)
                {
                    _logger.LogInformation("Found {Count} windows", result.Data.Count);
                    return result;
                }
                
                _logger.LogError("Failed to get windows: {Error}", result.Error);
                return new OperationResult<List<WindowInfo>>
                {
                    Success = false,
                    Error = result.Error ?? "Failed to get windows"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting windows");
                return new OperationResult<List<WindowInfo>>
                {
                    Success = false,
                    Error = $"Error getting windows: {ex.Message}"
                };
            }
        }

        public async Task<ProcessResult> LaunchApplicationAsync(string applicationPath, string? arguments = null, string? workingDirectory = null)
        {
            try
            {
                _logger.LogInformation("Launching application: {ApplicationPath}", applicationPath);

                if (!File.Exists(applicationPath))
                {
                    return new ProcessResult { Success = false, Error = $"Application not found: {applicationPath}" };
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = arguments ?? "",
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(applicationPath) ?? "",
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    return new ProcessResult { Success = false, Error = "Failed to start process" };
                }

                await Task.Delay(1000);

                var hasExited = process.HasExited;
                var processId = hasExited ? 0 : process.Id;
                var processName = hasExited ? "" : process.ProcessName;

                _logger.LogInformation("Application launched: ProcessId={ProcessId}, ProcessName={ProcessName}, HasExited={HasExited}",
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

        public async Task<OperationResult<ElementInfo?>> FindWindowByTitleAsync(string title, int? processId = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = "Window title cannot be empty"
                };
            }
            
            try
            {
                _logger.LogDebug("FindWindowByTitleAsync called with title: '{Title}', processId: {ProcessId}", title, processId);
                
                var searchParams = new ElementSearchParameters
                {
                    SearchText = title,
                    ControlType = "Window",
                    ProcessId = processId,
                    TreeScope = "children"
                };

                var result = await _uiAutomationWorker.FindFirstElementAsync(searchParams, timeoutSeconds: 15);
                
                if (!result.Success || result.Data == null)
                {
                    _logger.LogDebug("Window not found with exact title. Trying with partial match...");
                    
                    var allWindowsResult = await _uiAutomationWorker.FindAllElementsAsync(new ElementSearchParameters
                    {
                        ControlType = "Window",
                        ProcessId = processId,
                        TreeScope = "children"
                    }, timeoutSeconds: 15);
                    
                    if (allWindowsResult.Success && allWindowsResult.Data != null)
                    {
                        var matchingWindow = allWindowsResult.Data
                            .Where(w => !string.IsNullOrEmpty(w.Name))
                            .FirstOrDefault(w => 
                                string.Equals(w.Name.Trim(), title.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                w.Name.Contains(title, StringComparison.OrdinalIgnoreCase) ||
                                title.Contains(w.Name, StringComparison.OrdinalIgnoreCase));
                        
                        if (matchingWindow != null)
                        {
                            _logger.LogDebug("Partial match found: '{Name}' (ProcessId: {ProcessId})", matchingWindow.Name, matchingWindow.ProcessId);
                            return new OperationResult<ElementInfo?>
                            {
                                Success = true,
                                Data = matchingWindow
                            };
                        }
                    }
                    
                    return new OperationResult<ElementInfo?>
                    {
                        Success = false,
                        Error = $"Window with title '{title}' not found"
                    };
                }
                
                _logger.LogDebug("Window found: '{Name}' (ProcessId: {ProcessId})", result.Data.Name, result.Data.ProcessId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding window by title: {Title}", title);
                return new OperationResult<ElementInfo?>
                {
                    Success = false,
                    Error = $"Error finding window: {ex.Message}"
                };
            }
        }
    }
}
