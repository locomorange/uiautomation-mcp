using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class GetToggleStateOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetToggleStateOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<ToggleStateResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ToggleStateResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ToggleStateResult()
                });

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return Task.FromResult(new OperationResult<ToggleStateResult> 
                { 
                    Success = false, 
                    Error = "TogglePattern not supported",
                    Data = new ToggleStateResult()
                });

            var state = togglePattern.Current.ToggleState;
            var result = new ToggleStateResult
            {
                State = state.ToString()
            };
            
            return Task.FromResult(new OperationResult<ToggleStateResult> 
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