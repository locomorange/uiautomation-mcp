using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SetTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<SetTextOperation> _logger;

        public SetTextOperation(
            ElementFinderService elementFinderService,
            ILogger<SetTextOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<SetTextRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element '{typedRequest.ElementId}' not found",
                        Data = new SetValueResult { ActionName = "SetText" }
                    });
                }

                // Primary method: Use ValuePattern for text input controls
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                {
                    if (!vp.Current.IsReadOnly)
                    {
                        var previousValue = vp.Current.Value ?? "";
                        vp.SetValue(typedRequest.Text);
                        
                        var result = new SetValueResult
                        {
                            ActionName = "SetText",
                            Completed = true,
                            ExecutedAt = DateTime.UtcNow,
                            PreviousState = previousValue,
                            CurrentState = typedRequest.Text,
                            AttemptedValue = typedRequest.Text
                        };

                        return Task.FromResult(new OperationResult 
                        { 
                            Success = true, 
                            Data = result 
                        });
                    }
                    else
                    {
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = false, 
                            Error = "Element is read-only",
                            Data = new SetValueResult 
                            { 
                                ActionName = "SetText",
                                Completed = false,
                                AttemptedValue = typedRequest.Text
                            }
                        });
                    }
                }
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = "Element does not support text modification",
                    Data = new SetValueResult 
                    { 
                        ActionName = "SetText",
                        Completed = false,
                        AttemptedValue = typedRequest.Text
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetText operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to set text: {ex.Message}",
                    Data = new SetValueResult { ActionName = "SetText" }
                });
            }
        }
    }
}