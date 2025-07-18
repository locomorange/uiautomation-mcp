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
    public class GetTransformCapabilitiesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetTransformCapabilitiesOperation> _logger;

        public GetTransformCapabilitiesOperation(
            ElementFinderService elementFinderService, 
            ILogger<GetTransformCapabilitiesOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<GetTransformCapabilitiesRequest>(parametersJson)!;
                
                _logger.LogInformation($"Getting transform capabilities for element: {typedRequest.ElementId}");

                var element = _elementFinderService.FindElementById(
                    typedRequest.ElementId, 
                    typedRequest.WindowTitle, 
                    typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    _logger.LogWarning($"Element not found: {typedRequest.ElementId}");
                    return Task.FromResult(new OperationResult
                    {
                        Success = false,
                        Error = $"Element not found: {typedRequest.ElementId}"
                    });
                }

                var transformPattern = element.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                var boundingRect = element.Current.BoundingRectangle;

                var result = new TransformCapabilitiesResult
                {
                    Success = true,
                    ElementId = typedRequest.ElementId,
                    WindowTitle = typedRequest.WindowTitle,
                    ProcessId = typedRequest.ProcessId ?? 0,
                    CanMove = transformPattern?.Current.CanMove ?? false,
                    CanResize = transformPattern?.Current.CanResize ?? false,
                    CanRotate = transformPattern?.Current.CanRotate ?? false,
                    CurrentX = boundingRect.X,
                    CurrentY = boundingRect.Y,
                    CurrentWidth = boundingRect.Width,
                    CurrentHeight = boundingRect.Height,
                    CurrentRotation = 0.0, // Windows UI Automation doesn't provide rotation info directly
                    CurrentBounds = new UIAutomationMCP.Shared.BoundingRectangle
                    {
                        X = boundingRect.X,
                        Y = boundingRect.Y,
                        Width = boundingRect.Width,
                        Height = boundingRect.Height
                    }
                };

                _logger.LogInformation($"Transform capabilities retrieved for element: {typedRequest.ElementId}");
                return Task.FromResult(new OperationResult
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transform capabilities: {ex.Message}");
                return Task.FromResult(new OperationResult
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }
}