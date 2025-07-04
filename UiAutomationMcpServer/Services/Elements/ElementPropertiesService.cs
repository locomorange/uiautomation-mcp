using Microsoft.Extensions.Logging;
using System.Windows.Automation;
using UiAutomationMcpServer.Models;
using UiAutomationMcpServer.Services.Windows;

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

        public ElementPropertiesService(ILogger<ElementPropertiesService> logger, IWindowService windowService)
        {
            _logger = logger;
            _windowService = windowService;
        }

        public Task<OperationResult> GetElementPropertiesAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

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
                        X = element.Current.BoundingRectangle.X,
                        Y = element.Current.BoundingRectangle.Y,
                        Width = element.Current.BoundingRectangle.Width,
                        Height = element.Current.BoundingRectangle.Height
                    },
                    HelpText = element.Current.HelpText ?? "",
                    ItemStatus = element.Current.ItemStatus ?? "",
                    ItemType = element.Current.ItemType ?? "",
                    LabeledBy = element.Current.LabeledBy?.Current.Name ?? "",
                    Orientation = element.Current.Orientation.ToString()
                };

                return Task.FromResult(new OperationResult { Success = true, Data = properties });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element properties for {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
            }
        }

        public Task<OperationResult> GetElementPatternsAsync(string elementId, string? windowTitle = null, int? processId = null)
        {
            try
            {
                var element = FindElement(elementId, windowTitle, processId);
                if (element == null)
                {
                    return Task.FromResult(new OperationResult { Success = false, Error = $"Element '{elementId}' not found" });
                }

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

                return Task.FromResult(new OperationResult { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting element patterns for {ElementId}", elementId);
                return Task.FromResult(new OperationResult { Success = false, Error = ex.Message });
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
                        Value = rvp.Current.Value,
                        Minimum = rvp.Current.Minimum,
                        Maximum = rvp.Current.Maximum,
                        SmallChange = rvp.Current.SmallChange,
                        LargeChange = rvp.Current.LargeChange,
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
                        HorizontalScrollPercent = sp.Current.HorizontalScrollPercent,
                        VerticalScrollPercent = sp.Current.VerticalScrollPercent,
                        HorizontalViewSize = sp.Current.HorizontalViewSize,
                        VerticalViewSize = sp.Current.VerticalViewSize,
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

        private AutomationElement? FindElement(string elementId, string? windowTitle, int? processId)
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
                        return null;
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

                return searchRoot.FindFirst(TreeScope.Descendants, condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding element {ElementId}", elementId);
                return null;
            }
        }
    }
}