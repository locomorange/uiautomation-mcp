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

namespace UIAutomationMCP.Worker.Operations.Range
{
    public class SetRangeValueOperation : BaseUIAutomationOperation<SetRangeValueRequest, SetRangeValueResult>
    {
        public SetRangeValueOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetRangeValueOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<SetRangeValueResult> ExecuteOperationAsync(SetRangeValueRequest request)
        {
            // パターン変換（リクエストから取得、デフォルトはRangeValuePattern）
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? RangeValuePattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name, 
                controlType: request.ControlType, 
                windowTitle: request.WindowTitle, 
                processId: request.ProcessId ?? 0,
                requiredPattern: requiredPattern);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("SetRangeValue", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) || pattern is not RangeValuePattern rangePattern)
            {
                throw new UIAutomationInvalidOperationException("SetRangeValue", request.AutomationId, "RangeValuePattern not supported");
            }

            var currentValue = rangePattern.Current.Value;
            var minimum = rangePattern.Current.Minimum;
            var maximum = rangePattern.Current.Maximum;
            var isReadOnly = rangePattern.Current.IsReadOnly;
            var value = request.Value;

            if (isReadOnly)
            {
                throw new UIAutomationInvalidOperationException("SetRangeValue", request.AutomationId, "Range element is read-only");
            }

            var wasClampedToRange = false;
            var attemptedValue = value;

            if (value < minimum)
            {
                value = minimum;
                wasClampedToRange = true;
            }
            else if (value > maximum)
            {
                value = maximum;
                wasClampedToRange = true;
            }

            rangePattern.SetValue(value);
            var newValue = rangePattern.Current.Value;
            
            return new SetRangeValueResult
            {
                ActionName = "SetRangeValue",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                PreviousState = currentValue.ToString(),
                CurrentState = newValue.ToString(),
                Minimum = minimum,
                Maximum = maximum,
                AttemptedValue = attemptedValue,
                WasClampedToRange = wasClampedToRange
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(SetRangeValueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}