using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Subprocess.Core.Abstractions;
using System.Windows.Automation;

namespace UIAutomationMCP.Subprocess.Worker.Operations.Transform
{
    public class GetTransformCapabilitiesOperation : BaseUIAutomationOperation<GetTransformCapabilitiesRequest, TransformCapabilitiesResult>
    {
        public GetTransformCapabilitiesOperation(
            ElementFinderService elementFinderService,
            ILogger<GetTransformCapabilitiesOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Task<TransformCapabilitiesResult> ExecuteOperationAsync(GetTransformCapabilitiesRequest request)
        {
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                WindowTitle = request.WindowTitle,
            };

            var element = _elementFinderService.FindElement(searchCriteria);

            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("GetTransformCapabilities", request.AutomationId ?? request.Name ?? "Unknown");
            }

            bool canMove = false;
            bool canResize = false;
            bool canRotate = false;

            if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var patternObj) && patternObj is TransformPattern transformPattern)
            {
                canMove = transformPattern.Current.CanMove;
                canResize = transformPattern.Current.CanResize;
                canRotate = transformPattern.Current.CanRotate;
            }

            return Task.FromResult(new TransformCapabilitiesResult
            {
                CanMove = canMove,
                CanResize = canResize,
                CanRotate = canRotate,
                TransformType = "TransformPattern",
                Completed = true
            });
        }

        protected override UIAutomationMCP.Core.Validation.ValidationResult ValidateRequest(GetTransformCapabilitiesRequest request)
        {
             if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return UIAutomationMCP.Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }
            return UIAutomationMCP.Core.Validation.ValidationResult.Success;
        }
    }
}
