using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class GetDesktopWindowsOperation : IUIAutomationOperation
    {
        private readonly ILogger<GetDesktopWindowsOperation> _logger;

        public GetDesktopWindowsOperation(ILogger<GetDesktopWindowsOperation> logger)
        {
            _logger = logger;
        }

        public Task<OperationResult<DesktopWindowsResult>> ExecuteAsync(WorkerRequest request)
        {
            try
            {
                _logger.LogInformation("Starting GetDesktopWindows operation");
                
                _logger.LogInformation("Getting root element...");
                var rootElement = AutomationElement.RootElement;
                if (rootElement == null)
                {
                    _logger.LogError("Root element is null!");
                    return Task.FromResult(new OperationResult<DesktopWindowsResult>
                    {
                        Success = false,
                        Error = "Root element is null - UIAutomation may not be available",
                        Data = new DesktopWindowsResult()
                    });
                }
                
                _logger.LogInformation("Root element obtained: {RootElementName}", rootElement.Current.Name ?? "null");
                
                _logger.LogInformation("Creating condition for windows...");
                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window);
                
                _logger.LogInformation("Finding all windows...");
                var windows = rootElement.FindAll(TreeScope.Children, condition);
                _logger.LogInformation("Found {WindowCount} windows", windows?.Count ?? 0);
                
                var result = new DesktopWindowsResult();
                if (windows != null)
                {
                    foreach (AutomationElement window in windows)
                    {
                        if (window != null)
                        {
                            try
                            {
                                var processName = "";
                                try
                                {
                                    var process = Process.GetProcessById(window.Current.ProcessId);
                                    processName = process.ProcessName;
                                }
                                catch { }

                                var windowInfo = new UIAutomationMCP.Shared.Results.WindowInfo
                                {
                                    Title = window.Current.Name,
                                    ClassName = window.Current.ClassName,
                                    ProcessId = window.Current.ProcessId,
                                    ProcessName = processName,
                                    Handle = window.Current.NativeWindowHandle,
                                    IsVisible = !window.Current.IsOffscreen,
                                    IsEnabled = window.Current.IsEnabled,
                                    BoundingRectangle = new BoundingRectangle
                                    {
                                        X = window.Current.BoundingRectangle.X,
                                        Y = window.Current.BoundingRectangle.Y,
                                        Width = window.Current.BoundingRectangle.Width,
                                        Height = window.Current.BoundingRectangle.Height
                                    }
                                };
                                result.Windows.Add(windowInfo);
                                _logger.LogDebug("Added window: {WindowName} (PID: {ProcessId})", windowInfo.Title, windowInfo.ProcessId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to get properties for window");
                            }
                        }
                    }
                }

                _logger.LogDebug("GetDesktopWindows operation completed successfully with {Count} windows", result.Count);
                return Task.FromResult(new OperationResult<DesktopWindowsResult>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDesktopWindows operation failed - Exception type: {ExceptionType}, Message: {Message}", 
                    ex.GetType().Name, ex.Message);
                
                var errorMessage = $"Failed to get desktop windows: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                
                return Task.FromResult(new OperationResult<DesktopWindowsResult>
                {
                    Success = false,
                    Error = errorMessage,
                    Data = new DesktopWindowsResult()
                });
            }
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}