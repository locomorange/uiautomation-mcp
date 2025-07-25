using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Common.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class SetToggleStateOperation : BaseUIAutomationOperation<SetToggleStateRequest, ToggleActionResult>
    {
        public SetToggleStateOperation(ElementFinderService elementFinderService, ILogger<SetToggleStateOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<ToggleActionResult> ExecuteOperationAsync(SetToggleStateRequest request)
        {
            // Use TogglePattern as the default required pattern
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? TogglePattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                windowTitle: request.WindowTitle,
                processId: request.ProcessId ?? 0,
                requiredPattern: requiredPattern);
                
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SetToggleState", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
            {
                throw new UIAutomationInvalidOperationException("SetToggleState", request.AutomationId, "TogglePattern not supported");
            }

            if (!Enum.TryParse<ToggleState>(request.State, true, out var targetState))
            {
                throw new UIAutomationInvalidOperationException("SetToggleState", request.AutomationId, $"Invalid toggle state: {request.State}. Valid values: On, Off, Indeterminate");
            }

            var initialState = togglePattern.Current.ToggleState;
            var currentState = initialState;
            
            while (currentState != targetState)
            {
                togglePattern.Toggle();
                
                // Wait a moment for the state to update
                await Task.Delay(50);
                
                var newState = togglePattern.Current.ToggleState;
                
                if (newState == currentState)
                {
                    throw new UIAutomationInvalidOperationException("SetToggleState", request.AutomationId, $"Element does not support toggle state: {request.State}");
                }
                
                currentState = newState;
            }

            return new ToggleActionResult
            {
                ActionName = "SetToggleState",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                PreviousState = initialState.ToString(),
                CurrentState = currentState.ToString(),
                Details = $"Successfully toggled element (AutomationId: '{request.AutomationId}', Name: '{request.Name}') to {targetState} (was {initialState})"
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(SetToggleStateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            if (string.IsNullOrWhiteSpace(request.State))
            {
                return Core.Validation.ValidationResult.Failure("State is required");
            }

            if (!Enum.TryParse<ToggleState>(request.State, true, out _))
            {
                return Core.Validation.ValidationResult.Failure($"Invalid toggle state: {request.State}. Valid values: On, Off, Indeterminate");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}