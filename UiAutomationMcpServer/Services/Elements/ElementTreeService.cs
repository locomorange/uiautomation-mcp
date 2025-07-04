using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcp.Models;
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
        private readonly IUIAutomationWorker _uiAutomationWorker;

        public ElementTreeService(ILogger<ElementTreeService> logger, IWindowService windowService, IElementUtilityService elementUtilityService, IUIAutomationWorker uiAutomationWorker)
        {
            _logger = logger;
            _windowService = windowService;
            _elementUtilityService = elementUtilityService;
            _uiAutomationWorker = uiAutomationWorker;
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
                Name = SafeGetProperty(() => element.Current.Name) ?? "",
                AutomationId = SafeGetProperty(() => element.Current.AutomationId) ?? "",
                ControlType = SafeGetProperty(() => element.Current.ControlType.ProgrammaticName) ?? "",
                ClassName = SafeGetProperty(() => element.Current.ClassName) ?? "",
                ProcessId = SafeGetProperty(() => element.Current.ProcessId),
                IsEnabled = SafeGetProperty(() => element.Current.IsEnabled),
                IsVisible = SafeGetProperty(() => !element.Current.IsOffscreen, true),
                BoundingRectangle = SafeGetBoundingRectangle(element),
                AvailableActions = _elementUtilityService.GetAvailableActions(element),
                Children = new List<ElementTreeNode>()
            };

            if (currentDepth < maxDepth)
            {
                try
                {
                    var childCondition = GetTreeViewCondition(scope);
                    // 暫定的に直接AutomationAPIを使用（理想的にはWorkerを使用したい）
                    var children = await Task.Run(() => element.FindAll(TreeScope.Children, childCondition));

                    if (children != null)
                    {
                        foreach (AutomationElement child in children)
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

        private T? SafeGetProperty<T>(Func<T> propertyGetter, T? defaultValue = default)
        {
            try
            {
                return propertyGetter();
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to get property: {Error}", ex.Message);
                return defaultValue;
            }
        }

        private BoundingRectangle SafeGetBoundingRectangle(AutomationElement element)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                
                // Check for invalid values that would cause JSON serialization issues
                if (double.IsInfinity(rect.Left) || double.IsInfinity(rect.Top) ||
                    double.IsInfinity(rect.Width) || double.IsInfinity(rect.Height) ||
                    double.IsNaN(rect.Left) || double.IsNaN(rect.Top) ||
                    double.IsNaN(rect.Width) || double.IsNaN(rect.Height))
                {
                    _logger.LogDebug("Invalid BoundingRectangle values detected, using defaults");
                    return new BoundingRectangle { X = 0, Y = 0, Width = 0, Height = 0 };
                }
                
                return new BoundingRectangle
                {
                    X = rect.Left,
                    Y = rect.Top,
                    Width = rect.Width,
                    Height = rect.Height
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to get BoundingRectangle: {Error}", ex.Message);
                return new BoundingRectangle { X = 0, Y = 0, Width = 0, Height = 0 };
            }
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
