using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class GetRangeValueOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetRangeValueOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<RangeValueResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetRangeValueRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<RangeValueResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected GetRangeValueRequest.",
                    Data = new RangeValueResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<RangeValueResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new RangeValueResult()
                });

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
                return Task.FromResult(new OperationResult<RangeValueResult> 
                { 
                    Success = false, 
                    Error = "Element does not support RangeValuePattern",
                    Data = new RangeValueResult()
                });

            var result = new RangeValueResult
            {
                Value = rangePattern.Current.Value,
                Minimum = rangePattern.Current.Minimum,
                Maximum = rangePattern.Current.Maximum,
                LargeChange = rangePattern.Current.LargeChange,
                SmallChange = rangePattern.Current.SmallChange,
                IsReadOnly = rangePattern.Current.IsReadOnly
            };

            return Task.FromResult(new OperationResult<RangeValueResult> 
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
