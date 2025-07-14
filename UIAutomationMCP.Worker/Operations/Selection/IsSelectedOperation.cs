using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class IsSelectedOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public IsSelectedOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Element not found, cannot determine selection state"
                    }
                });
            
            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObject))
            {
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = $"Element does not support SelectionItemPattern: {elementId}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Element does not support SelectionItemPattern"
                    }
                });
            }

            try
            {
                var selectionItemPattern = (SelectionItemPattern)patternObject;
                bool isSelected = selectionItemPattern.Current.IsSelected;
                
                var result = new BooleanResult
                {
                    Value = isSelected,
                    Description = isSelected ? "Element is currently selected" : "Element is not selected"
                };
                
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<BooleanResult> 
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