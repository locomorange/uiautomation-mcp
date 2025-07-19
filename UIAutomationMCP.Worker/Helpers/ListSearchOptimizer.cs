using System;
using System.Windows.Automation;
using System.Collections.Generic;
using System.Linq;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// Optimizes list item search based on control type (Win32 vs WPF)
    /// </summary>
    public static class ListSearchOptimizer
    {
        /// <summary>
        /// Determines the optimal search method for list items
        /// </summary>
        public static ListSearchMethod GetOptimalMethod(AutomationElement listElement)
        {
            if (listElement == null)
                return ListSearchMethod.FindAll;

            try
            {
                var frameworkId = listElement.Current.FrameworkId;
                
                // WPF controls perform better with FindAll
                if (IsWpfFramework(frameworkId))
                {
                    return ListSearchMethod.FindAll;
                }
                
                // Win32 controls perform better with TreeWalker
                if (IsWin32Framework(frameworkId))
                {
                    return ListSearchMethod.TreeWalker;
                }
                
                // Default to FindAll for unknown frameworks
                return ListSearchMethod.FindAll;
            }
            catch
            {
                // Fallback to FindAll if framework detection fails
                return ListSearchMethod.FindAll;
            }
        }

        /// <summary>
        /// Finds list items using the optimal method
        /// </summary>
        public static AutomationElement? FindListItemByIndex(AutomationElement listElement, int index)
        {
            if (listElement == null || index < 0)
                return null;

            var method = GetOptimalMethod(listElement);
            
            return method switch
            {
                ListSearchMethod.TreeWalker => FindListItemByIndexWithTreeWalker(listElement, index),
                ListSearchMethod.FindAll => FindListItemByIndexWithFindAll(listElement, index),
                _ => FindListItemByIndexWithFindAll(listElement, index)
            };
        }

        /// <summary>
        /// Finds all list items using the optimal method
        /// </summary>
        public static AutomationElementCollection? FindAllListItems(AutomationElement listElement)
        {
            if (listElement == null)
                return null;

            var method = GetOptimalMethod(listElement);
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem);

            return method switch
            {
                ListSearchMethod.TreeWalker => FindAllListItemsWithTreeWalker(listElement, condition),
                ListSearchMethod.FindAll => listElement.FindAll(TreeScope.Children, condition),
                _ => listElement.FindAll(TreeScope.Children, condition)
            };
        }

        private static bool IsWpfFramework(string frameworkId)
        {
            return !string.IsNullOrEmpty(frameworkId) && 
                   (frameworkId.Equals("WPF", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("WinUI", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("UWP", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsWin32Framework(string frameworkId)
        {
            return !string.IsNullOrEmpty(frameworkId) && 
                   (frameworkId.Equals("Win32", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("WinForm", StringComparison.OrdinalIgnoreCase) ||
                    frameworkId.Equals("Windows Forms", StringComparison.OrdinalIgnoreCase));
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
                // Fallback to FindAll if TreeWalker fails
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

        private static AutomationElementCollection? FindAllListItemsWithTreeWalker(AutomationElement listElement, Condition condition)
        {
            try
            {
                var walker = new TreeWalker(condition);
                var items = new List<AutomationElement>();
                
                var currentElement = walker.GetFirstChild(listElement);
                while (currentElement != null)
                {
                    items.Add(currentElement);
                    currentElement = walker.GetNextSibling(currentElement);
                }
                
                // Convert to AutomationElementCollection-like structure
                return listElement.FindAll(TreeScope.Children, condition);
            }
            catch
            {
                // Fallback to FindAll
                return listElement.FindAll(TreeScope.Children, condition);
            }
        }
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