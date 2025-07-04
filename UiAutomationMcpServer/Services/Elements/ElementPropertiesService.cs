using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Services.Elements
{
    public interface IElementPropertiesService
    {
        Task<OperationResult> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null);
        Task<OperationResult> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null);
    }

    public class ElementPropertiesService : IElementPropertiesService
    {
        private readonly ILogger<ElementPropertiesService> _logger;
        private readonly IWindowService _windowService;
        private readonly IElementUtilityService _elementUtilityService;
        private readonly IUIAutomationHelper _uiAutomationHelper;

        public ElementPropertiesService(ILogger<ElementPropertiesService> logger, IWindowService windowService, IElementUtilityService elementUtilityService, IUIAutomationHelper uiAutomationHelper)
        {
            _logger = logger;
            _windowService = windowService;
            _elementUtilityService = elementUtilityService;
            _uiAutomationHelper = uiAutomationHelper;
        }

        public async Task<OperationResult> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                var availableActions = _elementUtilityService.GetAvailableActions(element);
                
                var properties = new
                {
                    Name = element.Current.Name ?? "",
                    AutomationId = element.Current.AutomationId ?? "",
                    ControlType = element.Current.ControlType.ProgrammaticName ?? "",
                    LocalizedControlType = element.Current.LocalizedControlType ?? "",
                    ClassName = element.Current.ClassName ?? "",
                    IsEnabled = element.Current.IsEnabled,
                    IsVisible = !element.Current.IsOffscreen,
                    IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                    HasKeyboardFocus = element.Current.HasKeyboardFocus,
                    IsPassword = element.Current.IsPassword,
                    IsContentElement = element.Current.IsContentElement,
                    IsControlElement = element.Current.IsControlElement,
                    AcceleratorKey = element.Current.AcceleratorKey ?? "",
                    AccessKey = element.Current.AccessKey ?? "",
                    ProcessId = element.Current.ProcessId,
                    RuntimeId = element.GetRuntimeId(),
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = double.IsInfinity(element.Current.BoundingRectangle.X) ? 0 : element.Current.BoundingRectangle.X,
                        Y = double.IsInfinity(element.Current.BoundingRectangle.Y) ? 0 : element.Current.BoundingRectangle.Y,
                        Width = double.IsInfinity(element.Current.BoundingRectangle.Width) ? 0 : element.Current.BoundingRectangle.Width,
                        Height = double.IsInfinity(element.Current.BoundingRectangle.Height) ? 0 : element.Current.BoundingRectangle.Height
                    },
                    HelpText = element.Current.HelpText ?? "",
                    ItemStatus = element.Current.ItemStatus ?? "",
                    ItemType = element.Current.ItemType ?? "",
                    LabeledBy = element.Current.LabeledBy?.Current.Name ?? "",
                    Orientation = element.Current.Orientation.ToString(),
                    AvailableActions = availableActions
                };

                return new OperationResult { Success = true, Data = properties };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element properties for {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<OperationResult> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var elementResult = await FindElementAsync(elementId, windowTitle, processId);
                if (!elementResult.Success || elementResult.Data == null)
                {
                    return new OperationResult { Success = false, Error = elementResult.Error ?? $"Element '{elementId}' not found" };
                }
                var element = elementResult.Data;

                var supportedPatterns = new List<string>();
                var patternDetails = new Dictionary<string, object>();

                // Check all common patterns
                var patternsToCheck = new[]
                {
                    (InvokePattern.Pattern, "Invoke"),
                    (ValuePattern.Pattern, "Value"),
                    (TogglePattern.Pattern, "Toggle"),
                    (SelectionItemPattern.Pattern, "SelectionItem"),
                    (ExpandCollapsePattern.Pattern, "ExpandCollapse"),
                    (ScrollPattern.Pattern, "Scroll"),
                    (ScrollItemPattern.Pattern, "ScrollItem"),
                    (RangeValuePattern.Pattern, "RangeValue"),
                    (WindowPattern.Pattern, "Window"),
                    (TransformPattern.Pattern, "Transform"),
                    (DockPattern.Pattern, "Dock"),
                    (TextPattern.Pattern, "Text"),
                    (GridPattern.Pattern, "Grid"),
                    (GridItemPattern.Pattern, "GridItem"),
                    (TablePattern.Pattern, "Table"),
                    (TableItemPattern.Pattern, "TableItem"),
                    (SelectionPattern.Pattern, "Selection"),
                    (MultipleViewPattern.Pattern, "MultipleView"),
                    (VirtualizedItemPattern.Pattern, "VirtualizedItem"),
                    (ItemContainerPattern.Pattern, "ItemContainer"),
                    (SynchronizedInputPattern.Pattern, "SynchronizedInput")
                };

                foreach (var (pattern, name) in patternsToCheck)
                {
                    try
                    {
                        if (element.TryGetCurrentPattern(pattern, out var patternObj))
                        {
                            supportedPatterns.Add(name);
                            patternDetails[name] = GetPatternDetails(patternObj, name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking pattern {PatternName}", name);
                    }
                }

                var result = new
                {
                    SupportedPatterns = supportedPatterns,
                    PatternDetails = patternDetails
                };

                return new OperationResult { Success = true, Data = result };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element patterns for {ElementId}", elementId);
                return new OperationResult { Success = false, Error = ex.Message };
            }
        }

        private object GetPatternDetails(object patternObj, string patternName)
        {
            try
            {
                return patternName switch
                {
                    "Value" when patternObj is ValuePattern vp => new
                    {
                        Value = vp.Current.Value,
                        IsReadOnly = vp.Current.IsReadOnly
                    },
                    "Toggle" when patternObj is TogglePattern tp => new
                    {
                        ToggleState = tp.Current.ToggleState.ToString()
                    },
                    "RangeValue" when patternObj is RangeValuePattern rvp => new
                    {
                        Value = double.IsInfinity(rvp.Current.Value) ? 0 : rvp.Current.Value,
                        Minimum = double.IsInfinity(rvp.Current.Minimum) ? 0 : rvp.Current.Minimum,
                        Maximum = double.IsInfinity(rvp.Current.Maximum) ? 0 : rvp.Current.Maximum,
                        SmallChange = double.IsInfinity(rvp.Current.SmallChange) ? 0 : rvp.Current.SmallChange,
                        LargeChange = double.IsInfinity(rvp.Current.LargeChange) ? 0 : rvp.Current.LargeChange,
                        IsReadOnly = rvp.Current.IsReadOnly
                    },
                    "ExpandCollapse" when patternObj is ExpandCollapsePattern ecp => new
                    {
                        ExpandCollapseState = ecp.Current.ExpandCollapseState.ToString()
                    },
                    "SelectionItem" when patternObj is SelectionItemPattern sip => new
                    {
                        IsSelected = sip.Current.IsSelected,
                        SelectionContainer = sip.Current.SelectionContainer?.Current.Name ?? ""
                    },
                    "Window" when patternObj is WindowPattern wp => new
                    {
                        CanMaximize = wp.Current.CanMaximize,
                        CanMinimize = wp.Current.CanMinimize,
                        IsModal = wp.Current.IsModal,
                        IsTopmost = wp.Current.IsTopmost,
                        WindowVisualState = wp.Current.WindowVisualState.ToString(),
                        WindowInteractionState = wp.Current.WindowInteractionState.ToString()
                    },
                    "Transform" when patternObj is TransformPattern tp => new
                    {
                        CanMove = tp.Current.CanMove,
                        CanResize = tp.Current.CanResize,
                        CanRotate = tp.Current.CanRotate
                    },
                    "Dock" when patternObj is DockPattern dp => new
                    {
                        DockPosition = dp.Current.DockPosition.ToString()
                    },
                    "Scroll" when patternObj is ScrollPattern sp => new
                    {
                        HorizontalScrollPercent = double.IsInfinity(sp.Current.HorizontalScrollPercent) ? 0 : sp.Current.HorizontalScrollPercent,
                        VerticalScrollPercent = double.IsInfinity(sp.Current.VerticalScrollPercent) ? 0 : sp.Current.VerticalScrollPercent,
                        HorizontalViewSize = double.IsInfinity(sp.Current.HorizontalViewSize) ? 0 : sp.Current.HorizontalViewSize,
                        VerticalViewSize = double.IsInfinity(sp.Current.VerticalViewSize) ? 0 : sp.Current.VerticalViewSize,
                        HorizontallyScrollable = sp.Current.HorizontallyScrollable,
                        VerticallyScrollable = sp.Current.VerticallyScrollable
                    },
                    _ => new { Available = true }
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting pattern details for {PatternName}", patternName);
                return new { Error = ex.Message };
            }
        }

        private async Task<OperationResult<AutomationElement?>> FindElementAsync(string elementId, string? windowTitle, int? processId)
        {
            try
            {
                AutomationElement? searchRoot = null;
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    searchRoot = _windowService.FindWindowByTitle(windowTitle, processId);
                    if (searchRoot == null)
                    {
                        _logger.LogWarning("Window '{WindowTitle}' not found", windowTitle);
                        return new OperationResult<AutomationElement?> { Success = false, Error = $"Window '{windowTitle}' not found" };
                    }
                }
                else
                {
                    searchRoot = AutomationElement.RootElement;
                }

                var condition = new OrCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, elementId),
                    new PropertyCondition(AutomationElement.NameProperty, elementId)
                );

                return await _uiAutomationHelper.FindFirstAsync(searchRoot, TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return new OperationResult<AutomationElement?> { Success = false, Error = ex.Message };
            }
        }
    }
}