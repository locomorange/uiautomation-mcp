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
    public class IsSelectedOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<IsSelectedOperation> _logger;

        public IsSelectedOperation(
            ElementFinderService elementFinderService, 
            ILogger<IsSelectedOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<IsSelectedRequest>(parametersJson)!;
                
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
                        Error = "Element not found",
                        Data = new BooleanResult
                        {
                            Value = false,
                            Description = "Element not found, cannot determine selection state"
                        }
                    });
                }

                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) || pattern is not SelectionItemPattern selectionItemPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "SelectionItemPattern not supported",
                        Data = new BooleanResult
                        {
                            Value = false,
                            Description = "Element does not support SelectionItemPattern"
                        }
                    });
                }

                var isSelected = selectionItemPattern.Current.IsSelected;
                
                var result = new BooleanResult
                {
                    Value = isSelected,
                    Description = isSelected ? "Element is currently selected" : "Element is not selected"
                };
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsSelected operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get selection state: {ex.Message}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Failed to determine selection state due to error"
                    }
                });
            }
        }
    }
}