using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class SetRangeValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public SetRangeValueOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var value = request.Parameters?.GetValueOrDefault("value")?.ToString() is string valueStr && 
                double.TryParse(valueStr, out var parsedValue) ? parsedValue : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support RangeValuePattern" });

            var currentValue = rangePattern.Current.Value;
            var minimum = rangePattern.Current.Minimum;
            var maximum = rangePattern.Current.Maximum;
            var isReadOnly = rangePattern.Current.IsReadOnly;

            if (isReadOnly)
                return Task.FromResult(new OperationResult { Success = false, Error = "Range element is read-only" });

            if (value < minimum || value > maximum)
            {
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Value {value} is out of range. Valid range: {minimum} - {maximum}" 
                });
            }

            rangePattern.SetValue(value);
            var newValue = rangePattern.Current.Value;
            
            return Task.FromResult(new OperationResult 
            { 
                Success = true, 
                Data = new 
                { 
                    PreviousValue = currentValue, 
                    NewValue = newValue,
                    Minimum = minimum,
                    Maximum = maximum
                }
            });
        }
    }
}
