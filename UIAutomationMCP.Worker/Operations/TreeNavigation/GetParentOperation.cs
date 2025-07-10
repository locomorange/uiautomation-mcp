using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetParentOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetParentOperation(ElementFinderService elementFinderService)
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
                return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });

            var parent = TreeWalker.ControlViewWalker.GetParent(element);
            if (parent == null)
                return Task.FromResult(new OperationResult { Success = true, Data = null });

            var parentInfo = new
            {
                AutomationId = parent.Current.AutomationId ?? "",
                Name = parent.Current.Name ?? "",
                ControlType = parent.Current.ControlType.LocalizedControlType,
                IsEnabled = parent.Current.IsEnabled,
                BoundingRectangle = new
                {
                    X = parent.Current.BoundingRectangle.X,
                    Y = parent.Current.BoundingRectangle.Y,
                    Width = parent.Current.BoundingRectangle.Width,
                    Height = parent.Current.BoundingRectangle.Height
                }
            };

            return Task.FromResult(new OperationResult { Success = true, Data = parentInfo });
        }
    }
}
