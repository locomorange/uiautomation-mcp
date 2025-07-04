using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

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
        private readonly IElementUtilityService _elementUtilityService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public ElementTreeService(ILogger<ElementTreeService> logger, IWindowService windowService, IElementUtilityService elementUtilityService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _elementUtilityService = elementUtilityService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> GetElementTreeAsync(string? windowTitle = null, string treeView = "control", int maxDepth = 3, int? processId = null)
        {
            try
            {
                AutomationElement? rootElement = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    rootElement = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (rootElement == null)
                    {
                        return new OperationResult { Success = false, Error = $"Window '{windowTitle}' not found" };
                    }
                }
                else
                {
                    rootElement = AutomationElement.RootElement;
                }

                var treeScope = GetTreeScope(treeView);
                var tree = await BuildElementTreeAsync(rootElement, treeScope, maxDepth, 0);

                return new OperationResult { Success = true, Data = tree };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new OperationResult { Success = false, Error = ex.Message };
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

        private async Task<ElementTreeNode> BuildElementTreeAsync(AutomationElement element, TreeScope scope, int maxDepth, int currentDepth)
        {
            var node = new ElementTreeNode
            {
                Name = element.Current.Name ?? "",
                AutomationId = element.Current.AutomationId ?? "",
                ControlType = element.Current.ControlType.ProgrammaticName ?? "",
                ClassName = element.Current.ClassName ?? "",
                ProcessId = element.Current.ProcessId,
                IsEnabled = element.Current.IsEnabled,
                IsVisible = !element.Current.IsOffscreen,
                BoundingRectangle = new BoundingRectangle
                {
                    X = double.IsInfinity(element.Current.BoundingRectangle.X) ? 0 : element.Current.BoundingRectangle.X,
                    Y = double.IsInfinity(element.Current.BoundingRectangle.Y) ? 0 : element.Current.BoundingRectangle.Y,
                    Width = double.IsInfinity(element.Current.BoundingRectangle.Width) ? 0 : element.Current.BoundingRectangle.Width,
                    Height = double.IsInfinity(element.Current.BoundingRectangle.Height) ? 0 : element.Current.BoundingRectangle.Height
                },
                AvailableActions = _elementUtilityService.GetAvailableActions(element),
                Children = new List<ElementTreeNode>()
            };

            if (currentDepth < maxDepth)
            {
                try
                {
                    var childCondition = GetTreeViewCondition(scope);
                    var childrenResult = await _uiAutomationHelper.FindAllAsync(element, TreeScope.Children, childCondition, 30);

                    if (childrenResult.Success && childrenResult.Data != null)
                    {
                        foreach (AutomationElement child in childrenResult.Data)
                        {
                            try
                            {
                                var childNode = await BuildElementTreeAsync(child, scope, maxDepth, currentDepth + 1);
                                node.Children.Add(childNode);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error processing child element");
                                continue;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Error getting child elements: {Error}", childrenResult.Error);
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
        public int ProcessId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }
        public BoundingRectangle BoundingRectangle { get; set; } = new();
        public Dictionary<string, string> AvailableActions { get; set; } = new();
        public List<ElementTreeNode> Children { get; set; } = new();
    }
}