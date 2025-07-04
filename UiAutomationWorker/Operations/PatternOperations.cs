using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UiAutomationWorker.Operations
{
    public class PatternOperations
    {
        private readonly ILogger<PatternOperations> _logger;

        public PatternOperations(ILogger<PatternOperations> logger)
        {
            _logger = logger;
        }

        public async Task<object> InvokeAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Executing Invoke pattern for element '{ElementId}' in window '{WindowTitle}'", elementId, windowTitle);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var invokePattern = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                if (invokePattern == null)
                {
                    return new { Success = false, Error = "InvokePattern not supported by this element" };
                }

                invokePattern.Invoke();
                _logger.LogInformation("Successfully invoked element '{ElementId}'", elementId);
                
                return new { Success = true, Message = "Element invoked successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Invoke pattern");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> ToggleAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Executing Toggle pattern for element '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var togglePattern = element.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
                if (togglePattern == null)
                {
                    return new { Success = false, Error = "TogglePattern not supported by this element" };
                }

                var currentState = togglePattern.Current.ToggleState;
                togglePattern.Toggle();
                var newState = togglePattern.Current.ToggleState;
                
                _logger.LogInformation("Toggled element '{ElementId}' from {OldState} to {NewState}", elementId, currentState, newState);
                
                return new { 
                    Success = true, 
                    PreviousState = currentState.ToString(),
                    CurrentState = newState.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Toggle pattern");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetValueAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var value = parameters["Value"]?.ToString();
                
                _logger.LogInformation("Setting value '{Value}' for element '{ElementId}'", value, elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var valuePattern = element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                if (valuePattern == null)
                {
                    return new { Success = false, Error = "ValuePattern not supported by this element" };
                }

                if (valuePattern.Current.IsReadOnly)
                {
                    return new { Success = false, Error = "Element is read-only" };
                }

                valuePattern.SetValue(value ?? "");
                _logger.LogInformation("Successfully set value for element '{ElementId}'", elementId);
                
                return new { Success = true, Value = value };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SelectItemAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Selecting item '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var selectionItemPattern = element.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                if (selectionItemPattern == null)
                {
                    return new { Success = false, Error = "SelectionItemPattern not supported by this element" };
                }

                selectionItemPattern.Select();
                _logger.LogInformation("Successfully selected item '{ElementId}'", elementId);
                
                return new { Success = true, Selected = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting item");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetWindowStateAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var stateString = parameters["State"]?.ToString();
                
                if (!Enum.TryParse<WindowVisualState>(stateString, true, out var state))
                {
                    return new { Success = false, Error = $"Invalid window state: {stateString}" };
                }
                
                _logger.LogInformation("Setting window state to '{State}' for element '{ElementId}'", state, elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var windowPattern = element.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
                if (windowPattern == null)
                {
                    return new { Success = false, Error = "WindowPattern not supported by this element" };
                }

                windowPattern.SetWindowVisualState(state);
                _logger.LogInformation("Successfully set window state to '{State}'", state);
                
                return new { Success = true, State = state.ToString() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting window state");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetValueAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Getting value for element '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var valuePattern = element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                if (valuePattern == null)
                {
                    return new { Success = false, Error = "ValuePattern not supported by this element" };
                }

                var value = valuePattern.Current.Value;
                _logger.LogInformation("Retrieved value '{Value}' for element '{ElementId}'", value, elementId);
                
                return new { Success = true, Value = value };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SetRangeValueAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var value = Convert.ToDouble(parameters["Value"]);
                
                _logger.LogInformation("Setting range value '{Value}' for element '{ElementId}'", value, elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var rangeValuePattern = element.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
                if (rangeValuePattern == null)
                {
                    return new { Success = false, Error = "RangeValuePattern not supported by this element" };
                }

                if (rangeValuePattern.Current.IsReadOnly)
                {
                    return new { Success = false, Error = "Element is read-only" };
                }

                rangeValuePattern.SetValue(value);
                _logger.LogInformation("Successfully set range value for element '{ElementId}'", elementId);
                
                return new { Success = true, Value = value };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting range value");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetRangeValueAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Getting range value for element '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var rangeValuePattern = element.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
                if (rangeValuePattern == null)
                {
                    return new { Success = false, Error = "RangeValuePattern not supported by this element" };
                }

                var current = rangeValuePattern.Current;
                _logger.LogInformation("Retrieved range value for element '{ElementId}'", elementId);
                
                return new { 
                    Success = true, 
                    Value = current.Value,
                    Minimum = current.Minimum,
                    Maximum = current.Maximum,
                    SmallChange = current.SmallChange,
                    LargeChange = current.LargeChange,
                    IsReadOnly = current.IsReadOnly
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting range value");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetTextAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                
                _logger.LogInformation("Getting text for element '{ElementId}'", elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var textPattern = element.GetCurrentPattern(TextPattern.Pattern) as TextPattern;
                if (textPattern == null)
                {
                    return new { Success = false, Error = "TextPattern not supported by this element" };
                }

                var documentRange = textPattern.DocumentRange;
                var text = documentRange.GetText(-1);
                
                _logger.LogInformation("Retrieved text from element '{ElementId}'", elementId);
                
                return new { Success = true, Text = text };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting text");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> SelectTextAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var startIndex = Convert.ToInt32(parameters["StartIndex"]);
                var length = Convert.ToInt32(parameters["Length"]);
                
                _logger.LogInformation("Selecting text in element '{ElementId}' from {StartIndex}, length {Length}", elementId, startIndex, length);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var textPattern = element.GetCurrentPattern(TextPattern.Pattern) as TextPattern;
                if (textPattern == null)
                {
                    return new { Success = false, Error = "TextPattern not supported by this element" };
                }

                var documentRange = textPattern.DocumentRange;
                var textRange = documentRange.GetText(-1);
                
                if (startIndex < 0 || startIndex >= textRange.Length || startIndex + length > textRange.Length)
                {
                    return new { Success = false, Error = "Invalid text range" };
                }

                var range = documentRange.Clone();
                range.MoveEndpointByUnit(System.Windows.Automation.Text.TextPatternRangeEndpoint.Start, 
                                       System.Windows.Automation.Text.TextUnit.Character, startIndex);
                range.MoveEndpointByUnit(System.Windows.Automation.Text.TextPatternRangeEndpoint.End, 
                                       System.Windows.Automation.Text.TextUnit.Character, startIndex + length - textRange.Length);
                range.Select();
                
                _logger.LogInformation("Successfully selected text in element '{ElementId}'", elementId);
                
                return new { Success = true, StartIndex = startIndex, Length = length };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting text");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> ExpandCollapseAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var expand = parameters.ContainsKey("Expand") ? Convert.ToBoolean(parameters["Expand"]) : (bool?)null;
                
                _logger.LogInformation("Executing ExpandCollapse for element '{ElementId}', expand: {Expand}", elementId, expand);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var expandCollapsePattern = element.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                if (expandCollapsePattern == null)
                {
                    return new { Success = false, Error = "ExpandCollapsePattern not supported by this element" };
                }

                var currentState = expandCollapsePattern.Current.ExpandCollapseState;
                
                if (expand.HasValue)
                {
                    if (expand.Value)
                    {
                        expandCollapsePattern.Expand();
                    }
                    else
                    {
                        expandCollapsePattern.Collapse();
                    }
                }
                else
                {
                    // Toggle
                    if (currentState == ExpandCollapseState.Expanded)
                    {
                        expandCollapsePattern.Collapse();
                    }
                    else
                    {
                        expandCollapsePattern.Expand();
                    }
                }

                var newState = expandCollapsePattern.Current.ExpandCollapseState;
                _logger.LogInformation("ExpandCollapse completed for element '{ElementId}', state: {OldState} -> {NewState}", elementId, currentState, newState);
                
                return new { 
                    Success = true, 
                    PreviousState = currentState.ToString(),
                    CurrentState = newState.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExpandCollapse");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> TransformAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var action = parameters["Action"]?.ToString()?.ToLowerInvariant();
                
                _logger.LogInformation("Executing Transform action '{Action}' for element '{ElementId}'", action, elementId);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var transformPattern = element.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                if (transformPattern == null)
                {
                    return new { Success = false, Error = "TransformPattern not supported by this element" };
                }

                switch (action)
                {
                    case "move":
                        var x = Convert.ToDouble(parameters["X"]);
                        var y = Convert.ToDouble(parameters["Y"]);
                        if (transformPattern.Current.CanMove)
                        {
                            transformPattern.Move(x, y);
                            return new { Success = true, Action = "move", X = x, Y = y };
                        }
                        return new { Success = false, Error = "Element cannot be moved" };

                    case "resize":
                        var width = Convert.ToDouble(parameters["Width"]);
                        var height = Convert.ToDouble(parameters["Height"]);
                        if (transformPattern.Current.CanResize)
                        {
                            transformPattern.Resize(width, height);
                            return new { Success = true, Action = "resize", Width = width, Height = height };
                        }
                        return new { Success = false, Error = "Element cannot be resized" };

                    case "rotate":
                        var degrees = Convert.ToDouble(parameters["Degrees"]);
                        if (transformPattern.Current.CanRotate)
                        {
                            transformPattern.Rotate(degrees);
                            return new { Success = true, Action = "rotate", Degrees = degrees };
                        }
                        return new { Success = false, Error = "Element cannot be rotated" };

                    default:
                        return new { Success = false, Error = $"Unknown transform action: {action}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Transform");
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> DockAsync(Dictionary<string, object> parameters)
        {
            try
            {
                var elementId = parameters["ElementId"]?.ToString();
                var windowTitle = parameters["WindowTitle"]?.ToString();
                var positionString = parameters["Position"]?.ToString();
                
                if (!Enum.TryParse<DockPosition>(positionString, true, out var position))
                {
                    return new { Success = false, Error = $"Invalid dock position: {positionString}" };
                }
                
                _logger.LogInformation("Docking element '{ElementId}' to position '{Position}'", elementId, position);

                var element = await FindElementAsync(elementId, windowTitle);
                if (element == null)
                {
                    return new { Success = false, Error = "Element not found" };
                }

                var dockPattern = element.GetCurrentPattern(DockPattern.Pattern) as DockPattern;
                if (dockPattern == null)
                {
                    return new { Success = false, Error = "DockPattern not supported by this element" };
                }

                dockPattern.SetDockPosition(position);
                _logger.LogInformation("Successfully docked element '{ElementId}' to position '{Position}'", elementId, position);
                
                return new { Success = true, Position = position.ToString() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error docking element");
                return new { Success = false, Error = ex.Message };
            }
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
                        _logger.LogWarning("Window '{WindowTitle}' not found", windowTitle);
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
    }
}
