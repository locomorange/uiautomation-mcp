using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlType
{
    public class ButtonOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public ButtonOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var operation = request.Parameters?.GetValueOrDefault("operation")?.ToString() ?? "click";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (element.Current.ControlType != System.Windows.Automation.ControlType.Button)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element is not a button" });

            try
            {
                switch (operation.ToLower())
                {
                    case "click":
                    case "invoke":
                        if (element.TryGetCurrentPattern(InvokePattern.Pattern, out var invokePattern) && invokePattern is InvokePattern invoke)
                        {
                            invoke.Invoke();
                            return Task.FromResult(new OperationResult { Success = true });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Button does not support invoke" });

                    case "toggle":
                        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) && togglePattern is TogglePattern toggle)
                        {
                            toggle.Toggle();
                            return Task.FromResult(new OperationResult { Success = true, Data = toggle.Current.ToggleState.ToString() });
                        }
                        return Task.FromResult(new OperationResult { Success = false, Error = "Button does not support toggle" });

                    case "getinfo":
                        var buttonInfo = new Dictionary<string, object>
                        {
                            ["Name"] = element.Current.Name,
                            ["AutomationId"] = element.Current.AutomationId,
                            ["IsEnabled"] = element.Current.IsEnabled,
                            ["IsVisible"] = !element.Current.IsOffscreen,
                            ["SupportedPatterns"] = element.GetSupportedPatterns().Select(p => p.ProgrammaticName).ToList()
                        };
                        return Task.FromResult(new OperationResult { Success = true, Data = buttonInfo });

                    default:
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Unknown operation: {operation}" });
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error performing button operation: {ex.Message}" });
            }
        }
    }
}