using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;

namespace UIAutomationMCP.Worker.Helpers
{
    /// <summary>
    /// Common helper for creating ElementInfo objects with optional details
    /// </summary>
    public static class ElementInfoBuilder
    {
        /// <summary>
        /// Creates an ElementInfo from AutomationElement with optional details
        /// </summary>
        public static ElementInfo CreateElementInfo(AutomationElement element, bool includeDetails = false, ILogger? logger = null)
        {
            var elementInfo = new ElementInfo
            {
                AutomationId = element.Current.AutomationId ?? "",
                Name = element.Current.Name ?? "",
                ControlType = element.Current.ControlType.LocalizedControlType ?? "",
                LocalizedControlType = element.Current.ControlType.LocalizedControlType ?? "",
                IsEnabled = element.Current.IsEnabled,
                IsVisible = !element.Current.IsOffscreen,
                IsOffscreen = element.Current.IsOffscreen,
                ProcessId = element.Current.ProcessId,
                ClassName = element.Current.ClassName ?? "",
                FrameworkId = element.Current.FrameworkId ?? "",
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element)
            };

            // Include details if requested
            if (includeDetails)
            {
                elementInfo.Details = CreateElementDetails(element, logger);
            }

            return elementInfo;
        }

        /// <summary>
        /// Creates an ElementInfo from cached AutomationElement with optional details
        /// </summary>
        public static ElementInfo CreateElementInfoFromCached(AutomationElement element, bool includeDetails = false, ILogger? logger = null)
        {
            var elementInfo = new ElementInfo
            {
                AutomationId = element.Cached.AutomationId ?? "",
                Name = element.Cached.Name ?? "",
                ControlType = element.Cached.ControlType.LocalizedControlType ?? "",
                LocalizedControlType = element.Cached.ControlType.LocalizedControlType ?? "",
                IsEnabled = element.Cached.IsEnabled,
                IsVisible = !element.Cached.IsOffscreen,
                IsOffscreen = element.Cached.IsOffscreen,
                ProcessId = element.Cached.ProcessId,
                ClassName = element.Cached.ClassName ?? "",
                FrameworkId = element.Cached.FrameworkId ?? "",
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Cached.BoundingRectangle.X,
                    Y = element.Cached.BoundingRectangle.Y,
                    Width = element.Cached.BoundingRectangle.Width,
                    Height = element.Cached.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element)
            };

            // Include details if requested
            if (includeDetails)
            {
                elementInfo.Details = CreateElementDetailsFromCached(element, logger);
            }

