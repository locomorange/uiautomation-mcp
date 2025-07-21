using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class ToggleElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ToggleElementOperation> _logger;

        public ToggleElementOperation(ElementFinderService elementFinderService, ILogger<ToggleElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ToggleElementRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new ToggleActionResult { ActionName = "Toggle" }
                    });
                }

                if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "TogglePattern not supported",
                        Data = new ToggleActionResult { ActionName = "Toggle" }
                    });
                }

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
                
                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleElement operation failed");
                return Task.FromResult(new OperationResult 
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
    }
}