using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class RotateElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<RotateElementOperation> _logger;

        public RotateElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<RotateElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<RotateElementRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element '{typedRequest.ElementId}' not found",
                        Data = new TransformActionResult { ActionName = "Rotate", TransformType = "Rotate" }
                    });
                }

                if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TransformPattern",
                        Data = new TransformActionResult { ActionName = "Rotate", TransformType = "Rotate" }
                    });
                }

                if (!transformPattern.Current.CanRotate)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element cannot be rotated (CanRotate = false)",
                        Data = new TransformActionResult { ActionName = "Rotate", TransformType = "Rotate" }
                    });
                }

                var degrees = typedRequest.Degrees;

                transformPattern.Rotate(degrees);
                
                // Get current bounds (rotation may affect positioning)
                var currentRect = element.Current.BoundingRectangle;
                var newBounds = new BoundingRectangle
                {
                    X = currentRect.X,
                    Y = currentRect.Y,
                    Width = currentRect.Width,
                    Height = currentRect.Height
                };

                var result = new TransformActionResult
                {
                    ActionName = "Rotate",
                    TransformType = "Rotate",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    NewBounds = newBounds,
                    RotationAngle = degrees,
                    Details = $"Rotated element by {degrees} degrees"
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RotateElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to rotate element: {ex.Message}",
                    Data = new TransformActionResult 
                    { 
                        ActionName = "Rotate",
                        TransformType = "Rotate",
                        Completed = false
                    }
                });
            }
        }
    }
}