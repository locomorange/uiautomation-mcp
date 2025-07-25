using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.ControlTypeInfo
{
    public class ValidateControlTypePatternsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ValidateControlTypePatternsOperation> _logger;


        public ValidateControlTypePatternsOperation(
            ElementFinderService elementFinderService,
            ILogger<ValidateControlTypePatternsOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ValidateControlTypePatternsRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new BooleanResult { Value = false, Description = "Element not found" }
                    });
                }

                var controlType = element.Current.ControlType;
                var availablePatterns = element.GetSupportedPatterns()
                    .Select(pattern => pattern.ProgrammaticName)
                    .ToArray();

                var expectedPatterns = ControlTypeHelper.GetPatternInfo(controlType);
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

                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true, 
                        Data = result 
                    });
                }
                else
                {
                    var result = new BooleanResult
                    {
                        Value = true,
                        Description = $"No specific pattern requirements defined for control type: {controlType.LocalizedControlType}"
                    };

                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true, 
                        Data = result 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateControlTypePatterns operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to validate control type patterns: {ex.Message}",
                    Data = new BooleanResult { Value = false, Description = "Operation failed" }
                });
            }
        }

    }
}