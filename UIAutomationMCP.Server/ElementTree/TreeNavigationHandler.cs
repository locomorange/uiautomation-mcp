using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcp.Models;
using UiAutomationMcpServer.Core;
using UiAutomationMcpServer.Helpers;

namespace UiAutomationMcpServer.ElementTree
{
    /// <summary>
    /// Handles UI Automation tree navigation operations
    /// </summary>
    public class TreeNavigationHandler : BaseAutomationHandler
    {
        public TreeNavigationHandler(
            ILogger<TreeNavigationHandler> logger,
            AutomationHelper automationHelper)
            : base(logger, automationHelper)
        {
        }

        /// <summary>
        /// Gets the UI element tree
        /// </summary>
        public async Task<WorkerResult> ExecuteGetTreeAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, async cancellationToken =>
            {
                var windowTitle = operation.Parameters.TryGetValue("windowTitle", out var wt) ? wt?.ToString() : null;
                var processId = operation.Parameters.TryGetValue("processId", out var pid) ? Convert.ToInt32(pid) : (int?)null;
                var maxDepth = operation.Parameters.TryGetValue("maxDepth", out var md) ? Convert.ToInt32(md) : 3;
                
                _logger.LogInformation("[TreeNavigationHandler] Getting element tree for window '{WindowTitle}', maxDepth: {MaxDepth}", windowTitle, maxDepth);

                var rootElement = await FindRootElementAsync(windowTitle, processId, cancellationToken);
                if (rootElement == null)
                {
                    throw new InvalidOperationException("Root element not found");
                }

                var tree = await BuildElementTreeAsync(rootElement, maxDepth, 0, cancellationToken);
                
                return new WorkerResult
                {
                    Success = true,
                    Data = new Dictionary<string, object>
                    { 
                        ["tree"] = tree,
                        ["maxDepth"] = maxDepth,
                        ["windowTitle"] = windowTitle ?? ""
                    }
                };
            }, "GetTree");
        }

        /// <summary>
        /// Gets children of the specified element
        /// </summary>
        public async Task<WorkerResult> ExecuteGetChildrenAsync(WorkerOperation operation)
        {
            return await ExecuteWithTimeoutAsync(operation, async cancellationToken =>
            {
                var elementId = operation.Parameters.TryGetValue("elementId", out var eid) ? eid?.ToString() : null;
                var windowTitle = operation.Parameters.TryGetValue("windowTitle", out var wt) ? wt?.ToString() : null;
                
                _logger.LogInformation("[TreeNavigationHandler] Getting children for element '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle, cancellationToken);
                if (element == null)
                {
                    throw new InvalidOperationException("Element not found");
                }

                var children = await GetDirectChildrenAsync(element, cancellationToken);
                
                return new WorkerResult
                {
                    Success = true,
                    Data = new Dictionary<string, object>
                    { 
                        ["children"] = children,
                        ["count"] = children.Count
                    }
                };
            }, "GetChildren");
        }

        /// <summary>
        /// Gets the UI element tree (synchronous version)
        /// </summary>
        public WorkerResult GetElementTree(WorkerOperation operation)
        {
            try
            {
                var windowTitle = operation.Parameters.TryGetValue("WindowTitle", out var wt) ? wt?.ToString() : null;
                var processId = operation.Parameters.TryGetValue("ProcessId", out var pid) && pid != null ? Convert.ToInt32(pid) : (int?)null;
                var maxDepth = operation.Parameters.TryGetValue("MaxDepth", out var md) && md != null ? Convert.ToInt32(md) : 3;
                
                _logger.LogInformation("[TreeNavigationHandler] Getting element tree for window '{WindowTitle}', maxDepth: {MaxDepth}", windowTitle, maxDepth);

                var rootElement = FindRootElement(windowTitle, processId);
                if (rootElement == null)
                {
                    return new WorkerResult
                    {
                        Success = false,
                        Error = "Root element not found"
                    };
                }

                var tree = BuildElementTree(rootElement, maxDepth, 0);
                
                return new WorkerResult
                {
                    Success = true,
                    Data = new Dictionary<string, object>
                    { 
                        ["tree"] = tree,
                        ["maxDepth"] = maxDepth,
                        ["windowTitle"] = windowTitle ?? ""
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TreeNavigationHandler] GetElementTree failed");
                return new WorkerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Finds the root element for tree operations (synchronous version)
        /// </summary>
        private AutomationElement? FindRootElement(string? windowTitle, int? processId)
        {
            try
            {
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var condition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                    var window = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
                    if (window != null)
                    {
                        _logger.LogInformation("[TreeNavigationHandler] Found window by title: '{WindowTitle}'", windowTitle);
                        return window;
                    }
                }

                if (processId.HasValue && processId > 0)
                {
                    var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId.Value);
                    var windows = AutomationElement.RootElement.FindAll(TreeScope.Children, condition);
                    if (windows != null && windows.Count > 0)
                    {
                        _logger.LogInformation("[TreeNavigationHandler] Found {Count} window(s) by ProcessId: {ProcessId}", windows.Count, processId);
                        return windows[0];
                    }
                }

                _logger.LogInformation("[TreeNavigationHandler] Using desktop root element");
                return AutomationElement.RootElement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TreeNavigationHandler] Failed to find root element");
                return AutomationElement.RootElement;
            }
        }

        /// <summary>
        /// Builds the element tree recursively (synchronous version)
        /// </summary>
        private Dictionary<string, object> BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            try
            {
                var nodeInfo = new Dictionary<string, object>
                {
                    ["name"] = element.Current.Name ?? "",
                    ["automationId"] = element.Current.AutomationId ?? "",
                    ["controlType"] = element.Current.ControlType?.ProgrammaticName ?? "",
                    ["className"] = element.Current.ClassName ?? "",
                    ["boundingRectangle"] = new Dictionary<string, object>
                    {
                        ["x"] = element.Current.BoundingRectangle.X,
                        ["y"] = element.Current.BoundingRectangle.Y,
                        ["width"] = element.Current.BoundingRectangle.Width,
                        ["height"] = element.Current.BoundingRectangle.Height
                    },
                    ["isEnabled"] = element.Current.IsEnabled,
                    ["hasKeyboardFocus"] = element.Current.HasKeyboardFocus,
                    ["processId"] = element.Current.ProcessId,
                    ["depth"] = currentDepth
                };

                if (currentDepth < maxDepth)
                {
                    var children = new List<Dictionary<string, object>>();
                    try
                    {
                        var childElements = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                        if (childElements != null)
                        {
                            foreach (AutomationElement child in childElements)
                            {
                                try
                                {
                                    var childTree = BuildElementTree(child, maxDepth, currentDepth + 1);
                                    children.Add(childTree);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogDebug(ex, "[TreeNavigationHandler] Failed to process child element at depth {Depth}", currentDepth + 1);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[TreeNavigationHandler] Failed to get children for element at depth {Depth}", currentDepth);
                    }
                    nodeInfo["children"] = children;
                }
                else
                {
                    nodeInfo["children"] = new List<object>();
                }

                return nodeInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TreeNavigationHandler] Failed to build element tree node at depth {Depth}", currentDepth);
                return new Dictionary<string, object>
                {
                    ["name"] = "Error",
                    ["error"] = ex.Message,
                    ["depth"] = currentDepth,
                    ["children"] = new List<object>()
                };
            }
        }

        private async Task<AutomationElement?> FindRootElementAsync(string? windowTitle, int? processId, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                
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
            }, cancellationToken);
        }

        private async Task<AutomationElement?> FindElementAsync(string? elementId, string? windowTitle, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                
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
            }, cancellationToken);
        }

        private async Task<Dictionary<string, object>> BuildElementTreeAsync(AutomationElement element, int maxDepth, int currentDepth, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var elementInfo = CreateElementInfo(element);
                var treeNode = new Dictionary<string, object>
                {
                    ["element"] = elementInfo,
                    ["depth"] = currentDepth,
                    ["children"] = new List<Dictionary<string, object>>()
                };

                if (currentDepth < maxDepth)
                {
                    try
                    {
                        var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                        var childTasks = new List<Task<Dictionary<string, object>>>();

                        foreach (AutomationElement child in children)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            childTasks.Add(BuildElementTreeAsync(child, maxDepth, currentDepth + 1, cancellationToken));
                        }

                        var childResults = await Task.WhenAll(childTasks);
                        treeNode["children"] = childResults.ToList();
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[TreeNavigationHandler] Error getting children for element at depth {Depth}", currentDepth);
                    }
                }

                return treeNode;
            }, cancellationToken);
        }

        private async Task<List<Dictionary<string, object>>> GetDirectChildrenAsync(AutomationElement element, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var children = new List<Dictionary<string, object>>();
                
                try
                {
                    var childElements = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                    
                    foreach (AutomationElement child in childElements)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        var childInfo = CreateElementInfo(child);
                        children.Add(new Dictionary<string, object>
                        {
                            ["element"] = childInfo,
                            ["hasChildren"] = HasChildren(child)
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[TreeNavigationHandler] Error getting direct children");
                }

                return children;
            }, cancellationToken);
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
                        X = SafeDoubleValue(element.Current.BoundingRectangle.X),
                        Y = SafeDoubleValue(element.Current.BoundingRectangle.Y),
                        Width = SafeDoubleValue(element.Current.BoundingRectangle.Width),
                        Height = SafeDoubleValue(element.Current.BoundingRectangle.Height)
                    },
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    HelpText = element.Current.HelpText ?? ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TreeNavigationHandler] Error creating ElementInfo");
                return new ElementInfo
                {
                    Name = "Error reading element",
                    AutomationId = "",
                    ControlType = "Unknown"
                };
            }
        }

        /// <summary>
        /// double値の安全な変換（NaN/Infinityチェック付き）
        /// </summary>
        private double SafeDoubleValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < -1000000 || value > 1000000)
            {
                return 0.0;
            }
            return Math.Round(value, 2);
        }
    }
}