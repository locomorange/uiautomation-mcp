using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
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
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                ProcessId = request.ProcessId
            };
            var element = _elementFinderService.FindElement(searchCriteria);
            
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