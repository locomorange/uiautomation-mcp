using System.Collections.ObjectModel;
using System.Windows.Automation;
using UIAutomationMCP.Subprocess.Core.Services;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Subprocess.Core.Helpers;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Subprocess.Worker.Extensions
{
    /// <summary>
    /// Extension methods for ElementFinderService to support Worker operations
    /// </summary>
    public static class ElementFinderServiceExtensions
    {
        public static AutomationElementCollection FindElementsAdvanced(this ElementFinderService service, AdvancedSearchParameters parameters)
        {
            // Simple implementation for now - find all elements and filter
            var rootElement = AutomationElement.RootElement;
            var condition = Condition.TrueCondition;
            
            if (!string.IsNullOrEmpty(parameters.AutomationId))
            {
                condition = new PropertyCondition(AutomationElement.AutomationIdProperty, parameters.AutomationId);
            }
            else if (!string.IsNullOrEmpty(parameters.Name))
            {
                condition = new PropertyCondition(AutomationElement.NameProperty, parameters.Name);
            }
            
            return rootElement.FindAll(TreeScope.Descendants, condition);
        }

        public static List<AutomationElement> ApplyFuzzyFilter(this ElementFinderService service, AutomationElementCollection elements, AdvancedSearchParameters parameters)
        {
            var result = new List<AutomationElement>();
            foreach (AutomationElement element in elements)
            {
                if (element != null)
                {
                    result.Add(element);
                }
            }
            return result;
        }

        public static List<AutomationElement> ApplyPatternFilter(this ElementFinderService service, List<AutomationElement> elements, AdvancedSearchParameters parameters)
        {
            // Return as-is for now
            return elements;
        }

        public static List<AutomationElement> SortElements(this ElementFinderService service, List<AutomationElement> elements, string sortBy)
        {
            // Simple alphabetical sort by name
            return elements.OrderBy(e => 
            {
                try 
                { 
                    return e.Current.Name ?? string.Empty; 
                } 
                catch 
                { 
                    return string.Empty; 
                }
            }).ToList();
        }

        public static AutomationElement GetSearchRoot(this ElementFinderService service, int? processId, string? windowTitle)
        {
            if (processId.HasValue)
            {
                var condition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId.Value);
                var processWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children, condition);
                if (processWindow != null)
                    return processWindow;
            }
            
            return AutomationElement.RootElement;
        }

        public static ElementInfo GetElementBasicInfo(this ElementFinderService service, AutomationElement element)
        {
            try
            {
                // Use ElementInfoBuilder for consistent element information extraction
                return ElementInfoBuilder.CreateElementInfo(element, false);
            }
            catch
            {
                return new ElementInfo();
            }
        }
    }
}

