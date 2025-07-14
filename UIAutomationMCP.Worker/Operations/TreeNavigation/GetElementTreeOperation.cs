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
    public class GetElementTreeOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly IOptions<UIAutomationOptions> _options;

        public GetElementTreeOperation(ElementFinderService elementFinderService, IOptions<UIAutomationOptions> options)
        {
            _elementFinderService = elementFinderService;
            _options = options;
        }

        public Task<OperationResult<ElementTreeResult>> ExecuteAsync(WorkerRequest request)
        {
            // 型安全なリクエストを試行し、失敗した場合は従来の方法にフォールバック
            var typedRequest = request.GetTypedRequest<GetElementTreeRequest>(_options);
            
            string windowTitle;
            int processId;
            int maxDepth;
            
            if (typedRequest != null)
            {
                // 型安全なパラメータアクセス
                windowTitle = typedRequest.WindowTitle ?? "";
                processId = typedRequest.ProcessId ?? 0;
                maxDepth = typedRequest.MaxDepth;
            }
            else
            {
                // 従来の方法（後方互換性のため）
                windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
                processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                    int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
                maxDepth = request.Parameters?.GetValueOrDefault("maxDepth")?.ToString() is string maxDepthStr && 
                    int.TryParse(maxDepthStr, out var parsedMaxDepth) ? parsedMaxDepth : 3;
            }

            var root = _elementFinderService.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var rootNode = BuildElementTree(root, maxDepth, 0);
            
            var result = new ElementTreeResult
            {
                RootNode = rootNode,
                TotalElements = CountElements(rootNode),
                MaxDepth = GetMaxDepth(rootNode)
            };

            return Task.FromResult(new OperationResult<ElementTreeResult> 
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

        private UIAutomationMCP.Shared.Results.TreeNode BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            var node = new UIAutomationMCP.Shared.Results.TreeNode
            {
                Element = new ElementInfo
                {
                    AutomationId = element.Current.AutomationId,
                    Name = element.Current.Name,
                    ControlType = element.Current.ControlType.LocalizedControlType,
                    ClassName = element.Current.ClassName,
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    ProcessId = element.Current.ProcessId,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = element.Current.BoundingRectangle.X,
                        Y = element.Current.BoundingRectangle.Y,
                        Width = element.Current.BoundingRectangle.Width,
                        Height = element.Current.BoundingRectangle.Height
                    }
                },
                Depth = currentDepth
            };

            if (currentDepth < maxDepth)
            {
                var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                foreach (AutomationElement child in children)
                {
                    if (child != null)
                    {
                        node.Children.Add(BuildElementTree(child, maxDepth, currentDepth + 1));
                    }
                }
            }

            return node;
        }

        private int CountElements(UIAutomationMCP.Shared.Results.TreeNode node)
        {
            int count = 1; // Count this node
            foreach (var child in node.Children)
            {
                count += CountElements(child);
            }
            return count;
        }

        private int GetMaxDepth(UIAutomationMCP.Shared.Results.TreeNode node)
        {
            if (node.Children.Count == 0)
                return node.Depth;

            int maxChildDepth = node.Depth;
            foreach (var child in node.Children)
            {
                int childDepth = GetMaxDepth(child);
                if (childDepth > maxChildDepth)
                    maxChildDepth = childDepth;
            }
            return maxChildDepth;
        }
    }
}