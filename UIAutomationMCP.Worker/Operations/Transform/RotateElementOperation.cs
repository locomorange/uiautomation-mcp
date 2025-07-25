using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.UIAutomation.Abstractions;
using UIAutomationMCP.UIAutomation.Services;

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
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name, 
                    controlType: typedRequest.ControlType, 
                    windowTitle: typedRequest.WindowTitle, 
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Element with AutomationId '{typedRequest.AutomationId}' and Name '{typedRequest.Name}' not found",
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