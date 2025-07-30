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
    public class ResizeElementOperation : BaseUIAutomationOperation<ResizeElementRequest, TransformActionResult>
    {
        public ResizeElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<ResizeElementOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<TransformActionResult> ExecuteOperationAsync(ResizeElementRequest request)
        {
            // Use TransformPattern as the default required pattern
            var requiredPattern = AutomationPatternHelper.GetAutomationPattern(request.RequiredPattern) ?? TransformPattern.Pattern;
            
            var searchCriteria = new ElementSearchCriteria
            {
                AutomationId = request.AutomationId,
                Name = request.Name,
                ControlType = request.ControlType,
                WindowTitle = request.WindowTitle,
                RequiredPattern = requiredPattern?.ProgrammaticName, WindowHandle = request.WindowHandle };
            var element = _elementFinderService.FindElement(searchCriteria);
                
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("ResizeElement", request.AutomationId);
            }

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
            {
                throw new UIAutomationInvalidOperationException("ResizeElement", request.AutomationId, "TransformPattern not supported");
            }

            if (!transformPattern.Current.CanResize)
            {
                throw new UIAutomationInvalidOperationException("ResizeElement", request.AutomationId, "Element cannot be resized (CanResize = false)");
            }

            var width = request.Width;
            var height = request.Height;

            transformPattern.Resize(width, height);
            
            // Wait a moment for the transformation to complete
            await Task.Delay(50);
            
            // Get updated bounds after resize
            var currentRect = element.Current.BoundingRectangle;
            var newBounds = new BoundingRectangle
            {
                X = currentRect.X,
                Y = currentRect.Y,
                Width = width,
                Height = height
            };

            return new TransformActionResult
            {
                ActionName = "Resize",
                TransformType = "Resize",
                Completed = true,
                ExecutedAt = DateTime.UtcNow,
                NewBounds = newBounds,
                Details = $"Resized element to {width}x{height}"
            };
        }

        protected override Core.Validation.ValidationResult ValidateRequest(ResizeElementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            if (request.Width <= 0)
            {
                return Core.Validation.ValidationResult.Failure("Width must be greater than 0");
            }

            if (request.Height <= 0)
            {
                return Core.Validation.ValidationResult.Failure("Height must be greater than 0");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}