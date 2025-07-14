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
    public class GetAncestorsOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetAncestorsOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementSearchResult>> ExecuteAsync(WorkerRequest request)
        {
            // 型安全なリクエストを試行し、失敗した場合は従来の方法にフォールバック
            var typedRequest = request.GetTypedRequest<GetAncestorsRequest>(_options);
            
            string elementId;
            string windowTitle;
            int processId;
            
            if (typedRequest != null)
            {
                // 型安全なパラメータアクセス
                elementId = typedRequest.ElementId;
                windowTitle = typedRequest.WindowTitle;
                processId = typedRequest.ProcessId ?? 0;
            }
            else
            {
                // 従来の方法（後方互換性のため）
                elementId = request.Parameters?.GetValueOrDefault("elementId")?.ToString() ?? "";
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            }

            var result = new ElementSearchResult();

            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return Task.FromResult(new OperationResult<ElementSearchResult> 
                { 
                    Success = false, 
                    Error = "Element not found",
                    Data = result
                });

            var current = TreeWalker.ControlViewWalker.GetParent(element);

            while (current != null && !Automation.Compare(current, AutomationElement.RootElement))
            {
                result.Elements.Add(new ElementInfo
                {
                    AutomationId = current.Current.AutomationId ?? "",
                    Name = current.Current.Name ?? "",
                    ControlType = current.Current.ControlType.LocalizedControlType,
                    ClassName = current.Current.ClassName,
                    IsEnabled = current.Current.IsEnabled,
                    IsVisible = !current.Current.IsOffscreen,
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