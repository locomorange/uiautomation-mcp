using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ElementInspection
{
    public class GetElementPropertiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetElementPropertiesOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            var properties = new Dictionary<string, object>
            {
                ["AutomationId"] = element.Current.AutomationId,
                ["Name"] = element.Current.Name,
                ["ControlType"] = element.Current.ControlType.LocalizedControlType,
                ["LocalizedControlType"] = element.Current.LocalizedControlType,
                ["IsEnabled"] = element.Current.IsEnabled,
                ["IsVisible"] = !element.Current.IsOffscreen,
                ["HasKeyboardFocus"] = element.Current.HasKeyboardFocus,
                ["IsKeyboardFocusable"] = element.Current.IsKeyboardFocusable,
                ["IsContentElement"] = element.Current.IsContentElement,
                ["IsControlElement"] = element.Current.IsControlElement,
                ["ClassName"] = element.Current.ClassName,
                ["ProcessId"] = element.Current.ProcessId,
                ["FrameworkId"] = element.Current.FrameworkId,
                ["AcceleratorKey"] = element.Current.AcceleratorKey,
                ["AccessKey"] = element.Current.AccessKey,
                ["HelpText"] = element.Current.HelpText,
                ["ItemStatus"] = element.Current.ItemStatus,
                ["ItemType"] = element.Current.ItemType,
                ["BoundingRectangle"] = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                }
            };

            return Task.FromResult(new OperationResult { Success = true, Data = properties });
        }
    }
}