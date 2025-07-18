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
    public class GetWindowInteractionStateOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetWindowInteractionStateOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<WindowInteractionStateResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetWindowInteractionStateRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<WindowInteractionStateResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new WindowInteractionStateResult()
                });
            
            var windowTitle = typedRequest.WindowTitle ?? "";
            var processId = typedRequest.ProcessId ?? 0;

            try
            {
                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                {
                    var failureResult = new WindowInteractionStateResult
                    {
                        InteractionState = "Unknown",
                        Description = "Window not found"
                    };
                    return Task.FromResult(new OperationResult<WindowInteractionStateResult> { Success = false, Error = "Window not found", Data = failureResult });
                }

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                {
                    var failureResult = new WindowInteractionStateResult
                    {
                        InteractionState = "Unknown",
                        Description = "WindowPattern not supported"
                    };
                    return Task.FromResult(new OperationResult<WindowInteractionStateResult> { Success = false, Error = "WindowPattern not supported", Data = failureResult });
                }

                var interactionState = windowPattern.Current.WindowInteractionState;
                var stateInfo = new WindowInteractionStateResult
                {
                    InteractionState = interactionState.ToString(),
                    InteractionStateValue = (int)interactionState,
                    Description = GetInteractionStateDescription(interactionState)
                };

                return Task.FromResult(new OperationResult<WindowInteractionStateResult> { Success = true, Data = stateInfo });
            }
            catch (Exception ex)
            {
                var failureResult = new WindowInteractionStateResult
                {
                    InteractionState = "Error",
                    Description = $"Error getting window interaction state: {ex.Message}"
                };
                return Task.FromResult(new OperationResult<WindowInteractionStateResult> { Success = false, Error = $"Error getting window interaction state: {ex.Message}", Data = failureResult });
            }
        }

        async Task<OperationResult> IUIAutomationOperation.ExecuteAsync(string parametersJson)
        {
            var typedResult = await ExecuteAsync(parametersJson);
            return new OperationResult
            {
                Success = typedResult.Success,
                Error = typedResult.Error,
                Data = typedResult.Data,
                ExecutionSeconds = typedResult.ExecutionSeconds
            };
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