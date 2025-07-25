using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Shared.ErrorHandling;
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
            var typedRequest = JsonSerializationHelper.Deserialize<ToggleElementRequest>(parametersJson)!;
            
            return Task.FromResult(ErrorHandlerRegistry.Handle(() =>
            {
                // Validate element ID
                var validationError = ErrorHandlerRegistry.ValidateElementId(typedRequest.AutomationId, "ToggleElement");
                if (validationError != null)
                {
                    throw new UIAutomationValidationException("ToggleElement", "Element ID is required");
                }
                
                // パターン変換（リクエストから取得、デフォルトはTogglePattern）
                var requiredPattern = AutomationPatternHelper.GetAutomationPattern(typedRequest.RequiredPattern) ?? TogglePattern.Pattern;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId,
                    requiredPattern: requiredPattern);
                
                if (element == null)
                {
                    throw new UIAutomationElementNotFoundException("ToggleElement", typedRequest.AutomationId);
                }

                if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
                {
                    throw new UIAutomationInvalidOperationException("ToggleElement", typedRequest.AutomationId, "TogglePattern not supported");
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
                
                return new OperationResult 
                { 
                    Success = true, 
                    Data = result
                };
            }, "ToggleElement", typedRequest.AutomationId, 
                logAction: (exc, op, elemId, excType) => _logger.LogError(exc, "{Operation} operation failed for element: {ElementId}. Exception: {ExceptionType}", op, elemId, excType)));
        }
    }
}