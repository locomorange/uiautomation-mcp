using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Models.Serialization;
using UIAutomationMCP.Core.Exceptions;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Worker.Extensions;

namespace UIAutomationMCP.Worker.Operations.Window
{
    public class WindowActionOperation : BaseUIAutomationOperation<WindowActionRequest, WindowActionResult>
    {
        public WindowActionOperation(
            ElementFinderService elementFinderService, 
            ILogger<WindowActionOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override async Task<WindowActionResult> ExecuteOperationAsync(WindowActionRequest request)
        {
            var action = request.Action;
            var windowTitle = request.WindowTitle ?? "";
            var processId = request.ProcessId;

            var searchCriteria = new ElementSearchCriteria
            {
                WindowTitle = windowTitle,
                ProcessId = processId
            };
            var window = _elementFinderService.FindElement(searchCriteria);
            if (window == null)
            {
                throw new UIAutomationElementNotFoundException("Window", windowTitle);
            }

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
            {
                throw new UIAutomationInvalidOperationException("WindowAction", windowTitle, "WindowPattern not supported");
            }

            // Get the current state before action
            var previousState = windowPattern.Current.WindowVisualState.ToString();
            var windowHandle = window.Current.NativeWindowHandle;
            
            var result = new WindowActionResult
            {
                ActionName = action,
                WindowTitle = window.Current.Name,
                WindowHandle = windowHandle,
                PreviousState = previousState,
                Completed = true,
                ExecutedAt = DateTime.UtcNow
            };

            switch (action.ToLowerInvariant())
            {
                case "minimize":
                    windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                    result.CurrentState = "Minimized";
                    break;
                case "maximize":
                    windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                    result.CurrentState = "Maximized";
                    break;
                case "normal":
                case "restore":
                    windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                    result.CurrentState = "Normal";
                    break;
                case "close":
                    windowPattern.Close();
                    result.CurrentState = "Closed";
                    break;
                case "setfocus":
                    window.SetFocus();
                    result.CurrentState = windowPattern.Current.WindowVisualState.ToString();
                    break;
                default:
                    throw new UIAutomationInvalidOperationException("WindowAction", windowTitle, $"Unsupported window action: {action}");
            }

            return result;
        }

        protected override Core.Validation.ValidationResult ValidateRequest(WindowActionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Action))
            {
                return Core.Validation.ValidationResult.Failure("Action is required");
            }

            var validActions = new[] { "minimize", "maximize", "normal", "restore", "close", "setfocus" };
            if (!validActions.Contains(request.Action.ToLowerInvariant()))
            {
                return Core.Validation.ValidationResult.Failure($"Invalid action '{request.Action}'. Valid actions are: {string.Join(", ", validActions)}");
            }

            if (request.ProcessId.HasValue && request.ProcessId <= 0)
            {
                return Core.Validation.ValidationResult.Failure("ProcessId must be greater than 0 when specified");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}