using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Value
{
    public class IsReadOnlyOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public IsReadOnlyOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<IsReadOnlyRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<BooleanResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected IsReadOnlyRequest.",
                    Data = new BooleanResult { Value = false, Description = "Invalid request format" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new BooleanResult { Value = false, Description = "Element not found" }
                });

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = "ValuePattern not supported",
                    Data = new BooleanResult { Value = false, Description = "ValuePattern not supported" }
                });

            var isReadOnly = valuePattern.Current.IsReadOnly;
            var result = new BooleanResult 
            { 
                Value = isReadOnly, 
                Description = isReadOnly ? "Element is read-only" : "Element is editable"
            };
            
            return Task.FromResult(new OperationResult<BooleanResult> 
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