using System.Windows.Automation;
using UIAutomationMCP.Shared;
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

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var toggleState = request.Parameters?.GetValueOrDefault("toggleState")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "TogglePattern not supported" });

            if (!Enum.TryParse<ToggleState>(toggleState, true, out var targetState))
                return Task.FromResult(new OperationResult { Success = false, Error = $"Invalid toggle state: {toggleState}. Valid values: On, Off, Indeterminate" });

            var currentState = togglePattern.Current.ToggleState;
            
            while (currentState != targetState)
            {
                togglePattern.Toggle();
                var newState = togglePattern.Current.ToggleState;
                
                if (newState == currentState)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element does not support toggle state: {toggleState}" });
                }
                
                currentState = newState;
            }

            return Task.FromResult(new OperationResult 
            { 
                Success = true, 
                Data = new { TargetState = targetState.ToString(), FinalState = currentState.ToString() }
            });
        }
    }
}