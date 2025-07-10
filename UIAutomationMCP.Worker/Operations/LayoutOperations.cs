using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    public class LayoutOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public LayoutOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }

        public OperationResult ExpandCollapseElement(string elementId, string action, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) || pattern is not ExpandCollapsePattern expandCollapsePattern)
                return new OperationResult { Success = false, Error = "Element does not support ExpandCollapsePattern" };

            // Let exceptions flow naturally - no try-catch
            var currentState = expandCollapsePattern.Current.ExpandCollapseState;
            
            switch (action.ToLowerInvariant())
            {
                case "expand":
                    expandCollapsePattern.Expand();
                    break;
                case "collapse":
                    expandCollapsePattern.Collapse();
                    break;
                case "toggle":
                    if (currentState == ExpandCollapseState.Expanded)
                        expandCollapsePattern.Collapse();
                    else
                        expandCollapsePattern.Expand();
                    break;
                default:
                    return new OperationResult { Success = false, Error = $"Unsupported expand/collapse action: {action}" };
            }

            var newState = expandCollapsePattern.Current.ExpandCollapseState;
            return new OperationResult 
            { 
                Success = true, 
                Data = new { PreviousState = currentState.ToString(), NewState = newState.ToString() }
            };
        }

        public OperationResult ScrollElement(string elementId, string direction, double amount = 1.0, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern) || pattern is not ScrollPattern scrollPattern)
                return new OperationResult { Success = false, Error = "Element does not support ScrollPattern" };

            // Let exceptions flow naturally - no try-catch
            switch (direction.ToLowerInvariant())
            {
                case "up":
                    scrollPattern.ScrollVertical(ScrollAmount.SmallDecrement);
                    break;
                case "down":
                    scrollPattern.ScrollVertical(ScrollAmount.SmallIncrement);
                    break;
                case "left":
                    scrollPattern.ScrollHorizontal(ScrollAmount.SmallDecrement);
                    break;
                case "right":
                    scrollPattern.ScrollHorizontal(ScrollAmount.SmallIncrement);
                    break;
                case "pageup":
                    scrollPattern.ScrollVertical(ScrollAmount.LargeDecrement);
                    break;
                case "pagedown":
                    scrollPattern.ScrollVertical(ScrollAmount.LargeIncrement);
                    break;
                case "pageleft":
                    scrollPattern.ScrollHorizontal(ScrollAmount.LargeDecrement);
                    break;
                case "pageright":
                    scrollPattern.ScrollHorizontal(ScrollAmount.LargeIncrement);
                    break;
                default:
                    return new OperationResult { Success = false, Error = $"Unsupported scroll direction: {direction}" };
            }

            return new OperationResult { Success = true, Data = $"Element scrolled {direction} successfully" };
        }

        public OperationResult ScrollElementIntoView(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var pattern) || pattern is not ScrollItemPattern scrollItemPattern)
                return new OperationResult { Success = false, Error = "Element does not support ScrollItemPattern" };

            // Let exceptions flow naturally - no try-catch
            scrollItemPattern.ScrollIntoView();
            return new OperationResult { Success = true, Data = "Element scrolled into view successfully" };
        }

        public OperationResult DockElement(string elementId, string dockPosition, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            if (!element.TryGetCurrentPattern(DockPattern.Pattern, out var pattern) || pattern is not DockPattern dockPattern)
                return new OperationResult { Success = false, Error = "Element does not support DockPattern" };

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

            // Let exceptions flow naturally - no try-catch
            dockPattern.SetDockPosition(newPosition);
            var updatedPosition = dockPattern.Current.DockPosition;
            
            return new OperationResult 
            { 
                Success = true, 
                Data = new { PreviousPosition = currentPosition.ToString(), NewPosition = updatedPosition.ToString() }
            };
        }

    }
}
