using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetWindowInfoOperation> _logger;

        public GetWindowInfoOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetWindowInfoOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetWindowInfoRequest>(parametersJson)!;
                
                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;

                var windowElement = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (windowElement == null)
                {
                    return Task.FromResult(new OperationResult 
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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWindowInfo operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get window info: {ex.Message}",
                    Data = new WindowInfoResult()
                });
            }
        }
    }
}