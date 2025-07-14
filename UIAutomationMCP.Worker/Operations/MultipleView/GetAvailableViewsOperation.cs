using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.MultipleView
{
    public class GetAvailableViewsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetAvailableViewsOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult<UIAutomationMCP.Shared.Results.AvailableViewsResult>> ExecuteAsync(WorkerRequest request)
        {
            var elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.AvailableViewsResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = new UIAutomationMCP.Shared.Results.AvailableViewsResult()
                });

            if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var pattern) || pattern is not MultipleViewPattern multipleViewPattern)
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.AvailableViewsResult> 
                { 
                    Success = false, 
                    Error = "MultipleViewPattern not supported",
                    Data = new UIAutomationMCP.Shared.Results.AvailableViewsResult()
                });

            try
            {
                var viewIds = multipleViewPattern.Current.GetSupportedViews();
                var currentViewId = multipleViewPattern.Current.CurrentView;
                
                var result = new UIAutomationMCP.Shared.Results.AvailableViewsResult
                {
                    CurrentViewId = currentViewId,
                    CurrentViewName = multipleViewPattern.GetViewName(currentViewId)
                };

                foreach (var viewId in viewIds)
                {
                    var viewName = multipleViewPattern.GetViewName(viewId);
                    var viewInfo = new UIAutomationMCP.Shared.Results.ViewInfo
                    {
                        ViewId = viewId,
                        ViewName = viewName,
                        IsCurrent = viewId == currentViewId
                    };
                    result.Views.Add(viewInfo);
                }

                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.AvailableViewsResult> 
                { 
                    Success = true, 
                    Data = result 
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new OperationResult<UIAutomationMCP.Shared.Results.AvailableViewsResult> 
                { 
                    Success = false, 
                    Error = $"Error getting available views: {ex.Message}",
                    Data = new UIAutomationMCP.Shared.Results.AvailableViewsResult()
                });
            }
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