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
    public class GetElementValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetElementValueOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementValueResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetElementValueRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ElementValueResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected GetElementValueRequest.",
                    Data = new ElementValueResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ElementValueResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ElementValueResult()
                });

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
                return Task.FromResult(new OperationResult<ElementValueResult> 
                { 
                    Success = false, 
                    Error = "ValuePattern not supported",
                    Data = new ElementValueResult()
                });

            var result = new ElementValueResult
            {
                Value = valuePattern.Current.Value ?? "",
                IsReadOnly = valuePattern.Current.IsReadOnly
            };
            
            return Task.FromResult(new OperationResult<ElementValueResult> 
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