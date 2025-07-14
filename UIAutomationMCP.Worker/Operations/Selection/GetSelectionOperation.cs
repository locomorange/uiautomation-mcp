using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.Selection
{
    public class GetSelectionOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetSelectionOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<SelectionInfoResult>> ExecuteAsync(WorkerRequest request)
        {
            var containerElementId = request.Parameters?.GetValueOrDefault("containerElementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(containerElementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<SelectionInfoResult> 
                { 
                    Success = false, 
                    Error = "Container element not found",
                    Data = new SelectionInfoResult()
                });

            if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var pattern) || pattern is not SelectionPattern selectionPattern)
                return Task.FromResult(new OperationResult<SelectionInfoResult> 
                { 
                    Success = false, 
                    Error = "SelectionPattern not supported",
                    Data = new SelectionInfoResult()
                });

            var result = new SelectionInfoResult
            {
                CanSelectMultiple = selectionPattern.Current.CanSelectMultiple,
                IsSelectionRequired = selectionPattern.Current.IsSelectionRequired
            };

            var selection = selectionPattern.Current.GetSelection();
            foreach (AutomationElement selectedElement in selection)
            {
                if (selectedElement != null)
                {
                    result.SelectedItems.Add(new ElementInfo
                    {
                        AutomationId = selectedElement.Current.AutomationId,
                        Name = selectedElement.Current.Name,
                        ControlType = selectedElement.Current.ControlType.LocalizedControlType,
                        ClassName = selectedElement.Current.ClassName,
                        IsEnabled = selectedElement.Current.IsEnabled,
                        IsVisible = !selectedElement.Current.IsOffscreen,
                        ProcessId = selectedElement.Current.ProcessId,
                        BoundingRectangle = new BoundingRectangle
                        {
                            X = selectedElement.Current.BoundingRectangle.X,
                            Y = selectedElement.Current.BoundingRectangle.Y,
                            Width = selectedElement.Current.BoundingRectangle.Width,
                            Height = selectedElement.Current.BoundingRectangle.Height
                        }
                    });
                }
            }

            return Task.FromResult(new OperationResult<SelectionInfoResult> 
            { 
                Success = true, 
                Data = result 
            });
        }

        Task<OperationResult> IUIAutomationOperation.ExecuteAsync(WorkerRequest request)
        {
            var typedResult = ExecuteAsync(request);
            return Task.FromResult(new OperationResult
            {
                Success = typedResult.Result.Success,
                Error = typedResult.Result.Error,
                Data = typedResult.Result.Data,
                ExecutionSeconds = typedResult.Result.ExecutionSeconds
            });
        }
    }
}
