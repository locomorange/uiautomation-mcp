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

namespace UIAutomationMCP.Subprocess.Worker.Operations.Value
{
    public class SetElementValueOperation : BaseUIAutomationOperation<SetElementValueRequest, SetValueResult>
    {
        public SetElementValueOperation(
            ElementFinderService elementFinderService, 
            ILogger<SetElementValueOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<SetValueResult> ExecuteOperationAsync(SetElementValueRequest request)
        {
            // 繝代ち繝ｼ繝ｳ螟画鋤・ｽE・ｽ繝ｪ繧ｯ繧ｨ繧ｹ繝医°繧牙叙蠕励√ョ繝輔か繝ｫ繝茨ｿｽEValuePattern・ｽE・ｽE
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? ValuePattern.Pattern;
            
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
                throw new UIAutomationElementNotFoundException("SetElementValue", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) || pattern is not ValuePattern valuePattern)
            {
                throw new UIAutomationInvalidOperationException("SetElementValue", request.AutomationId, "ValuePattern not supported");
            }

            var previousValue = valuePattern.Current.Value ?? "";
            valuePattern.SetValue(request.Value);
            var currentValue = valuePattern.Current.Value ?? "";
            
            return Task.FromResult(new SetValueResult
            {
                ActionName = "SetValue",
                Completed = true,
                PreviousState = previousValue,
                CurrentState = currentValue,
                AttemptedValue = request.Value
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(SetElementValueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Element ID is required");
            }

            if (request.Value == null)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Value is required");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}

