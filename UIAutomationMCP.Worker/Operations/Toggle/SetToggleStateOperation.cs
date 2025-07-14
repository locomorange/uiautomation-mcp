using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class SetToggleStateOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetToggleStateOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ToggleActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var toggleState = request.Parameters?.GetValueOrDefault("toggleState")?.ToString() ?? "";

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
                            Details = new Dictionary<string, object>
                            {
                                ["TargetState"] = targetState.ToString(),
                                ["ElementId"] = elementId
                            }
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
                Details = new Dictionary<string, object>
                {
                    ["TargetState"] = targetState.ToString(),
                    ["ElementId"] = elementId,
                    ["TogglesRequired"] = currentState != initialState ? 1 : 0
                }
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