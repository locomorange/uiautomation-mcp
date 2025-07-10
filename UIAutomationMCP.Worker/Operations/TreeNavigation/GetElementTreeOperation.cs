using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.TreeNavigation
{
    public class GetElementTreeOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;

        public GetElementTreeOperation(ElementFinderService elementFinderService)
        {
            _elementFinderService = elementFinderService;
        }

        public Task<OperationResult> ExecuteAsync(WorkerRequest request)
        {
            var windowTitle = request.Parameters?.GetValueOrDefault("windowTitle")?.ToString() ?? "";
            var processId = request.Parameters?.GetValueOrDefault("processId")?.ToString() is string processIdStr && 
                int.TryParse(processIdStr, out var parsedProcessId) ? parsedProcessId : 0;
            var maxDepth = request.Parameters?.GetValueOrDefault("maxDepth")?.ToString() is string maxDepthStr && 
                int.TryParse(maxDepthStr, out var parsedMaxDepth) ? parsedMaxDepth : 3;

            var root = _elementFinderService.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
            var tree = BuildElementTree(root, maxDepth, 0);

            return Task.FromResult(new OperationResult { Success = true, Data = tree });
        }

        private ElementTreeNode BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            var node = new ElementTreeNode
            {
                AutomationId = element.Current.AutomationId,
                Name = element.Current.Name,
                ControlType = element.Current.ControlType.LocalizedControlType,
                IsEnabled = element.Current.IsEnabled,
                ProcessId = element.Current.ProcessId,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                Children = new List<ElementTreeNode>()
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
    }

    public class ElementTreeNode
    {
        public string AutomationId { get; set; } = "";
        public string Name { get; set; } = "";
        public string ControlType { get; set; } = "";
        public bool IsEnabled { get; set; }
        public int ProcessId { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public List<ElementTreeNode> Children { get; set; } = new();
    }
}