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
    public class ResizeElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<ResizeElementOperation> _logger;

        public ResizeElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<ResizeElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<ResizeElementRequest>(parametersJson)!;
                
                var width = typedRequest.Width;
                var height = typedRequest.Height;

                if (width <= 0 || height <= 0)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Width and height must be greater than 0",
                        Data = new TransformActionResult { ActionName = "Resize", TransformType = "Resize" }
                    });
                }

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
                        Data = new TransformActionResult { ActionName = "Resize", TransformType = "Resize" }
                    });
                }

                if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TransformPattern",
                        Data = new TransformActionResult { ActionName = "Resize", TransformType = "Resize" }
                    });
                }

                if (!transformPattern.Current.CanResize)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element cannot be resized (CanResize = false)",
                        Data = new TransformActionResult { ActionName = "Resize", TransformType = "Resize" }
                    });
                }

                transformPattern.Resize(width, height);
                
                // Get updated bounds after resize
                var currentRect = element.Current.BoundingRectangle;
                var newBounds = new BoundingRectangle
                {
                    X = currentRect.X,
                    Y = currentRect.Y,
                    Width = width,
                    Height = height
                };

                var result = new TransformActionResult
                {
                    ActionName = "Resize",
                    TransformType = "Resize",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    NewBounds = newBounds,
                    Details = $"Resized element to {width}x{height}"
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResizeElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to resize element: {ex.Message}",
                    Data = new TransformActionResult 
                    { 
                        ActionName = "Resize",
                        TransformType = "Resize",
                        Completed = false
                    }
                });
            }
        }
    }
}