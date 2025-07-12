using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowCapabilitiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetWindowCapabilitiesOperation(ElementFinderService elementFinderService)
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
                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Window not found" });

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                    return Task.FromResult(new OperationResult { Success = false, Error = "WindowPattern not supported" });

                var capabilities = new Dictionary<string, object>
                {
                    ["Maximizable"] = windowPattern.Current.CanMaximize,
                    ["Minimizable"] = windowPattern.Current.CanMinimize,
                    ["CanMaximize"] = windowPattern.Current.CanMaximize,
                    ["CanMinimize"] = windowPattern.Current.CanMinimize,
                    ["IsModal"] = windowPattern.Current.IsModal,
                    ["IsTopmost"] = windowPattern.Current.IsTopmost,
                    ["WindowVisualState"] = windowPattern.Current.WindowVisualState.ToString(),
                    ["WindowInteractionState"] = windowPattern.Current.WindowInteractionState.ToString()
                };

                return Task.FromResult(new OperationResult { Success = true, Data = capabilities });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting window capabilities: {ex.Message}" });
            }
        }
    }
}