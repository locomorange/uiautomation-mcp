using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Value
{
    public class SetElementValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetElementValueOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<SetValueResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var value = request.Parameters?.GetValueOrDefault("value")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new SetValueResult { AttemptedValue = value }
                });

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = "ValuePattern not supported",
                    Data = new SetValueResult { AttemptedValue = value }
                });

            // Get the previous value before setting
            var previousValue = valuePattern.Current.Value ?? "";
            
            try
            {
                valuePattern.SetValue(value);
                
                // Get the current value after setting
                var currentValue = valuePattern.Current.Value ?? "";
                
                var result = new SetValueResult
                {
                    ActionName = "SetValue",
                    Completed = true,
                    PreviousState = previousValue,
                    CurrentState = currentValue,
                    AttemptedValue = value
                };
                
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = $"Failed to set value: {ex.Message}",
                    Data = new SetValueResult 
                    { 
                        ActionName = "SetValue",
                        Completed = false,
                        PreviousState = previousValue,
                        AttemptedValue = value 
                    }
                });
            }
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