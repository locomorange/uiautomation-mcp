using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowCapabilitiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetWindowCapabilitiesOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<WindowCapabilitiesResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetWindowCapabilitiesRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<WindowCapabilitiesResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new WindowCapabilitiesResult()
                });
            
            var windowTitle = typedRequest.WindowTitle ?? "";
            var processId = typedRequest.ProcessId ?? 0;

            try
            {
                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                    return Task.FromResult(new OperationResult<WindowCapabilitiesResult> 
                    { 
                        Success = false, 
                        Error = "Window not found",
                        Data = new WindowCapabilitiesResult()
                    });

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                    return Task.FromResult(new OperationResult<WindowCapabilitiesResult> 
                    { 
                        Success = false, 
                        Error = "WindowPattern not supported",
                        Data = new WindowCapabilitiesResult()
                    });

                var result = new WindowCapabilitiesResult
                {
                    CanMaximize = windowPattern.Current.CanMaximize,
                    CanMinimize = windowPattern.Current.CanMinimize,
                    IsModal = windowPattern.Current.IsModal,
                    IsTopmost = windowPattern.Current.IsTopmost,
                    WindowVisualState = windowPattern.Current.WindowVisualState.ToString(),
                    WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString()
                };

                return Task.FromResult(new OperationResult<WindowCapabilitiesResult> 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<WindowCapabilitiesResult> 
                { 
                    Success = false, 
                    Error = $"Error getting window capabilities: {ex.Message}",
                    Data = new WindowCapabilitiesResult()
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