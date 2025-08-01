using System.Windows.Automation;

namespace UIAutomationMCP.Subprocess.Core.Extensions
{
    /// <summary>
    /// AutomationElement extension methods for safe operations
    /// Shared between Worker and Monitor processes
    /// </summary>
    public static class AutomationElementExtensions
    {
        /// <summary>
        /// Safely get pattern from element
        /// </summary>
        public static TPattern? GetPattern<TPattern>(this AutomationElement element, AutomationPattern pattern) 
            where TPattern : class
        {
            try
            {
                if (element.TryGetCurrentPattern(pattern, out var patternObject))
                {
                    return patternObject as TPattern;
                }
                return null;
            }
            catch (ElementNotAvailableException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Safely get property value from element
        /// </summary>
        public static T? GetPropertyValue<T>(this AutomationElement element, AutomationProperty property)
        {
            try
            {
                var value = element.GetCurrentPropertyValue(property);
                return value is T typedValue ? typedValue : default;
            }
            catch (ElementNotAvailableException)
            {
                return default;
            }
            catch (InvalidOperationException)
            {
                return default;
            }
        }

        /// <summary>
        /// Safely find single element
        /// </summary>
        public static AutomationElement? FindElementSafe(this AutomationElement element, TreeScope scope, Condition condition)
        {
            try
            {
                return element.FindFirst(scope, condition);
            }
            catch (ElementNotAvailableException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Safely find multiple elements
        /// </summary>
        public static AutomationElementCollection? FindElementsSafe(this AutomationElement element, TreeScope scope, Condition condition)
        {
            try
            {
                return element.FindAll(scope, condition);
            }
            catch (ElementNotAvailableException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Check if element is available and valid
        /// </summary>
        public static bool IsAvailable(this AutomationElement element)
        {
            try
            {
                // Try to access a basic property to check availability
                var _ = element.Current.AutomationId;
                return true;
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get element's basic information safely
        /// </summary>
        public static (string automationId, string name, string controlType) GetBasicInfo(this AutomationElement element)
        {
            try
            {
                var automationId = element.GetPropertyValue<string>(AutomationElement.AutomationIdProperty) ?? "";
                var name = element.GetPropertyValue<string>(AutomationElement.NameProperty) ?? "";
                var controlType = element.GetPropertyValue<ControlType>(AutomationElement.ControlTypeProperty)?.ProgrammaticName ?? "";
                
                return (automationId, name, controlType);
            }
            catch
            {
                return ("", "", "");
            }
        }

        /// <summary>
        /// Check if element supports a specific pattern
        /// </summary>
        public static bool SupportsPattern(this AutomationElement element, AutomationPattern pattern)
        {
            try
            {
                return element.TryGetCurrentPattern(pattern, out _);
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}

