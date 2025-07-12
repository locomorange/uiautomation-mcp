using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowInteractionStateOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetWindowInteractionStateOperation(ElementFinderService elementFinderService)
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

                var interactionState = windowPattern.Current.WindowInteractionState;
                var stateInfo = new Dictionary<string, object>
                {
                    ["InteractionState"] = interactionState.ToString(),
                    ["InteractionStateValue"] = (int)interactionState,
                    ["Description"] = GetInteractionStateDescription(interactionState)
                };

                return Task.FromResult(new OperationResult { Success = true, Data = stateInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting window interaction state: {ex.Message}" });
            }
        }

        private static string GetInteractionStateDescription(WindowInteractionState state)
        {
            return state switch
            {
                WindowInteractionState.Running => "The window is running and responding to user input",
                WindowInteractionState.Closing => "The window is in the process of closing",
                WindowInteractionState.ReadyForUserInteraction => "The window is ready for user interaction",
                WindowInteractionState.BlockedByModalWindow => "The window is blocked by a modal window",
                WindowInteractionState.NotResponding => "The window is not responding",
                _ => "Unknown interaction state"
            };
        }
    }
}