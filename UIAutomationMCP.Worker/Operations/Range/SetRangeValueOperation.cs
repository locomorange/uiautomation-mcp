using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class SetRangeValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public SetRangeValueOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<SetRangeValueResult>> ExecuteAsync(WorkerRequest request)
        {
            // 型安全なリクエストを試行し、失敗した場合は従来の方法にフォールバック
            var typedRequest = request.GetTypedRequest<SetRangeValueRequest>(_options);
            
            string elementId, windowTitle;
            int processId;
            double value;
            
            if (typedRequest != null)
            {
                // 型安全なパラメータアクセス
                elementId = typedRequest.ElementId;
                windowTitle = typedRequest.WindowTitle;
                processId = typedRequest.ProcessId ?? 0;
                value = typedRequest.Value;
            }
            else
            {
                // 従来の方法（後方互換性のため）
                elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                value = request.Parameters?.GetValueOrDefault("value")?.ToString() is string valueStr && 
                    double.TryParse(valueStr, out var parsedValue) ? parsedValue : 0;
            }

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SetRangeValueResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new SetRangeValueResult { ActionName = "SetRangeValue" }
                });

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return Task.FromResult(new OperationResult<SetRangeValueResult> 
                { 
                    Success = false, 
                    Error = "Element does not support RangeValuePattern",
                    Data = new SetRangeValueResult { ActionName = "SetRangeValue" }
                });

            var currentValue = rangePattern.Current.Value;
            var minimum = rangePattern.Current.Minimum;
            var maximum = rangePattern.Current.Maximum;
            var isReadOnly = rangePattern.Current.IsReadOnly;

            if (isReadOnly)
                return Task.FromResult(new OperationResult<SetRangeValueResult> 
                { 
                    Success = false, 
                    Error = "Range element is read-only",
                    Data = new SetRangeValueResult { ActionName = "SetRangeValue" }
                });

            var wasClampedToRange = false;
            var attemptedValue = value;

            if (value < minimum)
            {
                value = minimum;
                wasClampedToRange = true;
            }
            else if (value > maximum)
            {
                value = maximum;
                wasClampedToRange = true;
            }

            rangePattern.SetValue(value);
            var newValue = rangePattern.Current.Value;
            
            var result = new SetRangeValueResult
            {
                ActionName = "SetRangeValue",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                PreviousState = currentValue,
                CurrentState = newValue,
                Minimum = minimum,
                Maximum = maximum,
                AttemptedValue = attemptedValue,
                WasClampedToRange = wasClampedToRange
            };

            return Task.FromResult(new OperationResult<SetRangeValueResult> 
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
