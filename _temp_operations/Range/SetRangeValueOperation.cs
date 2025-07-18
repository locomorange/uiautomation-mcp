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
            var typedRequest = request.GetTypedRequest<SetRangeValueRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<SetRangeValueResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new SetRangeValueResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;
            var value = typedRequest.Value;

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
