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

namespace UIAutomationMCP.Worker.Operations.Value
{
    public class SetElementValueOperation : BaseUIAutomationOperation<SetValueRequest, SetValueResult>
    {
        public SetElementValueOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetElementValueOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<SetValueResult> ExecuteOperationAsync(SetValueRequest request)
        {
            // パターン変換（リクエストから取得、デフォルトはValuePattern）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? ValuePattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                processId: request.ProcessId,
                requiredPattern: requiredPattern);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SetElementValue", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
            {
                throw new UIAutomationInvalidOperationException("SetElementValue", request.AutomationId, "ValuePattern not supported");
            }

            var previousValue = valuePattern.Current.Value ?? "";
            valuePattern.SetValue(request.Value);
            var currentValue = valuePattern.Current.Value ?? "";
            
            return new SetValueResult
            {
                ActionName = "SetValue",
                Completed = true,
                PreviousState = previousValue,
                CurrentState = currentValue,
                AttemptedValue = request.Value
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(SetValueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            if (request.Value == null)
            {
                return Core.Validation.ValidationResult.Failure("Value is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}