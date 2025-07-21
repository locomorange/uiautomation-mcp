using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class SetRangeValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SetRangeValueOperation> _logger;

        public SetRangeValueOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetRangeValueOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SetRangeValueRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element with AutomationId '{typedRequest.AutomationId}' and Name '{typedRequest.Name}' not found",
                        Data = new SetRangeValueResult { ActionName = "SetRangeValue" }
                    });
                }

                if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support RangeValuePattern",
                        Data = new SetRangeValueResult { ActionName = "SetRangeValue" }
                    });
                }

                var currentValue = rangePattern.Current.Value;
                var minimum = rangePattern.Current.Minimum;
                var maximum = rangePattern.Current.Maximum;
                var isReadOnly = rangePattern.Current.IsReadOnly;
                var value = typedRequest.Value;

                if (isReadOnly)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Range element is read-only",
                        Data = new SetRangeValueResult { ActionName = "SetRangeValue" }
                    });
                }

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

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetRangeValue operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to set range value: {ex.Message}",
                    Data = new SetRangeValueResult 
                    { 
                        ActionName = "SetRangeValue",
                        Completed = false
                    }
                });
            }
        }
    }
}