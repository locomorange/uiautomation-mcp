using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Helpers;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Toggle
{
    public class ToggleElementOperation : BaseUIAutomationOperation<ToggleElementRequest, ToggleActionResult>
    {
        public ToggleElementOperation(ElementFinderService elementFinderService, ILogger<ToggleElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<ToggleActionResult> ExecuteOperationAsync(ToggleElementRequest request)
        {
            // 繝代ち繝ｼ繝ｳ螟画鋤・ｽE・ｽ繝ｪ繧ｯ繧ｨ繧ｹ繝医°繧牙叙蠕励√ョ繝輔か繝ｫ繝茨ｿｽETogglePattern・ｽE・ｽE
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? TogglePattern.Pattern;

            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                RequiredPattern = requiredPattern?.ProgrammaticName,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);

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

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(ToggleElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}

