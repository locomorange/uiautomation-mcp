using System.Windows.Automation;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations
{
    /// <summary>
    /// Tree navigation operations with minimal UIAutomation API granularity
    /// </summary>
    public class TreeNavigationOperations
    {
        private readonly ElementFinderService _elementFinderService;

        public TreeNavigationOperations(ElementFinderService? elementFinderService = null)
        {
            _elementFinderService = elementFinderService ?? new ElementFinderService();
        }

        /// <summary>
        /// Get children of an element
        /// </summary>
        public OperationResult GetChildren(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
            var childrenInfo = new List<object>();

            foreach (AutomationElement child in children)
            {
                if (child != null)
                {
                    childrenInfo.Add(new
                    {
                        AutomationId = child.Current.AutomationId ?? "",
                        Name = child.Current.Name ?? "",
                        ControlType = child.Current.ControlType.LocalizedControlType,
                        IsEnabled = child.Current.IsEnabled,
                        BoundingRectangle = new
                        {
                            X = child.Current.BoundingRectangle.X,
                            Y = child.Current.BoundingRectangle.Y,
                            Width = child.Current.BoundingRectangle.Width,
                            Height = child.Current.BoundingRectangle.Height
                        }
                    });
                }
            }

            return new OperationResult { Success = true, Data = childrenInfo };
        }

        /// <summary>
        /// Get parent of an element
        /// </summary>
        public OperationResult GetParent(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var parent = TreeWalker.ControlViewWalker.GetParent(element);
            if (parent == null)
                return new OperationResult { Success = true, Data = null };

            var parentInfo = new
            {
                AutomationId = parent.Current.AutomationId ?? "",
                Name = parent.Current.Name ?? "",
                ControlType = parent.Current.ControlType.LocalizedControlType,
                IsEnabled = parent.Current.IsEnabled,
                BoundingRectangle = new
                {
                    X = parent.Current.BoundingRectangle.X,
                    Y = parent.Current.BoundingRectangle.Y,
                    Width = parent.Current.BoundingRectangle.Width,
                    Height = parent.Current.BoundingRectangle.Height
                }
            };

            return new OperationResult { Success = true, Data = parentInfo };
        }

        /// <summary>
        /// Get siblings of an element
        /// </summary>
        public OperationResult GetSiblings(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var parent = TreeWalker.ControlViewWalker.GetParent(element);
            if (parent == null)
                return new OperationResult { Success = true, Data = new List<object>() };

            var siblings = parent.FindAll(TreeScope.Children, Condition.TrueCondition);
            var siblingsInfo = new List<object>();

            foreach (AutomationElement sibling in siblings)
            {
                if (sibling != null && !sibling.Equals(element))
                {
                    siblingsInfo.Add(new
                    {
                        AutomationId = sibling.Current.AutomationId ?? "",
                        Name = sibling.Current.Name ?? "",
                        ControlType = sibling.Current.ControlType.LocalizedControlType,
                        IsEnabled = sibling.Current.IsEnabled,
                        BoundingRectangle = new
                        {
                            X = sibling.Current.BoundingRectangle.X,
                            Y = sibling.Current.BoundingRectangle.Y,
                            Width = sibling.Current.BoundingRectangle.Width,
                            Height = sibling.Current.BoundingRectangle.Height
                        }
                    });
                }
            }

            return new OperationResult { Success = true, Data = siblingsInfo };
        }

        /// <summary>
        /// Get descendants of an element
        /// </summary>
        public OperationResult GetDescendants(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var descendants = element.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            var descendantsInfo = new List<object>();

            foreach (AutomationElement descendant in descendants)
            {
                if (descendant != null)
                {
                    descendantsInfo.Add(new
                    {
                        AutomationId = descendant.Current.AutomationId ?? "",
                        Name = descendant.Current.Name ?? "",
                        ControlType = descendant.Current.ControlType.LocalizedControlType,
                        IsEnabled = descendant.Current.IsEnabled,
                        BoundingRectangle = new
                        {
                            X = descendant.Current.BoundingRectangle.X,
                            Y = descendant.Current.BoundingRectangle.Y,
                            Width = descendant.Current.BoundingRectangle.Width,
                            Height = descendant.Current.BoundingRectangle.Height
                        }
                    });
                }
            }

            return new OperationResult { Success = true, Data = descendantsInfo };
        }

        /// <summary>
        /// Get ancestors of an element
        /// </summary>
        public OperationResult GetAncestors(string elementId, string windowTitle = "", int processId = 0)
        {
            var element = _elementFinderService.FindElementById(elementId, windowTitle, processId);
            if (element == null)
                return new OperationResult { Success = false, Error = $"Element '{elementId}' not found" };

            // Let exceptions flow naturally - no try-catch
            var ancestors = new List<object>();
            var current = TreeWalker.ControlViewWalker.GetParent(element);

            while (current != null)
            {
                ancestors.Add(new
                {
                    AutomationId = current.Current.AutomationId ?? "",
                    Name = current.Current.Name ?? "",
                    ControlType = current.Current.ControlType.LocalizedControlType,
                    IsEnabled = current.Current.IsEnabled,
                    BoundingRectangle = new
                    {
                        X = current.Current.BoundingRectangle.X,
                        Y = current.Current.BoundingRectangle.Y,
                        Width = current.Current.BoundingRectangle.Width,
                        Height = current.Current.BoundingRectangle.Height
                    }
                });

                current = TreeWalker.ControlViewWalker.GetParent(current);
            }

            return new OperationResult { Success = true, Data = ancestors };
        }

        /// <summary>
        /// Get element tree with specified depth
        /// </summary>
        public OperationResult GetElementTree(string windowTitle = "", int processId = 0, int maxDepth = 3)
        {
            var searchRoot = _elementFinderService.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;

            // Let exceptions flow naturally - no try-catch
            var tree = BuildElementTree(searchRoot, maxDepth, 0);
            return new OperationResult { Success = true, Data = tree };
        }

        private object BuildElementTree(AutomationElement element, int maxDepth, int currentDepth)
        {
            var elementInfo = new
            {
                AutomationId = element.Current.AutomationId ?? "",
                Name = element.Current.Name ?? "",
                ControlType = element.Current.ControlType.LocalizedControlType,
                IsEnabled = element.Current.IsEnabled,
                BoundingRectangle = new
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                Children = currentDepth < maxDepth ? GetChildrenForTree(element, maxDepth, currentDepth + 1) : new List<object>()
            };

            return elementInfo;
        }

        private List<object> GetChildrenForTree(AutomationElement parent, int maxDepth, int currentDepth)
        {
            var children = new List<object>();
            var childElements = parent.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in childElements)
            {
                if (child != null)
                {
                    children.Add(BuildElementTree(child, maxDepth, currentDepth));
                }
            }

            return children;
        }

    }
}
