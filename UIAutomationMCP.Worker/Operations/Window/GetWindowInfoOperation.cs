using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetWindowInfoOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<WindowInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            // Try typed request first, fallback to legacy dictionary method
            var typedRequest = request.GetTypedRequest<GetWindowInfoRequest>(_options);
            
            string windowTitle;
            int processId;
            
            if (typedRequest != null)
            {
                // Type-safe parameter access
                windowTitle = typedRequest.WindowTitle ?? "";
                processId = typedRequest.ProcessId ?? 0;
            }
            else
            {
                // Legacy method (for backward compatibility)
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            }

            try
            {
                AutomationElement? windowElement = null;
                
                // Find window by process ID or title
                if (processId > 0)
                {
                    var processCondition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                    windowElement = AutomationElement.RootElement.FindFirst(TreeScope.Children, processCondition);
                }
                else if (!string.IsNullOrEmpty(windowTitle))
                {
                    var titleCondition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                    windowElement = AutomationElement.RootElement.FindFirst(TreeScope.Children, titleCondition);
                }

                if (windowElement == null)
                {
                    return Task.FromResult(new OperationResult<WindowInfoResult> 
                    { 
                        Success = false, 
                        Error = "Window not found",
                        Data = new WindowInfoResult()
                    });
                }

                var windowRect = windowElement.Current.BoundingRectangle;
                var result = new WindowInfoResult
                {
                    Title = windowElement.Current.Name,
                    ClassName = windowElement.Current.ClassName,
                    ProcessId = windowElement.Current.ProcessId,
                    Handle = windowElement.Current.NativeWindowHandle,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = windowRect.X,
                        Y = windowRect.Y,
                        Width = windowRect.Width,
                        Height = windowRect.Height
                    },
                    IsEnabled = windowElement.Current.IsEnabled,
                    IsVisible = !windowElement.Current.IsOffscreen,
                    WindowState = "Normal", // Default
                    CanMaximize = false,
                    CanMinimize = false,
                    IsModal = false,
                    IsTopmost = false
                };

                // Get process name
                try
                {
                    var process = Process.GetProcessById(result.ProcessId);
                    result.ProcessName = process.ProcessName;
                }
                catch
                {
                    result.ProcessName = "";
                }

                // Get window state if WindowPattern is available
                if (windowElement.TryGetCurrentPattern(WindowPattern.Pattern, out var windowPattern) && windowPattern is WindowPattern wp)
                {
                    result.WindowState = wp.Current.WindowVisualState.ToString();
                    result.CanMaximize = wp.Current.CanMaximize;
                    result.CanMinimize = wp.Current.CanMinimize;
                    result.IsModal = wp.Current.IsModal;
                    result.IsTopmost = wp.Current.IsTopmost;
                }

                return Task.FromResult(new OperationResult<WindowInfoResult> 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<WindowInfoResult> 
                { 
                    Success = false, 
                    Error = $"Error getting window info: {ex.Message}",
                    Data = new WindowInfoResult()
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