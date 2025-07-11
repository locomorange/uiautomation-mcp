using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowInfoOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetWindowInfoOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

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
                    return Task.FromResult(new OperationResult { Success = false, Error = "Window not found" });
                }

                var windowRect = windowElement.Current.BoundingRectangle;
                var windowInfo = new Dictionary<string, object>
                {
                    ["Title"] = windowElement.Current.Name,
                    ["ProcessId"] = windowElement.Current.ProcessId,
                    ["AutomationId"] = windowElement.Current.AutomationId,
                    ["ClassName"] = windowElement.Current.ClassName,
                    ["ControlType"] = windowElement.Current.ControlType.LocalizedControlType,
                    ["IsEnabled"] = windowElement.Current.IsEnabled,
                    ["IsOffscreen"] = windowElement.Current.IsOffscreen,
                    ["BoundingRectangle"] = new Dictionary<string, object>
                    {
                        ["X"] = windowRect.X,
                        ["Y"] = windowRect.Y,
                        ["Width"] = windowRect.Width,
                        ["Height"] = windowRect.Height
                    },
                    ["WindowPatternAvailable"] = windowElement.GetSupportedPatterns().Any(p => p == WindowPattern.Pattern),
                    ["TransformPatternAvailable"] = windowElement.GetSupportedPatterns().Any(p => p == TransformPattern.Pattern)
                };

                // Get window state if WindowPattern is available
                if (windowElement.TryGetCurrentPattern(WindowPattern.Pattern, out var windowPattern) && windowPattern is WindowPattern wp)
                {
                    windowInfo["WindowState"] = wp.Current.WindowVisualState.ToString();
                    windowInfo["CanMaximize"] = wp.Current.CanMaximize;
                    windowInfo["CanMinimize"] = wp.Current.CanMinimize;
                    windowInfo["IsModal"] = wp.Current.IsModal;
                    windowInfo["IsTopmost"] = wp.Current.IsTopmost;
                }

                return Task.FromResult(new OperationResult { Success = true, Data = windowInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting window info: {ex.Message}" });
            }
        }
    }
}