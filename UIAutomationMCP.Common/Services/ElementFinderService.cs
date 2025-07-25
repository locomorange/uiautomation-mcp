using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Common.Services
{
    /// <summary>
    /// Unified element finding service for UI Automation operations
    /// Shared between Worker and Monitor processes
    /// </summary>
    public class ElementFinderService
    {
        private readonly ILogger<ElementFinderService> _logger;

        public ElementFinderService(ILogger<ElementFinderService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Find element by AutomationId or Name
        /// </summary>
        public AutomationElement? FindElement(
            string? automationId = null, 
            string? name = null, 
            string? controlType = null, 
            string? windowTitle = null, 
            int? processId = null, 
            TreeScope scope = TreeScope.Descendants,
            AutomationPattern? requiredPattern = null)
        {
            // At least one identifier is required
            if (string.IsNullOrEmpty(automationId) && string.IsNullOrEmpty(name))
            {
                _logger.LogWarning("Both AutomationId and Name are null or empty");
                return null;
            }

            try
            {
                AutomationElement rootElement = AutomationElement.RootElement;
                
                // If process ID is specified, find the root window first
                if (processId.HasValue)
                {
                    var processCondition = new PropertyCondition(AutomationElement.ProcessIdProperty, processId.Value);
                    var processWindow = rootElement.FindFirst(TreeScope.Children, processCondition);
                    if (processWindow != null)
                    {
                        rootElement = processWindow;
                        scope = TreeScope.Descendants;
                    }
                }

                // If window title is specified, find the window first
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    var windowCondition = new PropertyCondition(AutomationElement.NameProperty, windowTitle);
                    var window = rootElement.FindFirst(TreeScope.Children, windowCondition);
                    if (window != null)
                    {
                        rootElement = window;
                        scope = TreeScope.Descendants;
                    }
                }

                // Build the search condition
                Condition? condition = null;

                // Primary search by AutomationId
                if (!string.IsNullOrEmpty(automationId))
                {
                    condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
                }
                // Fallback search by Name
                else if (!string.IsNullOrEmpty(name))
                {
                    condition = new PropertyCondition(AutomationElement.NameProperty, name);
                }

                // Add ControlType filter if specified
                if (!string.IsNullOrEmpty(controlType) && condition != null)
                {
                    if (TryGetControlType(controlType, out var controlTypeObj))
                    {
                        var controlTypeCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeObj);
                        condition = new AndCondition(condition, controlTypeCondition);
                    }
                }

                // Note: Pattern availability is checked after finding the element

                if (condition == null)
                {
                    _logger.LogWarning("No valid search condition could be built");
                    return null;
                }

                // Perform the search
                var element = rootElement.FindFirst(scope, condition);
                
                if (element != null)
                {
                    _logger.LogDebug("Found element: AutomationId={AutomationId}, Name={Name}, ControlType={ControlType}",
                        element.Current.AutomationId,
                        element.Current.Name,
                        element.Current.ControlType.ProgrammaticName);
                    
                    // Check if required pattern is supported
                    if (requiredPattern != null)
                    {
                        var supportedPatterns = element.GetSupportedPatterns();
                        if (!supportedPatterns.Contains(requiredPattern))
                        {
                            _logger.LogDebug("Element found but does not support required pattern: {Pattern}",
                                requiredPattern.ProgrammaticName);
                            return null;
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("Element not found with AutomationId={AutomationId}, Name={Name}, ControlType={ControlType}",
                        automationId, name, controlType);
                }

                return element;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element: AutomationId={AutomationId}, Name={Name}", automationId, name);
                return null;
            }
        }


        private bool TryGetControlType(string controlTypeName, out ControlType? controlType)
        {
            controlType = null;

            try
            {
                var field = typeof(ControlType).GetField(controlTypeName);
                if (field?.GetValue(null) is ControlType ct)
                {
                    controlType = ct;
                    return true;
                }

                // Try with "ControlType" suffix
                field = typeof(ControlType).GetField(controlTypeName + "ControlType");
                if (field?.GetValue(null) is ControlType ct2)
                {
                    controlType = ct2;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve ControlType: {ControlTypeName}", controlTypeName);
                return false;
            }
        }
    }
}