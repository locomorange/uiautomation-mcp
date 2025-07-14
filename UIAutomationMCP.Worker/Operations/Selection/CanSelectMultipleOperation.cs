using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class CanSelectMultipleOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public CanSelectMultipleOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<BooleanResult>> ExecuteAsync(WorkerRequest request)
        {
            // Try to get typed request first, fallback to legacy dictionary method
            var typedRequest = request.GetTypedRequest<CanSelectMultipleRequest>(_options);
            
            string containerId;
            string windowTitle;
            int processId;
            
            if (typedRequest != null)
            {
                containerId = typedRequest.ElementId;
                windowTitle = typedRequest.WindowTitle;
                processId = typedRequest.ProcessId ?? 0;
            }
            else
            {
                // Legacy dictionary method
                containerId = request.Parameters?.GetValueOrDefault("containerId")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            }

            var element = _elementFinderService.FindElementById(containerId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<BooleanResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Element not found, cannot determine multiple selection capability"
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
                bool canSelectMultiple = selectionPattern.Current.CanSelectMultiple;
                
                var result = new BooleanResult
                {
                    Value = canSelectMultiple,
                    Description = canSelectMultiple ? "Container supports multiple selection" : "Container supports only single selection"
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
                    Error = $"Failed to get multiple selection capability: {ex.Message}",
                    Data = new BooleanResult
                    {
                        Value = false,
                        Description = "Failed to determine multiple selection capability due to error"
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