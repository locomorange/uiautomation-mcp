using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetSiblingsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetSiblingsOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Element not found" });

            var parent = TreeWalker.ControlViewWalker.GetParent(element);
            if (parent == null)
                return Task.FromResult(new OperationResult { Success = false, Error = "Parent element not found" });

            var siblings = new List<ElementInfo>();
            var child = TreeWalker.ControlViewWalker.GetFirstChild(parent);
            
            while (child != null)
            {
                if (!Automation.Compare(child, element)) // Exclude the element itself
                {
                    siblings.Add(new ElementInfo
                    {
                        AutomationId = child.Current.AutomationId,
                        Name = child.Current.Name,
                        ControlType = child.Current.ControlType.LocalizedControlType,
                        IsEnabled = child.Current.IsEnabled,
                        ProcessId = child.Current.ProcessId,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = child.Current.BoundingRectangle.X,
                            Y = child.Current.BoundingRectangle.Y,
                            Width = child.Current.BoundingRectangle.Width,
                            Height = child.Current.BoundingRectangle.Height
                        }
                    });
                }
                child = TreeWalker.ControlViewWalker.GetNextSibling(child);
            }

            return Task.FromResult(new OperationResult { Success = true, Data = siblings });
        }
    }
}