using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.SynchronizedInput
{
    public class CancelSynchronizedInputOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public CancelSynchronizedInputOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<CancelSynchronizedInputRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected CancelSynchronizedInputRequest.",
                    Data = new ActionResult { ActionName = "CancelSynchronizedInput" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ActionResult { ActionName = "CancelSynchronizedInput" }
                });

            if (!element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var pattern) || pattern is not SynchronizedInputPattern synchronizedInputPattern)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "SynchronizedInputPattern not supported",
                    Data = new ActionResult { ActionName = "CancelSynchronizedInput" }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                synchronizedInputPattern.Cancel();
                
                var result = new ActionResult
                {
                    ActionName = "CancelSynchronizedInput",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Canceled synchronized input listening on {elementInfo.Name} ({elementInfo.ControlType})"
                };
                
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = $"Failed to cancel synchronized input: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "CancelSynchronizedInput",
                        Completed = false
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