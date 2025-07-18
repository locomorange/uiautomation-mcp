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
    public class GetWindowInteractionStateOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetWindowInteractionStateOperation> _logger;

        public GetWindowInteractionStateOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetWindowInteractionStateOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetWindowInteractionStateRequest>(parametersJson)!;
                
                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;

                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Window not found",
                        Data = new WindowInteractionStateResult
                        {
                            InteractionState = "Unknown",
                            Description = "Window not found"
                        }
                    });
                }

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "WindowPattern not supported",
                        Data = new WindowInteractionStateResult
                        {
                            InteractionState = "Unknown",
                            Description = "WindowPattern not supported"
                        }
                    });
                }

                var interactionState = windowPattern.Current.WindowInteractionState;
                var result = new WindowInteractionStateResult
                {
                    InteractionState = interactionState.ToString(),
                    InteractionStateValue = (int)interactionState,
                    Description = GetInteractionStateDescription(interactionState)
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWindowInteractionState operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get window interaction state: {ex.Message}",
                    Data = new WindowInteractionStateResult
                    {
                        InteractionState = "Error",
                        Description = $"Error getting window interaction state: {ex.Message}"
                    }
                });
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