using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SetTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetTextOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<SetValueResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var text = request.Parameters?.GetValueOrDefault("text")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new SetValueResult { ActionName = "SetText" }
                });

            // Primary method: Use ValuePattern for text input controls
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                if (!vp.Current.IsReadOnly)
                {
                    var previousValue = vp.Current.Value ?? "";
                    vp.SetValue(text);
                    
                    var result = new SetValueResult
                    {
                        ActionName = "SetText",
                        Completed = true,
                        ExecutedAt = DateTime.UtcNow,
                        PreviousState = previousValue,
                        CurrentState = text,
                        AttemptedValue = text
                    };

                    return Task.FromResult(new OperationResult<SetValueResult> 
                    { 
                        Success = true, 
                        Data = result 
                    });
                }
                else
                {
                    return Task.FromResult(new OperationResult<SetValueResult> 
                    { 
                        Success = false, 
                        Error = "Element is read-only",
                        Data = new SetValueResult 
                        { 
                            ActionName = "SetText",
                            Completed = false,
                            AttemptedValue = text
                        }
                    });
                }
            }
            
            return Task.FromResult(new OperationResult<SetValueResult> 
            { 
                Success = false, 
                Error = "Element does not support text modification",
                Data = new SetValueResult 
                { 
                    ActionName = "SetText",
                    Completed = false,
                    AttemptedValue = text
                }
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
