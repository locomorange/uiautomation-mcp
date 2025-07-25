using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.ControlTypeInfo
{
    public class ValidateControlTypePatternsOperation : BaseUIAutomationOperation<ValidateControlTypePatternsRequest, BooleanResult>
    {
        public ValidateControlTypePatternsOperation(
            ElementFinderService elementFinderService,
            ILogger<ValidateControlTypePatternsOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(ValidateControlTypePatternsRequest request)
        {
            if (string.IsNullOrEmpty(request.AutomationId) && string.IsNullOrEmpty(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<BooleanResult> ExecuteOperationAsync(ValidateControlTypePatternsRequest request)
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

            var controlType = element.Current.ControlType;
            var availablePatterns = element.GetSupportedPatterns()
                .Select(pattern => pattern.ProgrammaticName)
                .ToArray();

            var expectedPatterns = UIAutomationMCP.Common.Helpers.ControlTypeHelper.GetPatternInfo(controlType);
            if (expectedPatterns != null)
            {
                var missingRequired = expectedPatterns.RequiredPatterns
                    .Where(p => !availablePatterns.Any(ap => ap.Contains(p)))
                    .ToArray();

                var presentOptional = expectedPatterns.OptionalPatterns
                    .Where(p => availablePatterns.Any(ap => ap.Contains(p)))
                    .ToArray();

                var unexpectedPatterns = availablePatterns
                    .Where(ap => !expectedPatterns.RequiredPatterns.Concat(expectedPatterns.OptionalPatterns)
                        .Any(ep => ap.Contains(ep)))
                    .ToArray();

                var isValid = missingRequired.Length == 0;

                var result = new BooleanResult
                {
                    Value = isValid,
                    Description = isValid ? "All required patterns are supported" : $"Missing {missingRequired.Length} required pattern(s)"
                };

                return Task.FromResult(result);
            }
            else
            {
                var result = new BooleanResult
                {
                    Value = true,
                    Description = $"No specific pattern requirements defined for control type: {controlType.LocalizedControlType}"
                };

                return Task.FromResult(result);
            }
        }

    }
}