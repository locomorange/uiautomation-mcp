using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class IsSelectionRequiredOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public IsSelectionRequiredOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            var containerId = request.Parameters?.GetValueOrDefault("containerId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(containerId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Element not found, cannot determine selection requirement"
                    }
                });
            
            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var patternObject))
            {
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = $"Element does not support SelectionPattern: {containerId}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Element does not support SelectionPattern"
                    }
                });
            }

            try
            {
                var selectionPattern = (SelectionPattern)patternObject;
                bool isSelectionRequired = selectionPattern.Current.IsSelectionRequired;
                
                var result = new BooleanResult
                {
                    Value = isSelectionRequired,
                    Description = isSelectionRequired ? "Selection is required for this container" : "Selection is not required for this container"
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
                    Error = $"Failed to get selection requirement: {ex.Message}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Failed to determine selection requirement due to error"
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