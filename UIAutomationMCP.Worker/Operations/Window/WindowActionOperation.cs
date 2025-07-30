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

        protected override Task<WindowActionResult> ExecuteOperationAsync(WindowActionRequest request)
        {
            var action = request.Action;
            var windowTitle = request.WindowTitle ?? "";

            var searchCriteria = new ElementSearchCriteria
            {
                WindowTitle = windowTitle,
                WindowHandle = request.WindowHandle,
                UseWindowHandleAsFilter = true,
                RequiredPattern = "Window"
            };
            _logger.LogDebug("WindowAction: Using filter mode with WindowHandle={WindowHandle}, UseWindowHandleAsFilter={UseFilter}", 
                request.WindowHandle, searchCriteria.UseWindowHandleAsFilter);
            Console.Error.WriteLine($"*** WORKER DEBUG *** WindowAction with WindowHandle={request.WindowHandle}, UseWindowHandleAsFilter={searchCriteria.UseWindowHandleAsFilter}");
            var window = _elementFinderService.FindElement(searchCriteria);
            if (window == null)
            {
                var elementIdentifier = request.WindowHandle.HasValue 
                    ? $"WindowHandle={request.WindowHandle.Value}" 
                    : !string.IsNullOrEmpty(windowTitle) ? $"WindowTitle='{windowTitle}'" : "unknown";
                throw new UIAutomationElementNotFoundException("WindowAction", elementIdentifier);
            }

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
            {
                var elementIdentifier = request.WindowHandle.HasValue 
                    ? $"WindowHandle={request.WindowHandle.Value}" 
                    : !string.IsNullOrEmpty(windowTitle) ? $"WindowTitle='{windowTitle}'" : "unknown";
                throw new UIAutomationInvalidOperationException("WindowAction", elementIdentifier, "WindowPattern not supported");
            }

            // Get the current state before action
            var previousState = windowPattern.Current.WindowVisualState.ToString();
            var windowHandle = (long)window.Current.NativeWindowHandle;
            
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

            return Task.FromResult(result);
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

            if (request.WindowHandle.HasValue && request.WindowHandle <= 0)
            {
                return Core.Validation.ValidationResult.Failure("WindowHandle must be greater than 0 when specified");
            }

            return Core.Validation.ValidationResult.Success;
        }
    }
}