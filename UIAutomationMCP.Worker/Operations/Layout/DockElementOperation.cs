using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class DockElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public DockElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<StateChangeResult<string>>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var dockPosition = request.Parameters?.GetValueOrDefault("dockPosition")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
            {
                return Task.FromResult(new OperationResult<StateChangeResult<string>> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
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
                return Task.FromResult(new OperationResult<StateChangeResult<string>> 
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
            
            try
            {
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
                
                return Task.FromResult(new OperationResult<StateChangeResult<string>> 
                { 
                    Success = true, 
                    Data = new StateChangeResult<string> 
                    { 
                        ActionName = "DockElement", 
                        Completed = true, 
                        ExecutedAt = DateTime.UtcNow,
                        PreviousState = currentPosition.ToString(),
                        CurrentState = updatedPosition.ToString(),
                        Details = new Dictionary<string, object>
                        {
                            ["ElementId"] = elementId,
                            ["RequestedPosition"] = dockPosition,
                            ["ActualPosition"] = updatedPosition.ToString()
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<StateChangeResult<string>> 
                { 
                    Success = false, 
                    Error = $"Failed to dock element: {ex.Message}",
                    Data = new StateChangeResult<string> 
                    { 
                        ActionName = "DockElement", 
                        Completed = false, 
                        ExecutedAt = DateTime.UtcNow,
                        PreviousState = currentPosition.ToString()
                    }
                });
            }
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}
