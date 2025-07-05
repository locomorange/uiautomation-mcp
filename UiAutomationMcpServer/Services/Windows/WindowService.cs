using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UiAutomationMcp.Models;

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
            var windowElementsResult = await _uiAutomationWorker.FindAllAsync(
                windowTitle: null,
                searchText: null,
                controlType: "Window",
                processId: null,
                timeoutSeconds: 30);
            
            if (!windowElementsResult.Success)
            {
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
            
            return new OperationResult { Success = true, Data = windows };
        }

        public async Task<OperationResult<List<WindowInfo>>> GetWindowsAsync()
        {
            var result = await _uiAutomationWorker.GetWindowsAsync();
            return new OperationResult<List<WindowInfo>>
            {
                Success = result.Success,
                Data = result.Data,
                Error = result.Error
            };
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
            
            return result;
        }
    }
}
