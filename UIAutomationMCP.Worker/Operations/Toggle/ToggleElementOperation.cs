using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.UIAutomation.Helpers;

namespace UIAutomationMCP.Worker.Operations.Toggle
{
    public class ToggleElementOperation : BaseUIAutomationOperation<ToggleElementRequest, ToggleActionResult>
    {
        public ToggleElementOperation(ElementFinderService elementFinderService, ILogger<ToggleElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<ToggleActionResult> ExecuteOperationAsync(ToggleElementRequest request)
        {
            // パターン変換（リクエストから取得、デフォルトはTogglePattern）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? TogglePattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                processId: request.ProcessId,
                requiredPattern: requiredPattern);
                
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("ToggleElement", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) || pattern is not TogglePattern togglePattern)
            {
                throw new UIAutomationInvalidOperationException("ToggleElement", request.AutomationId, "TogglePattern not supported");
            }

            var previousState = togglePattern.Current.ToggleState.ToString();
            togglePattern.Toggle();
            
            // Wait a moment for the state to update
            await Task.Delay(50);
            
            var currentState = togglePattern.Current.ToggleState.ToString();
            
            return new ToggleActionResult
            {
                ActionName = "Toggle",
                PreviousState = previousState,
                CurrentState = currentState,
                Completed = true,
                ExecutedAt = DateTime.UtcNow
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(ToggleElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}