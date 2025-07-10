using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class DockElementOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public DockElementOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var dockPosition = request.Parameters?.GetValueOrDefault("dockPosition")?.ToString() ?? "";

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            if (!element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) || pattern is not DockPattern dockPattern)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element does not support DockPattern" });

            var currentPosition = dockPattern.Current.DockPosition;
            
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
            
            return Task.FromResult(new OperationResult 
            { 
                Success = true, 
                Data = new { PreviousPosition = currentPosition.ToString(), NewPosition = updatedPosition.ToString() }
            });
        }
    }
}
