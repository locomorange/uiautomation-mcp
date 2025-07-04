using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementTreeService
    {
        Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null);
    }

    public class ElementTreeService : IElementTreeService
    {
        private readonly ILogger<ElementTreeService> _logger;
        private readonly IWindowService _windowService;

        public ElementTreeService(ILogger<ElementTreeService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null)
        {
            try
            {
                AutomationElement? rootElement = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    rootElement = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (rootElement == null)
                    {
                        return Task.FromResult(new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" });
                    }
                }
                else
                {
                    rootElement = AutomationElement.RootElement;
                }

                var treeScope = GetTreeScope(treeView);
                var tree = BuildElementTree(rootElement, treeScope, maxDepth, 0);

                return Task.FromResult(new OperationResult { Success = true, Data = tree });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        private TreeScope GetTreeScope(string treeView)
        {
            return treeView.ToLower() switch
            {
                "raw" => TreeScope.Children,
                "control" => TreeScope.Children,
                "content" => TreeScope.Children,
                _ => TreeScope.Children
            };
        }

        private ElementTreeNode BuildElementTree(AutomationElement element, TreeScope scope, int maxDepth, int currentDepth)
        {
            var node = new ElementTreeNode
            {
                Name = element.Current.Name ?? "",
                AutomationId = element.Current.AutomationId ?? "",
                ControlType = element.Current.ControlType.ProgrammaticName ?? "",
                ClassName = element.Current.ClassName ?? "",
                IsEnabled = element.Current.IsEnabled,
                IsVisible = !element.Current.IsOffscreen,
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
                try
                {
                    var childCondition = GetTreeViewCondition(scope);
                    var children = element.FindAll(TreeScope.Children, childCondition);

                    foreach (AutomationElement child in children)
                    {
                        try
                        {
                            var childNode = BuildElementTree(child, scope, maxDepth, currentDepth + 1);
                            node.Children.Add(childNode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error processing child element");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting child elements");
                }
            }

            return node;
        }

        private Condition GetTreeViewCondition(TreeScope scope)
        {
            // For different tree views, we could filter different elements
            // For now, we'll use TrueCondition to get all elements
            return Condition.TrueCondition;
        }
    }

    public class ElementTreeNode
    {
        public string Name { get; set; } = "";
        public string AutomationId { get; set; } = "";
        public string ControlType { get; set; } = "";
        public string ClassName { get; set; } = "";
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public List<ElementTreeNode> Children { get; set; } = new();
    }
}