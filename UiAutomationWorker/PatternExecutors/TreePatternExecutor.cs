using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;

namespace UiAutomationWorker.PatternExecutors
{
    public class TreePatternExecutor
    {
        private readonly ILogger<TreePatternExecutor> _logger;

        public TreePatternExecutor(ILogger<TreePatternExecutor> logger)
        {
            _logger = logger;
        }

        public async Task<object> GetTreeAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var processId = parameters.ContainsKey("ProcessId") ? Convert.ToInt32(parameters["ProcessId"]) : (int?)null;
                var maxDepth = parameters.ContainsKey("MaxDepth") ? Convert.ToInt32(parameters["MaxDepth"]) : 3;
                
                _logger.LogInformation("Getting element tree for window '{WindowTitle}', maxDepth: {MaxDepth}", windowTitle, maxDepth);

                var rootElement = await FindRootElementAsync(windowTitle, processId);
                if (rootElement == null)
                {
                    return new { Success = false, Error = "Root element not found" };
                }

                var tree = await BuildElementTreeAsync(rootElement, maxDepth, 0);
                
                return new { 
                    Success = true, 
                    Tree = tree,
                    MaxDepth = maxDepth,
                    WindowTitle = windowTitle
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element tree");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetChildrenAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Getting children for element '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var children = await GetDirectChildrenAsync(element);
                
                return new { 
                    Success = true, 
                    Children = children,
                    Count = children.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element children");
                return new { Success = false, Error = ex.Message };
            }
        }

        private async Task<AutomationElement?> FindRootElementAsync(string? windowTitle, int? processId)
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    Condition condition = processId.HasValue
                        ? new AndCondition(
                            new PropertyCondition(AutomationElement.NameProperty, windowTitle),
                            new PropertyCondition(AutomationElement.ProcessIdProperty, processId.Value))
                        : new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                    
                    return AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
                }
                
                return AutomationElement.RootElement;
            });
        }

        private async Task<AutomationElement?> FindElementAsync(string? elementId, string? windowTitle)
        {
            return await Task.Run(() =>
            {
                AutomationElement? searchRoot = null;

                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                    searchRoot = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
                    
                    if (searchRoot == null)
                    {
                        return null;
                    }
                }
                else
                {
                    searchRoot = AutomationElement.RootElement;
                }

                if (string.IsNullOrEmpty(elementId))
                {
                    return searchRoot;
                }

                var elementCondition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                );

                return searchRoot.FindFirst(TreeScope.Descendants, elementCondition);
            });
        }

        private async Task<Dictionary<string, object>> BuildElementTreeAsync(AutomationElement element, int maxDepth, int currentDepth)
        {
            return await Task.Run(() =>
            {
                var elementInfo = CreateElementInfo(element);
                var treeNode = new Dictionary<string, object>
                {
                    ["Element"] = elementInfo,
                    ["Depth"] = currentDepth,
                    ["Children"] = new List<Dictionary<string, object>>()
                };

                if (currentDepth < maxDepth)
                {
                    try
                    {
                        var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                        var childTasks = new List<Task<Dictionary<string, object>>>();

                        foreach (AutomationElement child in children)
                        {
                            childTasks.Add(BuildElementTreeAsync(child, maxDepth, currentDepth + 1));
                        }

                        var childResults = Task.WhenAll(childTasks).Result;
                        treeNode["Children"] = childResults.ToList();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting children for element at depth {Depth}", currentDepth);
                    }
                }

                return treeNode;
            });
        }

        private async Task<List<Dictionary<string, object>>> GetDirectChildrenAsync(AutomationElement element)
        {
            return await Task.Run(() =>
            {
                var children = new List<Dictionary<string, object>>();
                
                try
                {
                    var childElements = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                    
                    foreach (AutomationElement child in childElements)
                    {
                        var childInfo = CreateElementInfo(child);
                        children.Add(new Dictionary<string, object>
                        {
                            ["Element"] = childInfo,
                            ["HasChildren"] = HasChildren(child)
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting direct children");
                }

                return children;
            });
        }

        private bool HasChildren(AutomationElement element)
        {
            try
            {
                var firstChild = element.FindFirst(TreeScope.Children, Condition.TrueCondition);
                return firstChild != null;
            }
            catch
            {
                return false;
            }
        }

        private ElementInfo CreateElementInfo(AutomationElement element)
        {
            try
            {
                return new ElementInfo
                {
                    Name = element.Current.Name ?? "",
                    AutomationId = element.Current.AutomationId ?? "",
                    ControlType = element.Current.ControlType.ProgrammaticName ?? "",
                    ClassName = element.Current.ClassName ?? "",
                    ProcessId = element.Current.ProcessId,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = element.Current.BoundingRectangle.X,
                        Y = element.Current.BoundingRectangle.Y,
                        Width = element.Current.BoundingRectangle.Width,
                        Height = element.Current.BoundingRectangle.Height
                    },
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    HelpText = element.Current.HelpText ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating ElementInfo");
                return new ElementInfo
                {
                    Name = "Error reading element",
                    AutomationId = "",
                    ControlType = "Unknown"
                };
            }
        }
    }
}
