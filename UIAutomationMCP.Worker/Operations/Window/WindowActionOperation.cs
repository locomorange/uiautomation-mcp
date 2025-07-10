using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WindowActionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public WindowActionOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var action = request.Parameters?.GetValueOrDefault("action")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
            if (window == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Window not found" });

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "WindowPattern not supported" });

            switch (action.ToLowerInvariant())
            {
                case "minimize":
                    windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                    break;
                case "maximize":
                    windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                    break;
                case "normal":
                case "restore":
                    windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                    break;
                case "close":
                    windowPattern.Close();
                    break;
                default:
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Unsupported window action: {action}" });
            }

            return Task.FromResult(new OperationResult { Success = true, Data = $"Window action '{action}' performed successfully" });
        }
    }
}