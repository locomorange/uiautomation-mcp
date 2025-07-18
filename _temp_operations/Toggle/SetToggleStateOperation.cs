using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class SetToggleStateOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SetToggleStateOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ToggleActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<SetToggleStateRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new ToggleActionResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;
            var toggleState = typedRequest.State;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ToggleActionResult { ActionName = "SetToggleState" }
                });

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = "TogglePattern not supported",
                    Data = new ToggleActionResult { ActionName = "SetToggleState" }
                });

            if (!Enum.TryParse<ToggleState>(toggleState, true, out var targetState))
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = $"Invalid toggle state: {toggleState}. Valid values: On, Off, Indeterminate",
                    Data = new ToggleActionResult { ActionName = "SetToggleState" }
                });

            var initialState = togglePattern.Current.ToggleState;
            var currentState = initialState;
            
            while (currentState != targetState)
            {
                togglePattern.Toggle();
                var newState = togglePattern.Current.ToggleState;
                
                if (newState == currentState)
                {
                    return Task.FromResult(new OperationResult<ToggleActionResult> 
                    { 
                        Success = false, 
                        Error = $"Element does not support toggle state: {toggleState}",
                        Data = new ToggleActionResult 
                        { 
                            ActionName = "SetToggleState",
                            Completed = false,
                            PreviousState = initialState.ToString(),
                            CurrentState = currentState.ToString(),
                            Details = $"Failed to set toggle state to {targetState} for element {elementId}"
                        }
                    });
                }
                
                currentState = newState;
            }

            var result = new ToggleActionResult
            {
                ActionName = "SetToggleState",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                PreviousState = initialState.ToString(),
                CurrentState = currentState.ToString(),
                Details = $"Successfully toggled {elementId} to {targetState} (was {initialState})"
            };

            return Task.FromResult(new OperationResult<ToggleActionResult> 
            { 
                Success = true, 
                Data = result
            });
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