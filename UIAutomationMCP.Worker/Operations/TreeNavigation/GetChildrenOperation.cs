using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;
using UIAutomationMCP.Worker.Services;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetChildrenOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetChildrenOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<TreeNavigationResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetChildrenRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<TreeNavigationResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new TreeNavigationResult { NavigationType = "Children" }
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<TreeNavigationResult> 
                { 
                    Success = false, 
                    Error = $"Element '{elementId}' not found",
                    Data = new TreeNavigationResult { NavigationType = "Children" }
                });

            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
            var result = new TreeNavigationResult
            {
                NavigationType = "Children"
            };

            foreach (AutomationElement child in children)
            {
                if (child != null)
                {
                    result.Elements.Add(new ElementInfo
                    {
                        AutomationId = child.Current.AutomationId ?? "",
                        Name = child.Current.Name ?? "",
                        ControlType = child.Current.ControlType.LocalizedControlType,
                        ClassName = child.Current.ClassName,
                        IsEnabled = child.Current.IsEnabled,
                        IsVisible = !child.Current.IsOffscreen,
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
            }

            return Task.FromResult(new OperationResult<TreeNavigationResult> 
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
