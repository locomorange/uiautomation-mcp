using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class CanSelectMultipleOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<CanSelectMultipleOperation> _logger;

        public CanSelectMultipleOperation(
            ElementFinderService elementFinderService, 
            ILogger<CanSelectMultipleOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<CanSelectMultipleRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new BooleanResult
                        {
                            Value = false,
                            Description = "Element not found, cannot determine multiple selection capability"
                        }
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionPattern not supported",
                        Data = new BooleanResult
                        {
                            Value = false,
                            Description = "Element does not support SelectionPattern"
                        }
                    });
                }

                var canSelectMultiple = selectionPattern.Current.CanSelectMultiple;
                
                var result = new BooleanResult
                {
                    Value = canSelectMultiple,
                    Description = canSelectMultiple ? "Container supports multiple selection" : "Container supports only single selection"
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CanSelectMultiple operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get multiple selection capability: {ex.Message}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Failed to determine multiple selection capability due to error"
                    }
                });
            }
        }
    }
}