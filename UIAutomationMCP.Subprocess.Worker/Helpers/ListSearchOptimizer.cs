using System;
using System.Windows.Automation;
using System.Collections.Generic;
using System.Linq;

namespace UIAutomationMCP.Subprocess.Worker.Helpers
{
    /// <summary>
    /// Optimizes list/container child element search based on framework (Win32 vs WPF).
    /// Win32 controls perform better with TreeWalker (step-by-step navigation),
    /// while WPF controls perform better with FindAll (batch COM call).
    /// </summary>
    public static class ListSearchOptimizer
    {
        // ControlTypes that are considered list-like containers
        private static readonly HashSet<ControlType> ListContainerTypes = new()
        {
            ControlType.List,
            ControlType.DataGrid,
            ControlType.Tree,
            ControlType.ComboBox
        };

        /// <summary>
        /// Determines the optimal search method based on the element's framework
        /// </summary>
        public static ListSearchMethod GetOptimalMethod(AutomationElement element)
        {
            if (element == null)
                return ListSearchMethod.FindAll;

            try
            {
                var frameworkId = element.Current.FrameworkId;

                if (IsWin32Framework(frameworkId))
                    return ListSearchMethod.TreeWalker;

                // WPF/WinUI/UWP and unknown frameworks default to FindAll
                return ListSearchMethod.FindAll;
            }
            catch
            {
                return ListSearchMethod.FindAll;
            }
        }

        /// <summary>
        /// Returns true if the given element is a list-like container (List, DataGrid, Tree, ComboBox)
        /// </summary>
        public static bool IsListContainer(AutomationElement element)
        {
            if (element == null)
                return false;

            try
            {
                return ListContainerTypes.Contains(element.Current.ControlType);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds all child elements matching the given condition, using the optimal method
        /// for the parent element's framework. This is the primary integration point for
        /// SearchElementsOperation when the search root is a list container.
        /// </summary>
        public static IReadOnlyList<AutomationElement> FindAllChildrenOptimized(
            AutomationElement parent, Condition condition)
        {
            if (parent == null)
                return Array.Empty<AutomationElement>();

            var method = GetOptimalMethod(parent);

            return method switch
            {
                ListSearchMethod.TreeWalker => FindChildrenWithTreeWalker(parent, condition),
                _ => FindChildrenWithFindAll(parent, condition)
            };
        }

        /// <summary>
        /// Finds all list items using the optimal method
        /// </summary>
        public static IReadOnlyList<AutomationElement> FindAllListItems(AutomationElement listElement)
        {
            if (listElement == null)
                return Array.Empty<AutomationElement>();

            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem);
            return FindAllChildrenOptimized(listElement, condition);
        }

        /// <summary>
        /// Finds a single list item by index using the optimal method
        /// </summary>
        public static AutomationElement? FindListItemByIndex(AutomationElement listElement, int index)
        {
            if (listElement == null || index < 0)
                return null;

            var method = GetOptimalMethod(listElement);

            return method switch
            {
                ListSearchMethod.TreeWalker => FindListItemByIndexWithTreeWalker(listElement, index),
                _ => FindListItemByIndexWithFindAll(listElement, index)
            };
        }

        #region Private helpers

        private static bool IsWin32Framework(string frameworkId)
        {
            return !string.IsNullOrEmpty(frameworkId) &&
                   (frameworkId.Equals("Win32", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("WinForm", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("Windows Forms", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// TreeWalker-based search: walks children one-by-one via GetFirstChild/GetNextSibling.
        /// More efficient for Win32 controls and avoids issues with large virtualized lists.
        /// </summary>
        private static List<AutomationElement> FindChildrenWithTreeWalker(
            AutomationElement parent, Condition condition)
        {
            try
            {
                var walker = new TreeWalker(condition);
                var items = new List<AutomationElement>();

                var current = walker.GetFirstChild(parent);
                while (current != null)
                {
                    items.Add(current);
                    current = walker.GetNextSibling(current);
                }

                return items;
            }
            catch
            {
                // Fallback to FindAll if TreeWalker fails
                return FindChildrenWithFindAll(parent, condition);
            }
        }

        /// <summary>
        /// FindAll-based search: single COM batch call. More efficient for WPF controls.
        /// </summary>
        private static List<AutomationElement> FindChildrenWithFindAll(
            AutomationElement parent, Condition condition)
        {
            try
            {
                var collection = parent.FindAll(TreeScope.Children, condition);
                var items = new List<AutomationElement>(collection.Count);

                foreach (AutomationElement element in collection)
                {
                    if (element != null)
                        items.Add(element);
                }

                return items;
            }
            catch
            {
                return new List<AutomationElement>();
            }
        }

        private static AutomationElement? FindListItemByIndexWithTreeWalker(AutomationElement listElement, int index)
        {
            try
            {
                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem);
                var walker = new TreeWalker(condition);

                var currentElement = walker.GetFirstChild(listElement);
                int currentIndex = 0;

                while (currentElement != null && currentIndex < index)
                {
                    currentElement = walker.GetNextSibling(currentElement);
                    currentIndex++;
                }

                return currentElement;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
            catch
            {
                return FindListItemByIndexWithFindAll(listElement, index);
            }
        }

        private static AutomationElement? FindListItemByIndexWithFindAll(AutomationElement listElement, int index)
        {
            try
            {
                var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem);
                var listItems = listElement.FindAll(TreeScope.Children, condition);

                if (index >= 0 && index < listItems.Count)
                {
                    return listItems[index];
                }

                return null;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Enumeration of available list search methods
    /// </summary>
    public enum ListSearchMethod
    {
        /// <summary>
        /// Use FindAll method (optimal for WPF controls)
        /// </summary>
        FindAll,

        /// <summary>
        /// Use TreeWalker method (optimal for Win32 controls)
        /// </summary>
        TreeWalker
    }
}

