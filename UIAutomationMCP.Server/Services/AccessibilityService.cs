using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public class AccessibilityService : IAccessibilityService
    {
        private readonly ILogger<AccessibilityService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly ElementInfoExtractor _elementInfoExtractor;
        private readonly AutomationHelper _automationHelper;

        public AccessibilityService(
            ILogger<AccessibilityService> logger, 
            UIAutomationExecutor executor, 
            ElementInfoExtractor elementInfoExtractor, 
            AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _elementInfoExtractor = elementInfoExtractor;
            _automationHelper = automationHelper;
        }

        public async Task<object> GetAccessibilityInfoAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting accessibility info for element: {ElementId}", elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        return new { Success = false, Error = "Could not find search root" };
                    }

                    var element = FindElementByIdOrName(elementId, searchRoot);
                    if (element == null)
                    {
                        return new { Success = false, Error = $"Element '{elementId}' not found" };
                    }

                    var accessibilityInfo = new
                    {
                        Success = true,
                        ElementId = elementId,
                        Name = element.Current.Name,
                        LocalizedControlType = element.Current.LocalizedControlType,
                        AccessKey = element.Current.AccessKey,
                        AcceleratorKey = element.Current.AcceleratorKey,
                        HelpText = element.Current.HelpText,
                        IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                        IsEnabled = element.Current.IsEnabled,
                        IsOffscreen = element.Current.IsOffscreen,
                        HasKeyboardFocus = element.Current.HasKeyboardFocus,
                        LabeledBy = GetLabeledByInfo(element),
                        DescribedBy = GetDescribedByInfo(element),
                        AutomationId = element.Current.AutomationId,
                        ClassName = element.Current.ClassName,
                        FrameworkId = element.Current.FrameworkId
                    };

                    return accessibilityInfo;
                }, timeoutSeconds, $"GetAccessibilityInfo_{elementId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accessibility info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> VerifyAccessibilityAsync(string? elementId = null, string? windowTitle = null, int? processId = null, int timeoutSeconds = 60)
        {
            try
            {
                _logger.LogInformation("Verifying accessibility for element: {ElementId}", elementId ?? "window");

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var issues = new List<object>();
                    var warnings = new List<object>();

                    if (!string.IsNullOrEmpty(elementId))
                    {
                        // Verify specific element
                        var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId);
                        if (searchRoot == null)
                        {
                            return new { Success = false, Error = "Could not find search root" };
                        }

                        var element = FindElementByIdOrName(elementId, searchRoot);
                        if (element == null)
                        {
                            return new { Success = false, Error = $"Element '{elementId}' not found" };
                        }

                        VerifyElementAccessibility(element, issues, warnings);
                    }
                    else
                    {
                        // Verify entire window
                        var window = _automationHelper.GetSearchRoot(windowTitle, processId);
                        if (window == null)
                        {
                            return new { Success = false, Error = "Window not found" };
                        }

                        VerifyWindowAccessibility(window, issues, warnings);
                    }

                    return new
                    {
                        Success = true,
                        ElementId = elementId,
                        WindowTitle = windowTitle,
                        Issues = issues,
                        Warnings = warnings,
                        IssueCount = issues.Count,
                        WarningCount = warnings.Count,
                        IsAccessible = issues.Count == 0
                    };
                }, timeoutSeconds, $"VerifyAccessibility_{elementId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying accessibility for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetLabeledByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting labeled by info for element: {ElementId}", elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        return new { Success = false, Error = "Could not find search root" };
                    }

                    var element = FindElementByIdOrName(elementId, searchRoot);
                    if (element == null)
                    {
                        return new { Success = false, Error = $"Element '{elementId}' not found" };
                    }

                    var labeledByInfo = GetLabeledByInfo(element);
                    return new
                    {
                        Success = true,
                        ElementId = elementId,
                        LabeledBy = labeledByInfo
                    };
                }, timeoutSeconds, $"GetLabeledBy_{elementId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting labeled by info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetDescribedByAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting described by info for element: {ElementId}", elementId);

                var result = await _executor.ExecuteAsync<object>(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        return new { Success = false, Error = "Could not find search root" };
                    }

                    var element = FindElementByIdOrName(elementId, searchRoot);
                    if (element == null)
                    {
                        return new { Success = false, Error = $"Element '{elementId}' not found" };
                    }

                    var describedByInfo = GetDescribedByInfo(element);
                    return new
                    {
                        Success = true,
                        ElementId = elementId,
                        DescribedBy = describedByInfo
                    };
                }, timeoutSeconds, $"GetDescribedBy_{elementId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting described by info for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        private AutomationElement? FindElementByIdOrName(string elementId, AutomationElement searchRoot)
        {
            try
            {
                var conditions = new List<Condition>();

                // Name条件
                if (!string.IsNullOrEmpty(elementId))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.NameProperty, elementId));
                }

                // AutomationId条件  
                if (!string.IsNullOrEmpty(elementId))
                {
                    conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty, elementId));
                }

                if (conditions.Count == 0)
                {
                    _logger.LogWarning("No valid element identifier provided");
                    return null;
                }

                var condition = conditions.Count == 1 ? conditions[0] : new OrCondition(conditions.ToArray());
                return searchRoot.FindFirst(TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find element by ID");
                return null;
            }
        }

        private object? GetLabeledByInfo(AutomationElement element)
        {
            try
            {
                var labeledByElement = element.Current.LabeledBy;
                if (labeledByElement != null)
                {
                    return new
                    {
                        Name = labeledByElement.Current.Name,
                        AutomationId = labeledByElement.Current.AutomationId,
                        ControlType = labeledByElement.Current.LocalizedControlType
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get labeled by info for element");
            }
            return null;
        }

        private object? GetDescribedByInfo(AutomationElement element)
        {
            try
            {
                // Described by is typically implemented via custom properties or ARIA-describedby
                // For now, we'll return basic description information
                var helpText = element.Current.HelpText;
                if (!string.IsNullOrEmpty(helpText))
                {
                    return new
                    {
                        HelpText = helpText,
                        Source = "HelpText property"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get described by info for element");
            }
            return null;
        }

        private void VerifyElementAccessibility(AutomationElement element, List<object> issues, List<object> warnings)
        {
            try
            {
                // Check for name
                if (string.IsNullOrEmpty(element.Current.Name))
                {
                    var controlTypeValue = element.Current.LocalizedControlType;
                    if (controlTypeValue != "pane" && controlTypeValue != "window" && controlTypeValue != "group")
                    {
                        issues.Add(new
                        {
                            Type = "MissingName",
                            Message = $"Element of type '{controlTypeValue}' is missing a name",
                            AutomationId = element.Current.AutomationId,
                            ControlType = controlTypeValue
                        });
                    }
                }

                // Check keyboard focusability for interactive elements
                var interactiveTypes = new[] { "button", "edit", "combo box", "list", "tab", "check box", "radio button" };
                var controlType = element.Current.LocalizedControlType?.ToLower();
                if (interactiveTypes.Contains(controlType) && !element.Current.IsKeyboardFocusable)
                {
                    warnings.Add(new
                    {
                        Type = "NotKeyboardFocusable",
                        Message = $"Interactive element '{controlType}' is not keyboard focusable",
                        Name = element.Current.Name,
                        AutomationId = element.Current.AutomationId
                    });
                }

                // Check if element is enabled but offscreen
                if (element.Current.IsEnabled && element.Current.IsOffscreen)
                {
                    warnings.Add(new
                    {
                        Type = "EnabledButOffscreen",
                        Message = "Element is enabled but offscreen",
                        Name = element.Current.Name,
                        AutomationId = element.Current.AutomationId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying element accessibility");
            }
        }

        private void VerifyWindowAccessibility(AutomationElement window, List<object> issues, List<object> warnings)
        {
            try
            {
                // Get all child elements and verify each one
                var condition = Condition.TrueCondition;
                var children = window.FindAll(TreeScope.Descendants, condition);

                foreach (AutomationElement child in children)
                {
                    VerifyElementAccessibility(child, issues, warnings);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying window accessibility");
            }
        }
    }
}
