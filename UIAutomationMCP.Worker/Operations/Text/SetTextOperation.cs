using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class SetTextOperation : BaseUIAutomationOperation<SetTextRequest, SetValueResult>
    {
        public SetTextOperation(
            ElementFinderService elementFinderService,
            ILogger<SetTextOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(SetTextRequest request)
        {
            if (string.IsNullOrEmpty(request.AutomationId) && string.IsNullOrEmpty(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            if (request.Text == null)
            {
                return Core.Validation.ValidationResult.Failure("Text cannot be null");
            }

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<SetValueResult> ExecuteOperationAsync(SetTextRequest request)
        {
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                processId: request.ProcessId);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
            }

            // Primary method: Use ValuePattern for text input controls
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
            {
                if (!vp.Current.IsReadOnly)
                {
                    var previousValue = vp.Current.Value ?? "";
                    vp.SetValue(request.Text);
                    
                    var result = new SetValueResult
                    {
                        ActionName = "SetText",
                        Completed = true,
                        ExecutedAt = DateTime.UtcNow,
                        PreviousState = previousValue,
                        CurrentState = request.Text,
                        AttemptedValue = request.Text
                    };

                    return Task.FromResult(result);
                }
                else
                {
                    throw new UIAutomationElementNotFoundException("Operation", null, "Element is read-only");
                }
            }
            
            throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support text modification");
        }
    }
}