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

        protected override Task<SetRangeValueResult> ExecuteOperationAsync(SetRangeValueRequest request)
        {
            // 繝代ち繝ｼ繝ｳ螟画鋤・医Μ繧ｯ繧ｨ繧ｹ繝医°繧牙叙蠕励√ョ繝輔か繝ｫ繝医・RangeValuePattern・・            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? RangeValuePattern.Pattern;
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                ProcessId = request.ProcessId,
                RequiredPattern = requiredPattern?.ProgrammaticName,
            }                WindowHandle = request.WindowHandle
            }
            var element = _elementFinderService.FindElement(searchCriteria);
            
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
            
            return Task.FromResult(new SetRangeValueResult
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
            });
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