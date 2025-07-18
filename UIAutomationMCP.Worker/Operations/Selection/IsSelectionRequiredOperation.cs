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
    public class IsSelectionRequiredOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<IsSelectionRequiredOperation> _logger;

        public IsSelectionRequiredOperation(
            ElementFinderService elementFinderService, 
            ILogger<IsSelectionRequiredOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<IsSelectionRequiredRequest>(parametersJson)!;
                
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
                            Description = "Element not found, cannot determine selection requirement"
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

                var isSelectionRequired = selectionPattern.Current.IsSelectionRequired;
                
                var result = new BooleanResult
                {
                    Value = isSelectionRequired,
                    Description = isSelectionRequired ? "Selection is required for this container" : "Selection is not required for this container"
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsSelectionRequired operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get selection requirement: {ex.Message}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Failed to determine selection requirement due to error"
                    }
                });
            }
        }
    }
}