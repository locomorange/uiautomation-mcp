using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class GetWindowCapabilitiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetWindowCapabilitiesOperation> _logger;

        public GetWindowCapabilitiesOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetWindowCapabilitiesOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetWindowCapabilitiesRequest>(parametersJson)!;
                
                var windowTitle = typedRequest.WindowTitle ?? "";
                var processId = typedRequest.ProcessId ?? 0;

                var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
                if (window == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Window not found",
                        Data = new WindowCapabilitiesResult()
                    });
                }

                if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "WindowPattern not supported",
                        Data = new WindowCapabilitiesResult()
                    });
                }

                var result = new WindowCapabilitiesResult
                {
                    CanMaximize = windowPattern.Current.CanMaximize,
                    CanMinimize = windowPattern.Current.CanMinimize,
                    IsModal = windowPattern.Current.IsModal,
                    IsTopmost = windowPattern.Current.IsTopmost,
                    WindowVisualState = windowPattern.Current.WindowVisualState.ToString(),
                    WindowInteractionState = windowPattern.Current.WindowInteractionState.ToString()
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWindowCapabilities operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to get window capabilities: {ex.Message}",
                    Data = new WindowCapabilitiesResult()
                });
            }
        }
    }
}