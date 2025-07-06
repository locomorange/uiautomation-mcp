using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Helpers;

namespace UIAutomationMCP.Server.Services
{
    public interface IElementInspectionService
    {
        Task<object> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
        Task<object> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30);
    }

    public class ElementInspectionService : IElementInspectionService
    {
        private readonly ILogger<ElementInspectionService> _logger;
        private readonly UIAutomationExecutor _executor;
        private readonly AutomationHelper _automationHelper;
        private readonly ElementInfoExtractor _elementInfoExtractor;

        public ElementInspectionService(
            ILogger<ElementInspectionService> logger, 
            UIAutomationExecutor executor, 
            AutomationHelper automationHelper, 
            ElementInfoExtractor elementInfoExtractor)
        {
            _logger = logger;
            _executor = executor;
            _automationHelper = automationHelper;
            _elementInfoExtractor = elementInfoExtractor;
        }

        public async Task<object> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting properties for element: {ElementId}", elementId);

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
                    return _elementInfoExtractor.ExtractElementInfo(element);
                }, timeoutSeconds, $"GetProperties_{elementId}");

                _logger.LogInformation("Properties retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = properties };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get properties for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        public async Task<object> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null, int timeoutSeconds = 30)
        {
            try
            {
                _logger.LogInformation("Getting patterns for element: {ElementId}", elementId);

                var element = await _executor.ExecuteAsync(() =>
                {
                    var searchRoot = _automationHelper.GetSearchRoot(windowTitle, processId) ?? AutomationElement.RootElement;
                    return _automationHelper.FindElementById(elementId, searchRoot);
                }, timeoutSeconds, $"FindElement_{elementId}");

                if (element == null)
                {
                    return new { Success = false, Error = $"Element '{elementId}' not found" };
                }

                var patterns = await _executor.ExecuteAsync(() =>
                {
                    var supportedPatterns = element.GetSupportedPatterns();
                    var patternList = new List<object>();

                    foreach (var pattern in supportedPatterns)
                    {
                        var patternInfo = new
                        {
                            PatternName = pattern.ProgrammaticName,
                            PatternId = pattern.Id,
                            Properties = GetPatternProperties(element, pattern)
                        };
                        patternList.Add(patternInfo);
                    }

                    return patternList;
                }, timeoutSeconds, $"GetPatterns_{elementId}");

                _logger.LogInformation("Patterns retrieved successfully for element: {ElementId}", elementId);
                return new { Success = true, Data = patterns };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get patterns for element: {ElementId}", elementId);
                return new { Success = false, Error = ex.Message };
            }
        }

        private object GetPatternProperties(AutomationElement element, AutomationPattern pattern)
        {
            try
            {
                if (pattern == InvokePattern.Pattern)
                {
                    return new { PatternType = "InvokePattern", CanInvoke = true };
                }
                else if (pattern == ValuePattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && valuePattern is ValuePattern vp)
                    {
                        return new
                        {
                            PatternType = "ValuePattern",
                            Value = vp.Current.Value,
                            IsReadOnly = vp.Current.IsReadOnly
                        };
                    }
                }
                else if (pattern == TogglePattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePattern) && togglePattern is TogglePattern tp)
                    {
                        return new
                        {
                            PatternType = "TogglePattern",
                            ToggleState = tp.Current.ToggleState.ToString()
                        };
                    }
                }
                else if (pattern == SelectionItemPattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var selectionItemPattern) && selectionItemPattern is SelectionItemPattern sip)
                    {
                        return new
                        {
                            PatternType = "SelectionItemPattern",
                            IsSelected = sip.Current.IsSelected
                        };
                    }
                }
                else if (pattern == SelectionPattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPattern) && selectionPattern is SelectionPattern sp)
                    {
                        return new
                        {
                            PatternType = "SelectionPattern",
                            CanSelectMultiple = sp.Current.CanSelectMultiple,
                            IsSelectionRequired = sp.Current.IsSelectionRequired
                        };
                    }
                }
                else if (pattern == ExpandCollapsePattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandCollapsePattern) && expandCollapsePattern is ExpandCollapsePattern ecp)
                    {
                        return new
                        {
                            PatternType = "ExpandCollapsePattern",
                            ExpandCollapseState = ecp.Current.ExpandCollapseState.ToString()
                        };
                    }
                }
                else if (pattern == ScrollPattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var scrollPattern) && scrollPattern is ScrollPattern scrp)
                    {
                        return new
                        {
                            PatternType = "ScrollPattern",
                            HorizontalScrollPercent = scrp.Current.HorizontalScrollPercent,
                            VerticalScrollPercent = scrp.Current.VerticalScrollPercent,
                            HorizontallyScrollable = scrp.Current.HorizontallyScrollable,
                            VerticallyScrollable = scrp.Current.VerticallyScrollable
                        };
                    }
                }
                else if (pattern == RangeValuePattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var rangeValuePattern) && rangeValuePattern is RangeValuePattern rvp)
                    {
                        return new
                        {
                            PatternType = "RangeValuePattern",
                            Value = rvp.Current.Value,
                            Minimum = rvp.Current.Minimum,
                            Maximum = rvp.Current.Maximum,
                            SmallChange = rvp.Current.SmallChange,
                            LargeChange = rvp.Current.LargeChange,
                            IsReadOnly = rvp.Current.IsReadOnly
                        };
                    }
                }
                else if (pattern == WindowPattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var windowPattern) && windowPattern is WindowPattern wp)
                    {
                        return new
                        {
                            PatternType = "WindowPattern",
                            WindowVisualState = wp.Current.WindowVisualState.ToString(),
                            WindowInteractionState = wp.Current.WindowInteractionState.ToString(),
                            IsModal = wp.Current.IsModal,
                            IsTopmost = wp.Current.IsTopmost,
                            CanMaximize = wp.Current.CanMaximize,
                            CanMinimize = wp.Current.CanMinimize
                        };
                    }
                }
                else if (pattern == TextPattern.Pattern)
                {
                    return new
                    {
                        PatternType = "TextPattern",
                        SupportsTextSelection = true
                    };
                }
                else if (pattern == TransformPattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var transformPattern) && transformPattern is TransformPattern tfp)
                    {
                        return new
                        {
                            PatternType = "TransformPattern",
                            CanMove = tfp.Current.CanMove,
                            CanResize = tfp.Current.CanResize,
                            CanRotate = tfp.Current.CanRotate
                        };
                    }
                }
                else if (pattern == DockPattern.Pattern)
                {
                    if (element.TryGetCurrentPattern(DockPattern.Pattern, out var dockPattern) && dockPattern is DockPattern dp)
                    {
                        return new
                        {
                            PatternType = "DockPattern",
                            DockPosition = dp.Current.DockPosition.ToString()
                        };
                    }
                }

                return new { PatternType = pattern.ProgrammaticName, Properties = "No specific properties available" };
            }
            catch (Exception)
            {
                return new { PatternType = pattern.ProgrammaticName, Properties = "Error retrieving properties" };
            }
        }
    }
}