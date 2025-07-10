using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetDescendantsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetDescendantsOperation(ElementFinderService elementFinderService)
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

            var descendants = element.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            var descendantList = new List<ElementInfo>();

            foreach (AutomationElement descendant in descendants)
            {
                if (descendant != null)
                {
                    descendantList.Add(new ElementInfo
                    {
                        AutomationId = descendant.Current.AutomationId,
                        Name = descendant.Current.Name,
                        ControlType = descendant.Current.ControlType.LocalizedControlType,
                        IsEnabled = descendant.Current.IsEnabled,
                        ProcessId = descendant.Current.ProcessId,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = descendant.Current.BoundingRectangle.X,
                            Y = descendant.Current.BoundingRectangle.Y,
                            Width = descendant.Current.BoundingRectangle.Width,
                            Height = descendant.Current.BoundingRectangle.Height
                        }
                    });
                }
            }

            return Task.FromResult(new OperationResult { Success = true, Data = descendantList });
        }
    }
}