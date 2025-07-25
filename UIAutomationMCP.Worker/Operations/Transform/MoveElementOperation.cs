using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Transform
{
    public class MoveElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<MoveElementOperation> _logger;

        public MoveElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<MoveElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<MoveElementRequest>(parametersJson)!;
                
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
                        Data = new TransformActionResult { ActionName = "Move", TransformType = "Move" }
                    });
                }

                if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TransformPattern",
                        Data = new TransformActionResult { ActionName = "Move", TransformType = "Move" }
                    });
                }

                if (!transformPattern.Current.CanMove)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element cannot be moved (CanMove = false)",
                        Data = new TransformActionResult { ActionName = "Move", TransformType = "Move" }
                    });
                }

                var x = typedRequest.X;
                var y = typedRequest.Y;

                transformPattern.Move(x, y);
                
                // Get updated bounds after move
                var newBounds = new BoundingRectangle
                {
                    X = x,
                    Y = y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                };

                var result = new TransformActionResult
                {
                    ActionName = "Move",
                    TransformType = "Move",
                    Completed = true,
                    ExecutedAt = DateTime.UtcNow,
                    NewBounds = newBounds,
                    Details = $"Moved element to position ({x}, {y})"
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MoveElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to move element: {ex.Message}",
                    Data = new TransformActionResult 
                    { 
                        ActionName = "Move",
                        TransformType = "Move",
                        Completed = false
                    }
                });
            }
        }
    }
}