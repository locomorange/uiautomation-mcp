using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
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

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var result = new ElementSearchResult();

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = result
                });

            var parent = TreeWalker.ControlViewWalker.GetParent(element);
            if (parent == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = true, 
                    Data = result
                });

            result.Elements.Add(new ElementInfo
            {
                AutomationId = parent.Current.AutomationId ?? "",
                Name = parent.Current.Name ?? "",
                ControlType = parent.Current.ControlType.LocalizedControlType,
                ClassName = parent.Current.ClassName,
                IsEnabled = parent.Current.IsEnabled,
                IsVisible = !parent.Current.IsOffscreen,
                ProcessId = parent.Current.ProcessId,
                BoundingRectangle = new BoundingRectangle
                {
                    X = parent.Current.BoundingRectangle.X,
                    Y = parent.Current.BoundingRectangle.Y,
                    Width = parent.Current.BoundingRectangle.Width,
                    Height = parent.Current.BoundingRectangle.Height
                }
            });

            return Task.FromResult(new OperationResult<ElementSearchResult> 
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
