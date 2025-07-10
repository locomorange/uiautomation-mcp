using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    public class WindowOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public WindowOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }

        public OperationResult WindowAction(string action, string windowTitle = "", int processId = 0)
        {
            var window = _elementFinderService.GetSearchRoot(windowTitle, processId);
            if (window == null)
                return new OperationResult { Success = false, Error = "Window not found" };

            if (!window.TryGetCurrentPattern(WindowPattern.Pattern, out var pattern) || pattern is not WindowPattern windowPattern)
                return new OperationResult { Success = false, Error = "WindowPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            switch (action.ToLowerInvariant())
            {
                case "minimize":
                    windowPattern.SetWindowVisualState(WindowVisualState.Minimized);
                    break;
                case "maximize":
                    windowPattern.SetWindowVisualState(WindowVisualState.Maximized);
                    break;
                case "normal":
                case "restore":
                    windowPattern.SetWindowVisualState(WindowVisualState.Normal);
                    break;
                case "close":
                    windowPattern.Close();
                    break;
                default:
                    return new OperationResult { Success = false, Error = $"Unsupported window action: {action}" };
            }

            return new OperationResult { Success = true, Data = $"Window action '{action}' performed successfully" };
        }

        public OperationResult TransformElement(string elementId, string action, double x = 0, double y = 0, double width = 0, double height = 0, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = "Element not found" };

            if (!element.TryGetCurrentPattern(TransformPattern.Pattern, out var pattern) || pattern is not TransformPattern transformPattern)
                return new OperationResult { Success = false, Error = "TransformPattern not supported" };

            // Let exceptions flow naturally - no try-catch
            switch (action.ToLowerInvariant())
            {
                case "move":
                    if (x == 0 && y == 0)
                        return new OperationResult { Success = false, Error = "Move action requires x and y coordinates" };
                    transformPattern.Move(x, y);
                    break;
                case "resize":
                    if (width == 0 && height == 0)
                        return new OperationResult { Success = false, Error = "Resize action requires width and height" };
                    transformPattern.Resize(width, height);
                    break;
                case "rotate":
                    transformPattern.Rotate(x); // Use x as rotation degrees
                    break;
                default:
                    return new OperationResult { Success = false, Error = $"Unsupported transform action: {action}" };
            }

            return new OperationResult { Success = true, Data = $"Element transformed with action '{action}' successfully" };
        }

    }
}
