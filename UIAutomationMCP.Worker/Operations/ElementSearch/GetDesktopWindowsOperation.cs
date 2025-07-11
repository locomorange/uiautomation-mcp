using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Worker.Operations.ElementSearch
{
    public class GetDesktopWindowsOperation : IUIAutomationOperation
    {
        private readonly ILogger<GetDesktopWindowsOperation> _logger;

        public GetDesktopWindowsOperation(ILogger<GetDesktopWindowsOperation> logger)
        {
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            try
            {
                _logger.LogInformation("Starting GetDesktopWindows operation");
                
                _logger.LogInformation("Getting root element...");
                var rootElement = AutomationElement.RootElement;
                if (rootElement == null)
                {
                    _logger.LogError("Root element is null!");
                    return Task.FromResult(new OperationResult
                    {
                        Success = false,
                        Error = "Root element is null - UIAutomation may not be available"
                    });
                }
                
                _logger.LogInformation("Root element obtained: {RootElementName}", rootElement.Current.Name ?? "null");
                
                _logger.LogInformation("Creating condition for windows...");
                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window);
                
                _logger.LogInformation("Finding all windows...");
                var windows = rootElement.FindAll(TreeScope.Children, condition);
                _logger.LogInformation("Found {WindowCount} windows", windows?.Count ?? 0);
                
                var windowList = new List<WindowInfo>();
                if (windows != null)
                {
                    foreach (AutomationElement window in windows)
                    {
                        if (window != null)
                        {
                            try
                            {
                                var windowInfo = new WindowInfo
                                {
                                    Name = window.Current.Name,
                                    ProcessId = window.Current.ProcessId,
                                    AutomationId = window.Current.AutomationId
                                };
                                windowList.Add(windowInfo);
                                _logger.LogDebug("Added window: {WindowName} (PID: {ProcessId})", windowInfo.Name, windowInfo.ProcessId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to get properties for window");
                            }
                        }
                    }
                }

                _logger.LogDebug("GetDesktopWindows operation completed successfully with {Count} windows", windowList.Count);
                return Task.FromResult(new OperationResult
                {
                    Success = true,
                    Data = windowList
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
                
                return Task.FromResult(new OperationResult
                {
                    Success = false,
                    Error = errorMessage
                });
            }
        }
    }
}