            return elementInfo;
        }

        private static string[] GetSupportedPatternsArray(AutomationElement element)
        {
            try
            {
                var supportedPatterns = element.GetSupportedPatterns();
                return supportedPatterns.Select(p => p.ProgrammaticName).ToArray();
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private static ElementDetails CreateElementDetails(AutomationElement element, ILogger? logger = null)
        {
            var details = new ElementDetails
            {
                HelpText = element.Current.HelpText ?? "",
                HasKeyboardFocus = element.Current.HasKeyboardFocus,
                IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                IsPassword = element.Current.IsPassword
            };

            // Set pattern information safely
            SetPatternInfo(element, details, logger, useCached: false);
            
            return details;
        }

        private static ElementDetails CreateElementDetailsFromCached(AutomationElement element, ILogger? logger = null)
        {
            var details = new ElementDetails
            {
                HelpText = element.Cached.HelpText ?? "",
                HasKeyboardFocus = element.Cached.HasKeyboardFocus,
                IsKeyboardFocusable = element.Cached.IsKeyboardFocusable,
                IsPassword = element.Cached.IsPassword
            };

            // Set pattern information safely
            SetPatternInfo(element, details, logger, useCached: true);
            
            return details;
        }

        private static void SetPatternInfo(AutomationElement element, ElementDetails details, ILogger? logger, bool useCached = false)
        {
            try
            {
                // Value Pattern
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePatternObj) && 
                    valuePatternObj is ValuePattern valuePattern)
                {
                    details.ValueInfo = new ValueInfo
                    {
                        Value = useCached ? valuePattern.Cached.Value ?? "" : valuePattern.Current.Value ?? "",
                        IsReadOnly = useCached ? valuePattern.Cached.IsReadOnly : valuePattern.Current.IsReadOnly
                    };
                }

                // Toggle Pattern
                if (element.TryGetCurrentPattern(TogglePattern.Pattern, out var togglePatternObj) && 
                    togglePatternObj is TogglePattern togglePattern)
                {
                    details.Toggle = new ToggleInfo
                    {
                        State = useCached ? togglePattern.Cached.ToggleState.ToString() : togglePattern.Current.ToggleState.ToString(),
                        CanToggle = true
                    };
                }

                // Selection Pattern
                if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selectionPatternObj) && 
                    selectionPatternObj is SelectionPattern selectionPattern)
                {
                    details.Selection = new SelectionInfo
                    {
                        CanSelectMultiple = useCached ? selectionPattern.Cached.CanSelectMultiple : selectionPattern.Current.CanSelectMultiple,
                        IsSelectionRequired = useCached ? selectionPattern.Cached.IsSelectionRequired : selectionPattern.Current.IsSelectionRequired
                    };
                }

                // Range Pattern
                if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var rangePatternObj) && 
                    rangePatternObj is RangeValuePattern rangePattern)
                {
                    details.Range = new RangeInfo
                    {
                        Value = useCached ? rangePattern.Cached.Value : rangePattern.Current.Value,
                        Minimum = useCached ? rangePattern.Cached.Minimum : rangePattern.Current.Minimum,
                        Maximum = useCached ? rangePattern.Cached.Maximum : rangePattern.Current.Maximum,
                        SmallChange = useCached ? rangePattern.Cached.SmallChange : rangePattern.Current.SmallChange,
                        LargeChange = useCached ? rangePattern.Cached.LargeChange : rangePattern.Current.LargeChange,
                        IsReadOnly = useCached ? rangePattern.Cached.IsReadOnly : rangePattern.Current.IsReadOnly
                    };
                }

                // Grid Pattern
                if (element.TryGetCurrentPattern(GridPattern.Pattern, out var gridPatternObj) && 
                    gridPatternObj is GridPattern gridPattern)
                {
                    details.Grid = new GridInfo
                    {
                        RowCount = useCached ? gridPattern.Cached.RowCount : gridPattern.Current.RowCount,
                        ColumnCount = useCached ? gridPattern.Cached.ColumnCount : gridPattern.Current.ColumnCount
                    };
                }

                // Table Pattern
                if (element.TryGetCurrentPattern(TablePattern.Pattern, out var tablePatternObj) && 
                    tablePatternObj is TablePattern tablePattern)
                {
                    var rowHeaders = new List<ElementInfo>();
                    var columnHeaders = new List<ElementInfo>();
                    
                    try
                    {
                        var rowHeaderElements = useCached ? tablePattern.Cached.GetRowHeaders() : tablePattern.Current.GetRowHeaders();
                        foreach (var header in rowHeaderElements)
                        {
                            rowHeaders.Add(new ElementInfo
                            {
                                AutomationId = useCached ? header.Cached.AutomationId ?? "" : header.Current.AutomationId ?? "",
                                Name = useCached ? header.Cached.Name ?? "" : header.Current.Name ?? "",
                                ControlType = useCached ? header.Cached.ControlType.LocalizedControlType ?? "" : header.Current.ControlType.LocalizedControlType ?? ""
                            });
                        }
                        
                        var columnHeaderElements = useCached ? tablePattern.Cached.GetColumnHeaders() : tablePattern.Current.GetColumnHeaders();
                        foreach (var header in columnHeaderElements)
                        {
                            columnHeaders.Add(new ElementInfo
                            {
                                AutomationId = useCached ? header.Cached.AutomationId ?? "" : header.Current.AutomationId ?? "",
                                Name = useCached ? header.Cached.Name ?? "" : header.Current.Name ?? "",
                                ControlType = useCached ? header.Cached.ControlType.LocalizedControlType ?? "" : header.Current.ControlType.LocalizedControlType ?? ""
                            });
                        }
                    }
                    catch { }

                    details.Table = new TableInfo
                    {
                        RowCount = useCached ? tablePattern.Cached.RowCount : tablePattern.Current.RowCount,
                        ColumnCount = useCached ? tablePattern.Cached.ColumnCount : tablePattern.Current.ColumnCount,
                        RowOrColumnMajor = useCached ? tablePattern.Cached.RowOrColumnMajor.ToString() : tablePattern.Current.RowOrColumnMajor.ToString(),
                        RowHeaders = rowHeaders,
                        ColumnHeaders = columnHeaders
                    };
                }

                // Scroll Pattern
                if (element.TryGetCurrentPattern(ScrollPattern.Pattern, out var scrollPatternObj) && 
                    scrollPatternObj is ScrollPattern scrollPattern)
                {
                    details.Scroll = new ScrollInfo
                    {
                        HorizontalPercent = useCached ? scrollPattern.Cached.HorizontalScrollPercent : scrollPattern.Current.HorizontalScrollPercent,
                        VerticalPercent = useCached ? scrollPattern.Cached.VerticalScrollPercent : scrollPattern.Current.VerticalScrollPercent,
                        HorizontalViewSize = useCached ? scrollPattern.Cached.HorizontalViewSize : scrollPattern.Current.HorizontalViewSize,
                        VerticalViewSize = useCached ? scrollPattern.Cached.VerticalViewSize : scrollPattern.Current.VerticalViewSize,
                        HorizontallyScrollable = useCached ? scrollPattern.Cached.HorizontallyScrollable : scrollPattern.Current.HorizontallyScrollable,
                        VerticallyScrollable = useCached ? scrollPattern.Cached.VerticallyScrollable : scrollPattern.Current.VerticallyScrollable
                    };
                }

                // Transform Pattern
                if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var transformPatternObj) && 
                    transformPatternObj is TransformPattern transformPattern)
                {
                    details.Transform = new TransformInfo
                    {
                        CanMove = useCached ? transformPattern.Cached.CanMove : transformPattern.Current.CanMove,
                        CanResize = useCached ? transformPattern.Cached.CanResize : transformPattern.Current.CanResize,
                        CanRotate = useCached ? transformPattern.Cached.CanRotate : transformPattern.Current.CanRotate
                    };
                }

                // Window Pattern
                if (element.TryGetCurrentPattern(WindowPattern.Pattern, out var windowPatternObj) && 
                    windowPatternObj is WindowPattern windowPattern)
                {
                    details.Window = new WindowPatternInfo
                    {
                        VisualState = useCached ? windowPattern.Cached.WindowVisualState.ToString() : windowPattern.Current.WindowVisualState.ToString(),
                        InteractionState = useCached ? windowPattern.Cached.WindowInteractionState.ToString() : windowPattern.Current.WindowInteractionState.ToString(),
                        IsModal = useCached ? windowPattern.Cached.IsModal : windowPattern.Current.IsModal,
                        IsTopmost = useCached ? windowPattern.Cached.IsTopmost : windowPattern.Current.IsTopmost,
                        CanMaximize = useCached ? windowPattern.Cached.CanMaximize : windowPattern.Current.CanMaximize,
                        CanMinimize = useCached ? windowPattern.Cached.CanMinimize : windowPattern.Current.CanMinimize
                    };
                }

                // ExpandCollapse Pattern
                if (element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var expandCollapsePatternObj) && 
                    expandCollapsePatternObj is ExpandCollapsePattern expandCollapsePattern)
                {
                    details.ExpandCollapse = new ExpandCollapseInfo
                    {
                        State = useCached ? expandCollapsePattern.Cached.ExpandCollapseState.ToString() : expandCollapsePattern.Current.ExpandCollapseState.ToString()
                    };
                }

                // Dock Pattern
                if (element.TryGetCurrentPattern(DockPattern.Pattern, out var dockPatternObj) && 
                    dockPatternObj is DockPattern dockPattern)
                {
                    details.Dock = new DockInfo
                    {
                        Position = useCached ? dockPattern.Cached.DockPosition.ToString() : dockPattern.Current.DockPosition.ToString()
                    };
                }

                // MultipleView Pattern
                if (element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) && 
                    multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                {
                    var availableViews = new List<PatternViewInfo>();
                    try
                    {
                        var viewIds = useCached ? multipleViewPattern.Cached.GetSupportedViews() : multipleViewPattern.Current.GetSupportedViews();
                        foreach (var viewId in viewIds)
                        {
                            string viewName = "";
                            try
                            {
                                viewName = multipleViewPattern.GetViewName(viewId);
                            }
                            catch { }
                            
                            availableViews.Add(new PatternViewInfo
                            {
                                ViewId = viewId,
                                ViewName = viewName
                            });
                        }
                    }
                    catch { }

                    details.MultipleView = new MultipleViewInfo
                    {
                        CurrentView = useCached ? multipleViewPattern.Cached.CurrentView : multipleViewPattern.Current.CurrentView,
                        AvailableViews = availableViews
                    };
                }

                // Text Pattern
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPatternObj) && 
                    textPatternObj is TextPattern textPattern)
                {
                    string textValue = "";
                    string selectedText = "";
                    try
                    {
                        textValue = textPattern.DocumentRange.GetText(-1);
                        var selections = textPattern.GetSelection();
                        if (selections.Length > 0)
                        {
                            selectedText = selections[0].GetText(-1);
                        }
                    }
                    catch { }

                    details.Text = new TextInfo
                    {
                        Text = textValue,
                        Length = textValue.Length,
                        SelectedText = selectedText,
                        HasSelection = !string.IsNullOrEmpty(selectedText)
                    };
                }

                // GridItem Pattern
                if (element.TryGetCurrentPattern(GridItemPattern.Pattern, out var gridItemPatternObj) && 
                    gridItemPatternObj is GridItemPattern gridItemPattern)
                {
                    details.GridItem = new GridItemInfo
                    {
                        Row = useCached ? gridItemPattern.Cached.Row : gridItemPattern.Current.Row,
                        Column = useCached ? gridItemPattern.Cached.Column : gridItemPattern.Current.Column,
                        RowSpan = useCached ? gridItemPattern.Cached.RowSpan : gridItemPattern.Current.RowSpan,
                        ColumnSpan = useCached ? gridItemPattern.Cached.ColumnSpan : gridItemPattern.Current.ColumnSpan,
                        ContainingGrid = useCached 
                            ? gridItemPattern.Cached.ContainingGrid?.Cached.Name ?? ""
                            : gridItemPattern.Current.ContainingGrid?.Current.Name ?? ""
                    };
                }

                // TableItem Pattern
                if (element.TryGetCurrentPattern(TableItemPattern.Pattern, out var tableItemPatternObj) && 
                    tableItemPatternObj is TableItemPattern tableItemPattern)
                {
                    var rowHeaderItems = new List<ElementInfo>();
                    var columnHeaderItems = new List<ElementInfo>();
                    
                    try
                    {
                        var rowHeaders = useCached ? tableItemPattern.Cached.GetRowHeaderItems() : tableItemPattern.Current.GetRowHeaderItems();
                        foreach (var header in rowHeaders)
                        {
                            rowHeaderItems.Add(new ElementInfo
                            {
                                AutomationId = useCached ? header.Cached.AutomationId ?? "" : header.Current.AutomationId ?? "",
                                Name = useCached ? header.Cached.Name ?? "" : header.Current.Name ?? "",
                                ControlType = useCached ? header.Cached.ControlType.LocalizedControlType ?? "" : header.Current.ControlType.LocalizedControlType ?? ""
                            });
                        }
                        
                        var columnHeaders = useCached ? tableItemPattern.Cached.GetColumnHeaderItems() : tableItemPattern.Current.GetColumnHeaderItems();
                        foreach (var header in columnHeaders)
                        {
                            columnHeaderItems.Add(new ElementInfo
                            {
                                AutomationId = useCached ? header.Cached.AutomationId ?? "" : header.Current.AutomationId ?? "",
                                Name = useCached ? header.Cached.Name ?? "" : header.Current.Name ?? "",
                                ControlType = useCached ? header.Cached.ControlType.LocalizedControlType ?? "" : header.Current.ControlType.LocalizedControlType ?? ""
                            });
                        }
                    }
                    catch { }

                    details.TableItem = new TableItemInfo
                    {
                        RowHeaders = rowHeaderItems,
                        ColumnHeaders = columnHeaderItems
                    };
                }

                // Invoke Pattern
                if (element.GetSupportedPatterns().Contains(InvokePattern.Pattern))
                {
                    details.Invoke = new InvokeInfo
                    {
                        IsInvokable = true
                    };
                }

                // ScrollItem Pattern  
                if (element.GetSupportedPatterns().Contains(ScrollItemPattern.Pattern))
                {
                    details.ScrollItem = new ScrollItemInfo
                    {
                        IsScrollable = true
                    };
                }

                // VirtualizedItem Pattern
                if (element.GetSupportedPatterns().Contains(VirtualizedItemPattern.Pattern))
                {
                    details.VirtualizedItem = new VirtualizedItemInfo
                    {
                        IsVirtualized = true
                    };
                }

                // ItemContainer Pattern
                if (element.GetSupportedPatterns().Contains(ItemContainerPattern.Pattern))
                {
                    details.ItemContainer = new ItemContainerInfo
                    {
                        IsItemContainer = true
                    };
                }

                // SynchronizedInput Pattern
                if (element.GetSupportedPatterns().Contains(SynchronizedInputPattern.Pattern))
                {
                    details.SynchronizedInput = new SynchronizedInputInfo
                    {
                        SupportsSynchronizedInput = true
                    };
                }

                // Accessibility Information
                details.Accessibility = new AccessibilityInfo
                {
                    AcceleratorKey = useCached ? element.Cached.AcceleratorKey ?? "" : element.Current.AcceleratorKey ?? "",
                    AccessKey = useCached ? element.Cached.AccessKey ?? "" : element.Current.AccessKey ?? "",
                    HelpText = useCached ? element.Cached.HelpText ?? "" : element.Current.HelpText ?? ""
                };

            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to retrieve pattern information for element");
            }
        }
    }
}