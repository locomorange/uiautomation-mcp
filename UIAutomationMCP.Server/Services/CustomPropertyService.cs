using System;
using System.Linq;
using System.Threading.Tasks;
using UIAutomationMCP.Server.Helpers;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace UIAutomationMCP.Server.Services
{
    public class CustomPropertyService : ICustomPropertyService
    {
        private readonly ILogger<CustomPropertyService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly ElementInfoExtractor _elementInfoExtractor;
        private readonly AutomationHelper _automationHelper;

        public CustomPropertyService(ILogger<CustomPropertyService> logger, UIAutomationExecutor executor, ElementInfoExtractor elementInfoExtractor, AutomationHelper automationHelper)
        {
            _logger = logger;
            _executor = executor;
            _elementInfoExtractor = elementInfoExtractor;
            _automationHelper = automationHelper;
        }

        public async Task<object> GetCustomPropertiesAsync(string elementId, string[] propertyIds, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting custom properties for element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var properties = await _executor.ExecuteAsync(() =>
                {
                    var propertyValues = propertyIds.Select(propId => new
                    {
                        propertyId = propId,
                        value = GetCustomPropertyValue(element, propId)
                    }).ToArray();

                    return new
                    {
                        elementId,
                        properties = propertyValues,
                        timestamp = DateTime.UtcNow
                    };
                }, timeoutSeconds, $"GetCustomProperties_{elementId}");

                return properties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get custom properties for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetCustomPropertyAsync(string elementId, string propertyId, object value, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Setting custom property for element: {ElementId}, Property: {PropertyId}", elementId, propertyId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var result = await _executor.ExecuteAsync(() =>
                {
                    var success = SetCustomPropertyValue(element, propertyId, value);

                    return new
                    {
                        elementId,
                        propertyId,
                        value,
                        success,
                        timestamp = DateTime.UtcNow
                    };
                }, timeoutSeconds, $"SetCustomProperty_{propertyId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set custom property {PropertyId} for element {ElementId}", propertyId, elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetAllPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting all properties for element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var allProperties = await _executor.ExecuteAsync(() =>
                {
                    var properties = GetAllAvailableProperties(element);

                    return new
                    {
                        elementId,
                        standardProperties = properties.StandardProperties,
                        patternProperties = properties.PatternProperties,
                        customProperties = properties.CustomProperties,
                        timestamp = DateTime.UtcNow
                    };
                }, timeoutSeconds, $"GetAllProperties_{elementId}");

                return allProperties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all properties for element {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        private object? GetCustomPropertyValue(AutomationElement element, string propertyId)
        {
            try
            {
                // Try to get standard properties first
                var standardProperty = GetStandardProperty(propertyId);
                if (standardProperty != null)
                {
                    return element.GetCurrentPropertyValue(standardProperty);
                }

                // Try to map custom property IDs to known properties
                return GetMappedPropertyValue(element, propertyId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get property {PropertyId}", propertyId);
                return null;
            }
        }

        private bool SetCustomPropertyValue(AutomationElement element, string propertyId, object value)
        {
            try
            {
                // Note: Most properties in UI Automation are read-only
                // This method primarily serves as a placeholder for potential future functionality
                // or for setting values through patterns rather than direct property manipulation

                // For writable properties, we might use patterns instead
                switch (propertyId.ToLower())
                {
                    case "value":
                        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) &&
                            valuePattern is ValuePattern valuePatternInstance &&
                            valuePatternInstance.Current.IsReadOnly == false)
                        {
                            valuePatternInstance.SetValue(value?.ToString() ?? "");
                            return true;
                        }
                        break;

                    case "toggle":
                        if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) &&
                            togglePattern is TogglePattern togglePatternInstance)
                        {
                            togglePatternInstance.Toggle();
                            return true;
                        }
                        break;

                    case "selection":
                        if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionPattern) &&
                            selectionPattern is SelectionItemPattern selectionPatternInstance)
                        {
                            if (value is bool boolValue && boolValue)
                            {
                                selectionPatternInstance.Select();
                                return true;
                            }
                        }
                        break;
                }

                _logger.LogWarning("Property {PropertyId} is read-only or not supported for modification", propertyId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set property {PropertyId} to {Value}", propertyId, value);
                return false;
            }
        }

        private AutomationProperty? GetStandardProperty(string propertyId)
        {
            // Map string property IDs to AutomationProperty objects
            return propertyId.ToLower() switch
            {
                "name" => AutomationElement.NameProperty,
                "automationid" => AutomationElement.AutomationIdProperty,
                "classname" => AutomationElement.ClassNameProperty,
                "controltype" => AutomationElement.ControlTypeProperty,
                "localizedcontroltype" => AutomationElement.LocalizedControlTypeProperty,
                "helptext" => AutomationElement.HelpTextProperty,
                "acceleratorkey" => AutomationElement.AcceleratorKeyProperty,
                "accesskey" => AutomationElement.AccessKeyProperty,
                "isenabled" => AutomationElement.IsEnabledProperty,
                "isoffscreen" => AutomationElement.IsOffscreenProperty,
                "iskeyboardfocusable" => AutomationElement.IsKeyboardFocusableProperty,
                "haskeyboardfocus" => AutomationElement.HasKeyboardFocusProperty,
                "ispassword" => AutomationElement.IsPasswordProperty,
                "isrequiredforform" => AutomationElement.IsRequiredForFormProperty,
                "itemtype" => AutomationElement.ItemTypeProperty,
                "itemstatus" => AutomationElement.ItemStatusProperty,
                "boundingrectangle" => AutomationElement.BoundingRectangleProperty,
                "processid" => AutomationElement.ProcessIdProperty,
                "runtimeid" => AutomationElement.RuntimeIdProperty,
                "frameworkid" => AutomationElement.FrameworkIdProperty,
                "iscontentlement" => AutomationElement.IsContentElementProperty,
                "iscontrollement" => AutomationElement.IsControlElementProperty,
                "orientation" => AutomationElement.OrientationProperty,
                _ => null
            };
        }

        private object? GetMappedPropertyValue(AutomationElement element, string propertyId)
        {
            // Map custom property names to specific pattern properties or computed values
            return propertyId.ToLower() switch
            {
                "value" => GetElementValue(element),
                "isselected" => GetSelectionState(element),
                "togglestate" => GetToggleState(element),
                "expandcollapsestate" => GetExpandCollapseState(element),
                "rangevalue" => GetRangeValue(element),
                "gridposition" => GetGridPosition(element),
                "tableposition" => GetTablePosition(element),
                _ => null
            };
        }

        private object? GetElementValue(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern) &&
                pattern is ValuePattern valuePattern)
            {
                return valuePattern.Current.Value;
            }
            return null;
        }

        private object? GetSelectionState(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern) &&
                pattern is SelectionItemPattern selectionPattern)
            {
                return selectionPattern.Current.IsSelected;
            }
            return null;
        }

        private object? GetToggleState(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern) &&
                pattern is TogglePattern togglePattern)
            {
                return togglePattern.Current.ToggleState.ToString();
            }
            return null;
        }

        private object? GetExpandCollapseState(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var pattern) &&
                pattern is ExpandCollapsePattern expandPattern)
            {
                return expandPattern.Current.ExpandCollapseState.ToString();
            }
            return null;
        }

        private object? GetRangeValue(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var pattern) &&
                pattern is RangeValuePattern rangePattern)
            {
                return new
                {
                    value = rangePattern.Current.Value,
                    minimum = rangePattern.Current.Minimum,
                    maximum = rangePattern.Current.Maximum,
                    smallChange = rangePattern.Current.SmallChange,
                    largeChange = rangePattern.Current.LargeChange
                };
            }
            return null;
        }

        private object? GetGridPosition(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(GridItemPattern.Pattern, out var pattern) &&
                pattern is GridItemPattern gridItemPattern)
            {
                return new
                {
                    row = gridItemPattern.Current.Row,
                    column = gridItemPattern.Current.Column,
                    rowSpan = gridItemPattern.Current.RowSpan,
                    columnSpan = gridItemPattern.Current.ColumnSpan
                };
            }
            return null;
        }

        private object? GetTablePosition(AutomationElement element)
        {
            if (element.TryGetCurrentPattern(TableItemPattern.Pattern, out var pattern) &&
                pattern is TableItemPattern tableItemPattern)
            {
                return new
                {
                    row = tableItemPattern.Current.Row,
                    column = tableItemPattern.Current.Column,
                    rowHeaderItems = tableItemPattern.Current.GetRowHeaderItems()?.Select(item => item.Current.Name).ToArray(),
                    columnHeaderItems = tableItemPattern.Current.GetColumnHeaderItems()?.Select(item => item.Current.Name).ToArray()
                };
            }
            return null;
        }

        private (object StandardProperties, object PatternProperties, object CustomProperties) GetAllAvailableProperties(AutomationElement element)
        {
            // Get standard properties
            var standardProps = new Dictionary<string, object?>();
            var standardPropertyNames = new[]
            {
                "name", "automationid", "classname", "controltype", "localizedcontroltype",
                "helptext", "acceleratorkey", "accesskey", "isenabled", "isoffscreen",
                "iskeyboardfocusable", "haskeyboardfocus", "ispassword", "isrequiredforform",
                "itemtype", "itemstatus", "boundingrectangle", "processid", "orientation"
            };

            foreach (var propName in standardPropertyNames)
            {
                standardProps[propName] = GetCustomPropertyValue(element, propName);
            }

            // Get pattern-specific properties
            var patternProps = new Dictionary<string, object?>();
            var patterns = element.GetSupportedPatterns();

            foreach (var pattern in patterns)
            {
                var patternName = pattern.ProgrammaticName.Replace("PatternIdentifiers.Pattern", "");
                patternProps[patternName] = GetPatternSpecificProperties(element, pattern);
            }

            // Get custom mapped properties
            var customProps = new Dictionary<string, object?>();
            var customPropertyNames = new[]
            {
                "value", "isselected", "togglestate", "expandcollapsestate",
                "rangevalue", "gridposition", "tableposition"
            };

            foreach (var propName in customPropertyNames)
            {
                var value = GetMappedPropertyValue(element, propName);
                if (value != null)
                {
                    customProps[propName] = value;
                }
            }

            return (standardProps, patternProps, customProps);
        }

        private object? GetPatternSpecificProperties(AutomationElement element, AutomationPattern pattern)
        {
            try
            {
                if (element.TryGetCurrentPattern(pattern, out var patternInstance))
                {
                    // Return pattern-specific information based on pattern type
                    switch (pattern.ProgrammaticName)
                    {
                        case "ValuePatternIdentifiers.Pattern":
                            if (patternInstance is ValuePattern valuePattern)
                                return new { Value = valuePattern.Current.Value, IsReadOnly = valuePattern.Current.IsReadOnly };
                            break;

                        case "SelectionItemPatternIdentifiers.Pattern":
                            if (patternInstance is SelectionItemPattern selectionPattern)
                                return new { IsSelected = selectionPattern.Current.IsSelected };
                            break;

                        case "TogglePatternIdentifiers.Pattern":
                            if (patternInstance is TogglePattern togglePattern)
                                return new { ToggleState = togglePattern.Current.ToggleState.ToString() };
                            break;

                        case "ExpandCollapsePatternIdentifiers.Pattern":
                            if (patternInstance is ExpandCollapsePattern expandPattern)
                                return new { ExpandCollapseState = expandPattern.Current.ExpandCollapseState.ToString() };
                            break;

                        case "RangeValuePatternIdentifiers.Pattern":
                            if (patternInstance is RangeValuePattern rangePattern)
                                return new
                                {
                                    Value = rangePattern.Current.Value,
                                    Minimum = rangePattern.Current.Minimum,
                                    Maximum = rangePattern.Current.Maximum,
                                    SmallChange = rangePattern.Current.SmallChange,
                                    LargeChange = rangePattern.Current.LargeChange,
                                    IsReadOnly = rangePattern.Current.IsReadOnly
                                };
                            break;

                        default:
                            return new { PatternSupported = true };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get properties for pattern {PatternName}", pattern.ProgrammaticName);
            }

            return null;
        }
    }
}
