using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Accessibility
{
    public class GetLabeledByOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetLabeledByOperation(ElementFinderService elementFinderService)
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

            try
            {
                var labeledBy = element.Current.LabeledBy;
                if (labeledBy == null)
                    return Task.FromResult(new OperationResult { Success = false, Error = "Element is not labeled by another element" });

                var labelInfo = new Dictionary<string, object>
                {
                    ["AutomationId"] = labeledBy.Current.AutomationId,
                    ["Name"] = labeledBy.Current.Name,
                    ["ControlType"] = labeledBy.Current.ControlType.LocalizedControlType,
                    ["LocalizedControlType"] = labeledBy.Current.LocalizedControlType,
                    ["ClassName"] = labeledBy.Current.ClassName,
                    ["IsEnabled"] = labeledBy.Current.IsEnabled,
                    ["IsVisible"] = !labeledBy.Current.IsOffscreen,
                    ["BoundingRectangle"] = new BoundingRectangle
                    {
                        X = labeledBy.Current.BoundingRectangle.X,
                        Y = labeledBy.Current.BoundingRectangle.Y,
                        Width = labeledBy.Current.BoundingRectangle.Width,
                        Height = labeledBy.Current.BoundingRectangle.Height
                    },
                    ["HelpText"] = labeledBy.Current.HelpText,
                    ["AccessKey"] = labeledBy.Current.AccessKey,
                    ["AcceleratorKey"] = labeledBy.Current.AcceleratorKey
                };

                return Task.FromResult(new OperationResult { Success = true, Data = labelInfo });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult { Success = false, Error = $"Error getting labeled by element: {ex.Message}" });
            }
        }
    }
}