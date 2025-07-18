using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.VirtualizedItem
{
    public class RealizeVirtualizedItemOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public RealizeVirtualizedItemOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<RealizeVirtualizedItemRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format. Expected RealizeVirtualizedItemRequest.",
                    Data = new ActionResult { ActionName = "RealizeVirtualizedItem" }
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
                    Data = new ActionResult { ActionName = "RealizeVirtualizedItem" }
                });

            if (!element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out var pattern) || pattern is not VirtualizedItemPattern virtualizedItemPattern)
                return Task.FromResult(new OperationResult<ActionResult> 
                { 
                    Success = false, 
                    Error = "VirtualizedItemPattern not supported",
                    Data = new ActionResult { ActionName = "RealizeVirtualizedItem" }
                });

            try
            {
                var elementInfo = _elementFinderService.GetElementBasicInfo(element);
                
                virtualizedItemPattern.Realize();
                
                var result = new ActionResult
                {
                    ActionName = "RealizeVirtualizedItem",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    Details = $"Realized virtualized item: {elementInfo.Name} ({elementInfo.ControlType}, ID: {elementInfo.AutomationId})"
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
                    Error = $"Failed to realize virtualized item: {ex.Message}",
                    Data = new ActionResult 
                    { 
                        ActionName = "RealizeVirtualizedItem",
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