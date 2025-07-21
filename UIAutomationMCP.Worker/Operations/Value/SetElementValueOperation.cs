using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Value
{
    public class SetElementValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SetElementValueOperation> _logger;

        public SetElementValueOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetElementValueOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SetValueRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new SetValueResult { AttemptedValue = typedRequest.Value }
                    });
                }

                if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "ValuePattern not supported",
                        Data = new SetValueResult { AttemptedValue = typedRequest.Value }
                    });
                }

                var previousValue = valuePattern.Current.Value ?? "";
                
                try
                {
                    valuePattern.SetValue(typedRequest.Value);
                    
                    var currentValue = valuePattern.Current.Value ?? "";
                    
                    var result = new SetValueResult
                    {
                        ActionName = "SetValue",
                        Completed = true,
                        PreviousState = previousValue,
                        CurrentState = currentValue,
                        AttemptedValue = typedRequest.Value
                    };
                    
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true,
                        Data = result
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set element value");
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Failed to set value: {ex.Message}",
                        Data = new SetValueResult 
                        { 
                            ActionName = "SetValue",
                            Completed = false,
                            PreviousState = previousValue,
                            AttemptedValue = typedRequest.Value 
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetElementValue operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to execute operation: {ex.Message}",
                    Data = new SetValueResult { AttemptedValue = "" }
                });
            }
        }
    }
}