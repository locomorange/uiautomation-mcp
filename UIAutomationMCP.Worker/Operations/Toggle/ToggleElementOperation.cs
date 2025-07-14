using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class ToggleElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public ToggleElementOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ToggleActionResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<ToggleElementRequest>(_options);
            if (typedRequest == null)
            {
                return Task.FromResult(new OperationResult<ToggleActionResult>
                {
                    Success = false,
                    Error = "Invalid request format. Expected ToggleElementRequest.",
                    Data = new ToggleActionResult { ActionName = "Toggle" }
                });
            }
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new ToggleActionResult { ActionName = "Toggle" }
                });

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = "TogglePattern not supported",
                    Data = new ToggleActionResult { ActionName = "Toggle" }
                });

            try
            {
                var previousState = togglePattern.Current.ToggleState.ToString();
                togglePattern.Toggle();
                
                // Wait a moment for the state to update
                System.Threading.Thread.Sleep(50);
                
                var currentState = togglePattern.Current.ToggleState.ToString();
                
                var result = new ToggleActionResult
                {
                    ActionName = "Toggle",
                    PreviousState = previousState,
                    CurrentState = currentState,
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow
                };
                
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<ToggleActionResult> 
                { 
                    Success = false, 
                    Error = $"Failed to toggle element: {ex.Message}",
                    Data = new ToggleActionResult 
                    { 
                        ActionName = "Toggle",
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