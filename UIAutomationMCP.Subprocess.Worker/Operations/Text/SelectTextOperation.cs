using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Text
{
    public class SelectTextOperation : BaseUIAutomationOperation<SelectTextRequest, ActionResult>
    {
        public SelectTextOperation(
            ElementFinderService elementFinderService,
            ILogger<SelectTextOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(SelectTextRequest request)
        {
            if (string.IsNullOrEmpty(request.AutomationId) && string.IsNullOrEmpty(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            if (request.StartIndex < 0)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("StartIndex must be non-negative");
            }

            if (request.Length <= 0)
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Length must be greater than 0");
            }

            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }

        protected override Task<ActionResult> ExecuteOperationAsync(SelectTextRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                WindowHandle = request.WindowHandle
            };
            var element = _elementFinderService.FindElement(searchCriteria);

            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, $"Element with AutomationId '{request.AutomationId}' and Name '{request.Name}' not found");
            }

            if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support TextPattern");
            }

            var documentRange = textPattern.DocumentRange;
            var fullText = documentRange.GetText(-1);

            if (request.StartIndex >= fullText.Length)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Start index is out of range");
            }

            var length = request.Length;
            if (request.StartIndex + length > fullText.Length)
                length = fullText.Length - request.StartIndex;

            var textRange = documentRange.Clone();
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, request.StartIndex);
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, request.StartIndex);
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);
            textRange.Select();

            return Task.FromResult(new ActionResult
            {
                ActionName = "SelectText",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                Details = $"Selected text from index {request.StartIndex}, length {length}: '{fullText.Substring(request.StartIndex, length)}'"
            });
        }
    }
}

