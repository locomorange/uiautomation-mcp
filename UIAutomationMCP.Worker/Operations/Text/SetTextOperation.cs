using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SetTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SetTextOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SetValueResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<SetTextRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<SetValueResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected SetTextRequest.",
                    Data = new SetValueResult { ActionName = "SetText" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;
            var text = typedRequest.Text;

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
