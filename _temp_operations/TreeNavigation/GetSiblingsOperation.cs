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
    public class GetSiblingsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetSiblingsOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            var typedRequest = request.GetTypedRequest<GetSiblingsRequest>(_options);
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

            var parent = TreeWalker.ControlViewWalker.GetParent(element);
            if (parent == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = "Parent element not found",
                    Data = result
                });

            var child = TreeWalker.ControlViewWalker.GetFirstChild(parent);
            
            while (child != null)
            {
                if (!Automation.Compare(child, element)) // Exclude the element itself
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
                child = TreeWalker.ControlViewWalker.GetNextSibling(child);
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