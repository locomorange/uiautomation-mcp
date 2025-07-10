using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetAncestorsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetAncestorsOperation(ElementFinderService elementFinderService)
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

            var ancestors = new List<ElementInfo>();
            var current = TreeWalker.ControlViewWalker.GetParent(element);

            while (current != null && !Automation.Compare(current, AutomationElement.RootElement))
            {
                ancestors.Add(new ElementInfo
                {
                    AutomationId = current.Current.AutomationId,
                    Name = current.Current.Name,
                    ControlType = current.Current.ControlType.LocalizedControlType,
                    IsEnabled = current.Current.IsEnabled,
                    ProcessId = current.Current.ProcessId,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = current.Current.BoundingRectangle.X,
                        Y = current.Current.BoundingRectangle.Y,
                        Width = current.Current.BoundingRectangle.Width,
                        Height = current.Current.BoundingRectangle.Height
                    }
                });
                current = TreeWalker.ControlViewWalker.GetParent(current);
            }

            return Task.FromResult(new OperationResult { Success = true, Data = ancestors });
        }
    }
}