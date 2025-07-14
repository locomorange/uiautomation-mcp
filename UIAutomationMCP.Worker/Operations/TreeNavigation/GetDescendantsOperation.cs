using System.Windows.Automation;
using Microsoft.Extensions.Options;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Options;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetDescendantsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetDescendantsOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetDescendantsRequest>(_options);
            if (typedRequest == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = "Invalid request format",
                    Data = new ElementSearchResult()
                });
            
            var elementId = typedRequest.ElementId;
            var windowTitle = typedRequest.WindowTitle;
            var processId = typedRequest.ProcessId ?? 0;

            var result = new ElementSearchResult();

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = result
                });

            var descendants = element.FindAll(TreeScope.Descendants, Condition.TrueCondition);

            foreach (AutomationElement descendant in descendants)
            {
                if (descendant != null)
                {
                    result.Elements.Add(new ElementInfo
                    {
                        AutomationId = descendant.Current.AutomationId ?? "",
                        Name = descendant.Current.Name ?? "",
                        ControlType = descendant.Current.ControlType.LocalizedControlType,
                        ClassName = descendant.Current.ClassName,
                        IsEnabled = descendant.Current.IsEnabled,
                        IsVisible = !descendant.Current.IsOffscreen,
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