using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.Layout
{
    public class DockElementOperation : BaseUIAutomationOperation<DockElementRequest, StateChangeResult<string>>
    {
        public DockElementOperation(
            ElementFinderService elementFinderService, 
            ILogger<DockElementOperation> logger) : base(elementFinderService, logger)
        {
        }

        protected override async Task<StateChangeResult<string>> ExecuteOperationAsync(DockElementRequest request)
        {
            var element = _elementFinderService.FindElement(
                automationId: request.AutomationId, 
                name: request.Name, 
                controlType: request.ControlType, 
                windowTitle: request.WindowTitle, 
                processId: request.ProcessId ?? 0);
            
            if (element == null)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, $"Element with AutomationId '{request.AutomationId}' and Name '{request.Name}' not found");
            }

            if (!element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) || pattern is not DockPattern dockPattern)
            {
                throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support DockPattern");
            }

            var currentPosition = dockPattern.Current.DockPosition;
            var dockPosition = request.DockPosition ?? "fill";
            
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
                Details = $"Docked element (AutomationId: '{request.AutomationId}', Name: '{request.Name}') to {dockPosition}, actual position: {updatedPosition}"
            };

            return result;
        }
    }
}