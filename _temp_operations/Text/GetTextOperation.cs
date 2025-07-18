using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetTextOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetTextOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<TextInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetTextRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<TextInfoResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected GetTextRequest.",
                    Data = new TextInfoResult()
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<TextInfoResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new TextInfoResult()
                });

            var result = new TextInfoResult();

            // Try TextPattern first
            if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPattern) && textPattern is TextPattern tp)
            {
                var documentRange = tp.DocumentRange;
                result.Text = documentRange.GetText(-1);
            }
            // Try ValuePattern for text input controls
            else if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                result.Text = vp.Current.Value ?? "";
            }
            else
            {
                // Fallback to Name property
                result.Text = element.Current.Name ?? "";
            }

            return Task.FromResult(new OperationResult<TextInfoResult> 
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
