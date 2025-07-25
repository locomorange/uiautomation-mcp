using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;
using UIAutomationMCP.UIAutomation.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class MoveElementOperation : BaseUIAutomationOperation<MoveElementRequest, TransformActionResult>
    {
        public MoveElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<MoveElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<TransformActionResult> ExecuteOperationAsync(MoveElementRequest request)
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
                throw new UIAutomationElementNotFoundException("MoveElement", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
            {
                throw new UIAutomationInvalidOperationException("MoveElement", request.AutomationId, "TransformPattern not supported");
            }

            if (!transformPattern.Current.CanMove)
            {
                throw new UIAutomationInvalidOperationException("MoveElement", request.AutomationId, "Element cannot be moved (CanMove = false)");
            }

            var x = request.X;
            var y = request.Y;

            transformPattern.Move(x, y);
            
            // Wait a moment for the transformation to complete
            await Task.Delay(50);
            
            // Get updated bounds after move
            var newBounds = new BoundingRectangle
            {
                X = x,
                Y = y,
                Width = element.Current.BoundingRectangle.Width,
                Height = element.Current.BoundingRectangle.Height
            };

            return new TransformActionResult
            {
                ActionName = "Move",
                TransformType = "Move",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                NewBounds = newBounds.ToString(),
                Details = $"Moved element to position ({x}, {y})"
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(MoveElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            // No specific validation for coordinates as they can be negative or zero
            return Core.Validation.ValidationResult.Success;
        }
    }
}