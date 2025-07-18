using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class DockElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<DockElementOperation> _logger;

        public DockElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<DockElementOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var typedRequest = JsonSerializationHelper.Deserialize<DockElementRequest>(parametersJson)!;
                
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
                        Data = new StateChangeResult<string> 
                        { 
                            ActionName = "DockElement", 
                            Completed = false, 
                            ExecutedAt = DateTime.UtcNow 
                        }
                    });
                }

                if (!element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) || pattern is not DockPattern dockPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support DockPattern",
                        Data = new StateChangeResult<string> 
                        { 
                            ActionName = "DockElement", 
                            Completed = false, 
                            ExecutedAt = DateTime.UtcNow 
                        }
                    });
                }

                var currentPosition = dockPattern.Current.DockPosition;
                var dockPosition = typedRequest.DockPosition ?? "fill";
                
                DockPosition newPosition = dockPosition.ToLowerInvariant() switch
                {
                    "top" => DockPosition.Top,
                    "bottom" => DockPosition.Bottom,
                    "left" => DockPosition.Left,
                    "right" => DockPosition.Right,
                    "fill" => DockPosition.Fill,
                    "none" => DockPosition.None,
                    _ => throw new ArgumentException($"Unsupported dock position: {dockPosition}")
                };

                dockPattern.SetDockPosition(newPosition);
                var updatedPosition = dockPattern.Current.DockPosition;
                
                var result = new StateChangeResult<string> 
                { 
                    ActionName = "DockElement", 
                    Completed = true, 
                    ExecutedAt = DateTime.UtcNow,
                    PreviousState = currentPosition.ToString(),
                    CurrentState = updatedPosition.ToString(),
                    Details = $"Docked element {typedRequest.ElementId} to {dockPosition}, actual position: {updatedPosition}"
                };

                return Task.FromResult(new OperationResult 
                { 
                    Success = true, 
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DockElement operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Failed to dock element: {ex.Message}",
                    Data = new StateChangeResult<string> 
                    { 
                        ActionName = "DockElement", 
                        Completed = false, 
                        ExecutedAt = DateTime.UtcNow
                    }
                });
            }
        }
    }
}