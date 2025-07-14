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
            // 型安全なリクエストを試行し、失敗した場合は従来の方法にフォールバック
            var typedRequest = request.GetTypedRequest<SetTextRequest>(_options);
            
            string elementId, windowTitle, text;
            int processId;
            
            if (typedRequest != null)
            {
                // 型安全なパラメータアクセス
                elementId = typedRequest.ElementId;
                windowTitle = typedRequest.WindowTitle;
                processId = typedRequest.ProcessId ?? 0;
                text = typedRequest.Text;
            }
            else
            {
                // 従来の方法（後方互換性のため）
                elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                text = request.Parameters?.GetValueOrDefault("text")?.ToString() ?? "";
            }

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
