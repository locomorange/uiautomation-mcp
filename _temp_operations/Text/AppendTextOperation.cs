using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class AppendTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public AppendTextOperation(ElementFinderService elementFinderService)
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
            {
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new SetValueResult 
                    { 
                        ActionName = "AppendText", 
                        Completed = false, 
                        ExecutedAt = DateTime.UtcNow,
                        AttemptedValue = text
                    }
                });
            }

            // Try ValuePattern first
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                if (!vp.Current.IsReadOnly)
                {
                    try
                    {
                        var currentValue = vp.Current.Value ?? "";
                        var newValue = currentValue + text;
                        vp.SetValue(newValue);
                        
                        // Verify the value was set correctly
                        var verifiedValue = vp.Current.Value ?? "";
                        
                        return Task.FromResult(new OperationResult<SetValueResult> 
                        { 
                            Success = true, 
                            Data = new SetValueResult 
                            { 
                                ActionName = "AppendText", 
                                Completed = true, 
                                ExecutedAt = DateTime.UtcNow,
                                PreviousState = currentValue,
                                CurrentState = verifiedValue,
                                AttemptedValue = newValue
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        return Task.FromResult(new OperationResult<SetValueResult> 
                        { 
                            Success = false, 
                            Error = $"Failed to append text: {ex.Message}",
                            Data = new SetValueResult 
                            { 
                                ActionName = "AppendText", 
                                Completed = false, 
                                ExecutedAt = DateTime.UtcNow,
                                AttemptedValue = text
                            }
                        });
                    }
                }
                else
                {
                    return Task.FromResult(new OperationResult<SetValueResult> 
                    { 
                        Success = false, 
                        Error = "Element is read-only",
                        Data = new SetValueResult 
                        { 
                            ActionName = "AppendText", 
                            Completed = false, 
                            ExecutedAt = DateTime.UtcNow,
                            AttemptedValue = text
                        }
                    });
                }
            }
            else
            {
                return Task.FromResult(new OperationResult<SetValueResult> 
                { 
                    Success = false, 
                    Error = "Element does not support text modification",
                    Data = new SetValueResult 
                    { 
                        ActionName = "AppendText", 
                        Completed = false, 
                        ExecutedAt = DateTime.UtcNow,
                        AttemptedValue = text
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
