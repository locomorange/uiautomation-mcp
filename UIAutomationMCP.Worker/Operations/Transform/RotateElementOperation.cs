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

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class RotateElementOperation : BaseUIAutomationOperation<RotateElementRequest, TransformActionResult>
    {
        public RotateElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<RotateElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<TransformActionResult> ExecuteOperationAsync(RotateElementRequest request)
        {
            // Use TransformPattern as the default required pattern
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? TransformPattern.Pattern;
            
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name,
                controlType: request.ControlType,
                windowTitle: request.WindowTitle,
                processId: request.ProcessId ?? 0,
                requiredPattern: requiredPattern);
                
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("RotateElement", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
            {
                throw new UIAutomationInvalidOperationException("RotateElement", request.AutomationId, "TransformPattern not supported");
            }

            if (!transformPattern.Current.CanRotate)
            {
                throw new UIAutomationInvalidOperationException("RotateElement", request.AutomationId, "Element cannot be rotated (CanRotate = false)");
            }

            var degrees = request.Degrees;

            transformPattern.Rotate(degrees);
            
            // Wait a moment for the transformation to complete
            await Task.Delay(50);
            
            // Get current bounds (rotation may affect positioning)
            var currentRect = element.Current.BoundingRectangle;
            var newBounds = new BoundingRectangle
            {
                X = currentRect.X,
                Y = currentRect.Y,
                Width = currentRect.Width,
                Height = currentRect.Height
            };

            return new TransformActionResult
            {
                ActionName = "Rotate",
                TransformType = "Rotate",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                NewBounds = newBounds.ToString(),
                RotationAngle = degrees,
                Details = $"Rotated element by {degrees} degrees"
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(RotateElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            // No specific validation for rotation degrees as any value can be valid
            return Core.Validation.ValidationResult.Success;
        }
    }
}