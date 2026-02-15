using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;

namespace UIAutomationMCP.Subprocess.Core.Helpers
{
    /// <summary>
    /// Common helper for creating ElementInfo objects with optional details
    /// Shared between Worker and Monitor processes
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
                ControlType = element.Current.ControlType.ProgrammaticName ?? "",
                LocalizedControlType = string.IsNullOrEmpty(element.Current.ControlType.LocalizedControlType) ? null : element.Current.ControlType.LocalizedControlType,
                IsEnabled = element.Current.IsEnabled,
                IsVisible = !element.Current.IsOffscreen,
                IsOffscreen = element.Current.IsOffscreen,
                ProcessId = element.Current.ProcessId,
                ClassName = element.Current.ClassName ?? "",
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element, false)
            };

            // 階層的HWND検索
            var (windowHandle, rootWindowHandle) = GetHierarchicalWindowHandles(element, false);
            elementInfo.WindowHandle = windowHandle;
            elementInfo.RootWindowHandle = rootWindowHandle;

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
                ControlType = element.Cached.ControlType.ProgrammaticName ?? "",
                LocalizedControlType = string.IsNullOrEmpty(element.Cached.ControlType.LocalizedControlType) ? null : element.Cached.ControlType.LocalizedControlType,
                IsEnabled = element.Cached.IsEnabled,
                IsVisible = !element.Cached.IsOffscreen,
                IsOffscreen = element.Cached.IsOffscreen,
                ProcessId = element.Cached.ProcessId,
                ClassName = element.Cached.ClassName ?? "",
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Cached.BoundingRectangle.X,
                    Y = element.Cached.BoundingRectangle.Y,
                    Width = element.Cached.BoundingRectangle.Width,
                    Height = element.Cached.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element, true)
            };

            // 階層的HWND検索
            var (windowHandle, rootWindowHandle) = GetHierarchicalWindowHandles(element, true);
            elementInfo.WindowHandle = windowHandle;
            elementInfo.RootWindowHandle = rootWindowHandle;

            // Include details if requested
            if (includeDetails)
            {
                elementInfo.Details = CreateElementDetailsFromCached(element, logger);
            }

            return elementInfo;
        }


        private static string[] GetSupportedPatternsArray(AutomationElement element, bool useCached = false)
        {
            try
            {
                if (useCached)
                {
                    // For cached elements, GetSupportedPatterns cannot be used
                    // Infer from cached pattern properties
                    var patterns = new List<string>();

                    // Check pattern property existence to infer
                    try { if (element.GetCachedPattern(ValuePattern.Pattern) != null) patterns.Add("ValuePatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(TogglePattern.Pattern) != null) patterns.Add("TogglePatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(SelectionPattern.Pattern) != null) patterns.Add("SelectionPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(RangeValuePattern.Pattern) != null) patterns.Add("RangeValuePatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(GridPattern.Pattern) != null) patterns.Add("GridPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(TablePattern.Pattern) != null) patterns.Add("TablePatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(ScrollPattern.Pattern) != null) patterns.Add("ScrollPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(TransformPattern.Pattern) != null) patterns.Add("TransformPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(WindowPattern.Pattern) != null) patterns.Add("WindowPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(ExpandCollapsePattern.Pattern) != null) patterns.Add("ExpandCollapsePatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(DockPattern.Pattern) != null) patterns.Add("DockPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(MultipleViewPattern.Pattern) != null) patterns.Add("MultipleViewPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(TextPattern.Pattern) != null) patterns.Add("TextPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(GridItemPattern.Pattern) != null) patterns.Add("GridItemPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(TableItemPattern.Pattern) != null) patterns.Add("TableItemPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(InvokePattern.Pattern) != null) patterns.Add("InvokePatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(ScrollItemPattern.Pattern) != null) patterns.Add("ScrollItemPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(VirtualizedItemPattern.Pattern) != null) patterns.Add("VirtualizedItemPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(ItemContainerPattern.Pattern) != null) patterns.Add("ItemContainerPatternIdentifiers.Pattern"); } catch { }
                    try { if (element.GetCachedPattern(SynchronizedInputPattern.Pattern) != null) patterns.Add("SynchronizedInputPatternIdentifiers.Pattern"); } catch { }

                    return patterns.ToArray();
                }
                else
                {
                    // For non-cached elements, use GetSupportedPatterns
                    var supportedPatterns = element.GetSupportedPatterns();
                    return supportedPatterns.Select(p => p.ProgrammaticName).ToArray();
                }
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
                HelpText = string.IsNullOrEmpty(element.Current.HelpText) ? null : element.Current.HelpText,
                HasKeyboardFocus = element.Current.HasKeyboardFocus,
                IsKeyboardFocusable = element.Current.IsKeyboardFocusable,
                IsPassword = element.Current.IsPassword,
                FrameworkId = string.IsNullOrEmpty(element.Current.FrameworkId) ? null : element.Current.FrameworkId,
                RuntimeId = GetRuntimeIdString(element)
            };

            // Set pattern information safely
            SetPatternInfo(element, details, logger, useCached: false);

            return details;
        }

        private static ElementDetails CreateElementDetailsFromCached(AutomationElement element, ILogger? logger = null)
        {
            var details = new ElementDetails
            {
                HelpText = string.IsNullOrEmpty(element.Cached.HelpText) ? null : element.Cached.HelpText,
                HasKeyboardFocus = element.Cached.HasKeyboardFocus,
                IsKeyboardFocusable = element.Cached.IsKeyboardFocusable,
                IsPassword = element.Cached.IsPassword,
                FrameworkId = string.IsNullOrEmpty(element.Cached.FrameworkId) ? null : element.Cached.FrameworkId,
                RuntimeId = GetRuntimeIdString(element)
            };

            // Set pattern information safely
            SetPatternInfo(element, details, logger, useCached: true);

            return details;
        }

        /// <summary>
        /// 階層的HWND検索: 要素から親方向に辿って適切なHWND構造を取得
        /// </summary>
        /// <param name="element">開始要素</param>
        /// <param name="useCached">Cachedプロパティを使用するか</param>
        /// <returns>(WindowHandle: 最も近いHWND, RootWindowHandle: RootElement直下のHWND)</returns>
        private static (long? WindowHandle, long? RootWindowHandle) GetHierarchicalWindowHandles(AutomationElement element, bool useCached = false)
        {
            try
            {
                var current = element;
                var visited = new HashSet<IntPtr>();
                long? nearestHwnd = null;
                long? rootHwnd = null;
                AutomationElement? previousElement = null;

                // 要素から親階層を辿る
                while (current != null)
                {
                    // 循環参�EチェチE��
                    var elementPtr = new IntPtr(current.GetHashCode());
                    if (visited.Contains(elementPtr))
                        break;
                    visited.Add(elementPtr);

                    try
                    {
                        // NativeWindowHandleは常にCurrentを使用（Cachedでは利用不可）
                        var hwndValue = current.Current.NativeWindowHandle;

                        if (hwndValue != 0 && nearestHwnd == null)
                        {
                            nearestHwnd = (long)hwndValue;
                        }

                        // 親要素を取得
                        var parentElement = TreeWalker.ControlViewWalker.GetParent(current);

                        // 親がRootElementかチェック
                        if (parentElement != null && IsRootElement(parentElement))
                        {
                            // 現在の要素がRootElementの直下なので、これをrootHwndとする
                            var currentHwnd = current.Current.NativeWindowHandle;
                            if (currentHwnd != 0)
                            {
                                rootHwnd = (long)currentHwnd;
                            }
                            break;
                        }

                        previousElement = current;
                        current = parentElement;
                    }
                    catch (Exception)
                    {
                        // アクセスエラーが発生した場合、親要素に移動
                        current = TreeWalker.ControlViewWalker.GetParent(current);
                    }
                }

                return (nearestHwnd, rootHwnd);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        /// <summary>
        /// 要素がRootElementかどうかを判定
        /// </summary>
        private static bool IsRootElement(AutomationElement element)
        {
            try
            {
                return element.Equals(AutomationElement.RootElement) ||
                       TreeWalker.ControlViewWalker.GetParent(element) == null;
            }
            catch
            {
                return false;
            }
        }

        private static void SetPatternInfo(AutomationElement element, ElementDetails details, ILogger? logger, bool useCached = false)
        {
            try
            {
                // Value Pattern
                if ((useCached ? element.TryGetCachedPattern(ValuePattern.Pattern, out var valuePatternObj) : element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePatternObj)) &&
                    valuePatternObj is ValuePattern valuePattern)
                {
                    details.ValueInfo = new ValueInfo
                    {
                        Value = useCached ? valuePattern.Cached.Value ?? "" : valuePattern.Current.Value ?? "",
                        IsReadOnly = useCached ? valuePattern.Cached.IsReadOnly : valuePattern.Current.IsReadOnly
                    };
                }

                // Toggle Pattern
                if ((useCached ? element.TryGetCachedPattern(TogglePattern.Pattern, out var togglePatternObj) : element.TryGetCurrentPattern(TogglePattern.Pattern, out togglePatternObj)) &&
                    togglePatternObj is TogglePattern togglePattern)
                {
                    var state = useCached ? togglePattern.Cached.ToggleState : togglePattern.Current.ToggleState;
                    details.Toggle = new ToggleInfo
                    {
                        State = state.ToString(),
                        IsToggled = state == ToggleState.On,
                        CanToggle = true
                    };
                }

                // RangeValue Pattern
                if ((useCached ? element.TryGetCachedPattern(RangeValuePattern.Pattern, out var rangePatternObj) : element.TryGetCurrentPattern(RangeValuePattern.Pattern, out rangePatternObj)) &&
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

                // Window Pattern
                if ((useCached ? element.TryGetCachedPattern(WindowPattern.Pattern, out var windowPatternObj) : element.TryGetCurrentPattern(WindowPattern.Pattern, out windowPatternObj)) &&
                    windowPatternObj is WindowPattern windowPattern)
                {
                    details.Window = new WindowPatternInfo
                    {
                        CanMaximize = useCached ? windowPattern.Cached.CanMaximize : windowPattern.Current.CanMaximize,
                        CanMinimize = useCached ? windowPattern.Cached.CanMinimize : windowPattern.Current.CanMinimize,
                        IsModal = useCached ? windowPattern.Cached.IsModal : windowPattern.Current.IsModal,
                        IsTopmost = useCached ? windowPattern.Cached.IsTopmost : windowPattern.Current.IsTopmost,
                        InteractionState = (useCached ? windowPattern.Cached.WindowInteractionState : windowPattern.Current.WindowInteractionState).ToString(),
                        VisualState = (useCached ? windowPattern.Cached.WindowVisualState : windowPattern.Current.WindowVisualState).ToString()
                    };
                }

                // Selection Pattern
                if ((useCached ? element.TryGetCachedPattern(SelectionPattern.Pattern, out var selectionPatternObj) : element.TryGetCurrentPattern(SelectionPattern.Pattern, out selectionPatternObj)) &&
                    selectionPatternObj is SelectionPattern selectionPattern)
                {
                    var selection = useCached ? selectionPattern.Cached.GetSelection() : selectionPattern.Current.GetSelection();
                    details.Selection = new SelectionInfo
                    {
                        CanSelectMultiple = useCached ? selectionPattern.Cached.CanSelectMultiple : selectionPattern.Current.CanSelectMultiple,
                        IsSelectionRequired = useCached ? selectionPattern.Cached.IsSelectionRequired : selectionPattern.Current.IsSelectionRequired,
                        SelectedCount = selection.Length,
                        SelectedItems = selection.Select(item =>
                        {
                            try
                            {
                                return new SelectionItemInfo
                                {
                                    AutomationId = (useCached ? item.Cached.AutomationId : item.Current.AutomationId) ?? "",
                                    Name = (useCached ? item.Cached.Name : item.Current.Name) ?? "",
                                    ControlType = (useCached ? item.Cached.ControlType : item.Current.ControlType).ProgrammaticName ?? ""
                                };
                            }
                            catch
                            {
                                // Cached properties may not be available for selection items;
                                // fall back to Current which triggers a cross-process call
                                return new SelectionItemInfo
                                {
                                    AutomationId = item.Current.AutomationId ?? "",
                                    Name = item.Current.Name ?? "",
                                    ControlType = item.Current.ControlType.ProgrammaticName ?? ""
                                };
                            }
                        }).ToList()
                    };
                }

                // Grid Pattern
                if ((useCached ? element.TryGetCachedPattern(GridPattern.Pattern, out var gridPatternObj) : element.TryGetCurrentPattern(GridPattern.Pattern, out gridPatternObj)) &&
                    gridPatternObj is GridPattern gridPattern)
                {
                    // Check if the element also supports TablePattern to infer header/selection info
                    var hasTablePattern = useCached
                        ? element.TryGetCachedPattern(TablePattern.Pattern, out _)
                        : element.TryGetCurrentPattern(TablePattern.Pattern, out _);
                    var hasSelectionPattern = useCached
                        ? element.TryGetCachedPattern(SelectionPattern.Pattern, out _)
                        : element.TryGetCurrentPattern(SelectionPattern.Pattern, out _);

                    details.Grid = new GridInfo
                    {
                        RowCount = useCached ? gridPattern.Cached.RowCount : gridPattern.Current.RowCount,
                        ColumnCount = useCached ? gridPattern.Cached.ColumnCount : gridPattern.Current.ColumnCount,
                        CanSelectMultiple = hasSelectionPattern,
                        HasRowHeaders = hasTablePattern,
                        HasColumnHeaders = hasTablePattern
                    };
                }

                // Scroll Pattern
                if ((useCached ? element.TryGetCachedPattern(ScrollPattern.Pattern, out var scrollPatternObj) : element.TryGetCurrentPattern(ScrollPattern.Pattern, out scrollPatternObj)) &&
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

                // Text Pattern
                if ((useCached ? element.TryGetCachedPattern(TextPattern.Pattern, out var textPatternObj) : element.TryGetCurrentPattern(TextPattern.Pattern, out textPatternObj)) &&
                    textPatternObj is TextPattern textPattern)
                {
                    try
                    {
                        var documentRange = textPattern.DocumentRange;
                        var text = documentRange.GetText(-1);
                        var selection = textPattern.GetSelection();
                        var selectedText = selection.Length > 0 ? selection[0].GetText(-1) : "";
                        
                        details.Text = new TextInfo
                        {
                            Text = text ?? "",
                            Length = text?.Length ?? 0,
                            SelectedText = selectedText ?? "",
                            HasSelection = selection.Length > 0
                        };
                    }
                    catch
                    {
                        details.Text = new TextInfo { Text = "", Length = 0, SelectedText = "", HasSelection = false };
                    }
                }

                // Transform Pattern
                if ((useCached ? element.TryGetCachedPattern(TransformPattern.Pattern, out var transformPatternObj) : element.TryGetCurrentPattern(TransformPattern.Pattern, out transformPatternObj)) &&
                    transformPatternObj is TransformPattern transformPattern)
                {
                    var bounds = useCached ? element.Cached.BoundingRectangle : element.Current.BoundingRectangle;
                    details.Transform = new TransformInfo
                    {
                        CanMove = useCached ? transformPattern.Cached.CanMove : transformPattern.Current.CanMove,
                        CanResize = useCached ? transformPattern.Cached.CanResize : transformPattern.Current.CanResize,
                        CanRotate = useCached ? transformPattern.Cached.CanRotate : transformPattern.Current.CanRotate,
                        CurrentX = bounds.X,
                        CurrentY = bounds.Y,
                        CurrentWidth = bounds.Width,
                        CurrentHeight = bounds.Height
                    };
                }

                // ExpandCollapse Pattern
                if ((useCached ? element.TryGetCachedPattern(ExpandCollapsePattern.Pattern, out var expandCollapsePatternObj) : element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out expandCollapsePatternObj)) &&
                    expandCollapsePatternObj is ExpandCollapsePattern expandCollapsePattern)
                {
                    details.ExpandCollapse = new ExpandCollapseInfo
                    {
                        State = (useCached ? expandCollapsePattern.Cached.ExpandCollapseState : expandCollapsePattern.Current.ExpandCollapseState).ToString()
                    };
                }

                // Dock Pattern
                if ((useCached ? element.TryGetCachedPattern(DockPattern.Pattern, out var dockPatternObj) : element.TryGetCurrentPattern(DockPattern.Pattern, out dockPatternObj)) &&
                    dockPatternObj is DockPattern dockPattern)
                {
                    details.Dock = new DockInfo
                    {
                        Position = (useCached ? dockPattern.Cached.DockPosition : dockPattern.Current.DockPosition).ToString()
                    };
                }

                // MultipleView Pattern
                if ((useCached ? element.TryGetCachedPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) : element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out multipleViewPatternObj)) &&
                    multipleViewPatternObj is MultipleViewPattern multipleViewPattern)
                {
                    var currentView = useCached ? multipleViewPattern.Cached.CurrentView : multipleViewPattern.Current.CurrentView;
                    var supportedViews = useCached ? multipleViewPattern.Cached.GetSupportedViews() : multipleViewPattern.Current.GetSupportedViews();
                    
                    details.MultipleView = new MultipleViewInfo
                    {
                        CurrentView = currentView,
                        SupportedViewCount = supportedViews.Length,
                        ViewChangedEventSupported = true,
                        AvailableViews = supportedViews.Select(viewId => new PatternViewInfo
                        {
                            ViewId = viewId,
                            ViewName = multipleViewPattern.GetViewName(viewId) ?? $"View {viewId}"
                        }).ToList()
                    };
                }

                // GridItem Pattern
                if ((useCached ? element.TryGetCachedPattern(GridItemPattern.Pattern, out var gridItemPatternObj) : element.TryGetCurrentPattern(GridItemPattern.Pattern, out gridItemPatternObj)) &&
                    gridItemPatternObj is GridItemPattern gridItemPattern)
                {
                    var containingGrid = useCached ? gridItemPattern.Cached.ContainingGrid : gridItemPattern.Current.ContainingGrid;
                    string? containingGridId = null;
                    if (containingGrid != null)
                    {
                        try
                        {
                            containingGridId = useCached
                                ? (containingGrid.Cached.AutomationId ?? containingGrid.Cached.Name)
                                : (containingGrid.Current.AutomationId ?? containingGrid.Current.Name);
                        }
                        catch
                        {
                            // Cached properties may not be available; fall back to Current
                            containingGridId = containingGrid.Current.AutomationId ?? containingGrid.Current.Name;
                        }
                    }

                    details.GridItem = new GridItemInfo
                    {
                        Row = useCached ? gridItemPattern.Cached.Row : gridItemPattern.Current.Row,
                        Column = useCached ? gridItemPattern.Cached.Column : gridItemPattern.Current.Column,
                        RowSpan = useCached ? gridItemPattern.Cached.RowSpan : gridItemPattern.Current.RowSpan,
                        ColumnSpan = useCached ? gridItemPattern.Cached.ColumnSpan : gridItemPattern.Current.ColumnSpan,
                        ContainingGrid = containingGridId
                    };
                }

                // TableItem Pattern
                if ((useCached ? element.TryGetCachedPattern(TableItemPattern.Pattern, out var tableItemPatternObj) : element.TryGetCurrentPattern(TableItemPattern.Pattern, out tableItemPatternObj)) &&
                    tableItemPatternObj is TableItemPattern tableItemPattern)
                {
                    try
                    {
                        var rowHeaders = useCached ? tableItemPattern.Cached.GetRowHeaderItems() : tableItemPattern.Current.GetRowHeaderItems();
                        var columnHeaders = useCached ? tableItemPattern.Cached.GetColumnHeaderItems() : tableItemPattern.Current.GetColumnHeaderItems();
                        
                        details.TableItem = new TableItemInfo
                        {
                            RowHeaders = rowHeaders.Select(h => CreateElementInfo(h, false, logger)).ToList(),
                            ColumnHeaders = columnHeaders.Select(h => CreateElementInfo(h, false, logger)).ToList()
                        };
                    }
                    catch
                    {
                        details.TableItem = new TableItemInfo();
                    }
                }

                // Table Pattern
                if ((useCached ? element.TryGetCachedPattern(TablePattern.Pattern, out var tablePatternObj) : element.TryGetCurrentPattern(TablePattern.Pattern, out tablePatternObj)) &&
                    tablePatternObj is TablePattern tablePattern)
                {
                    try
                    {
                        var rowHeaders = useCached ? tablePattern.Cached.GetRowHeaders() : tablePattern.Current.GetRowHeaders();
                        var columnHeaders = useCached ? tablePattern.Cached.GetColumnHeaders() : tablePattern.Current.GetColumnHeaders();
                        
                        details.Table = new TableInfo
                        {
                            RowCount = useCached ? tablePattern.Cached.RowCount : tablePattern.Current.RowCount,
                            ColumnCount = useCached ? tablePattern.Cached.ColumnCount : tablePattern.Current.ColumnCount,
                            RowOrColumnMajor = (useCached ? tablePattern.Cached.RowOrColumnMajor : tablePattern.Current.RowOrColumnMajor).ToString(),
                            RowHeaders = rowHeaders.Select(h => CreateElementInfo(h, false, logger)).ToList(),
                            ColumnHeaders = columnHeaders.Select(h => CreateElementInfo(h, false, logger)).ToList()
                        };
                    }
                    catch
                    {
                        details.Table = new TableInfo();
                    }
                }

                // Invoke Pattern
                if ((useCached ? element.TryGetCachedPattern(InvokePattern.Pattern, out var invokePatternObj) : element.TryGetCurrentPattern(InvokePattern.Pattern, out invokePatternObj)) &&
                    invokePatternObj is InvokePattern)
                {
                    details.Invoke = new InvokeInfo { IsInvokable = true };
                }

                // ScrollItem Pattern
                if ((useCached ? element.TryGetCachedPattern(ScrollItemPattern.Pattern, out var scrollItemPatternObj) : element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out scrollItemPatternObj)) &&
                    scrollItemPatternObj is ScrollItemPattern)
                {
                    details.ScrollItem = new ScrollItemInfo { IsScrollable = true };
                }

                // VirtualizedItem Pattern
                if ((useCached ? element.TryGetCachedPattern(VirtualizedItemPattern.Pattern, out var virtualizedItemPatternObj) : element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out virtualizedItemPatternObj)) &&
                    virtualizedItemPatternObj is VirtualizedItemPattern)
                {
                    details.VirtualizedItem = new VirtualizedItemInfo { IsVirtualized = true };
                }

                // ItemContainer Pattern
                if ((useCached ? element.TryGetCachedPattern(ItemContainerPattern.Pattern, out var itemContainerPatternObj) : element.TryGetCurrentPattern(ItemContainerPattern.Pattern, out itemContainerPatternObj)) &&
                    itemContainerPatternObj is ItemContainerPattern)
                {
                    details.ItemContainer = new ItemContainerInfo { IsItemContainer = true };
                }

                // SynchronizedInput Pattern
                if ((useCached ? element.TryGetCachedPattern(SynchronizedInputPattern.Pattern, out var synchronizedInputPatternObj) : element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out synchronizedInputPatternObj)) &&
                    synchronizedInputPatternObj is SynchronizedInputPattern)
                {
                    details.SynchronizedInput = new SynchronizedInputInfo { SupportsSynchronizedInput = true };
                }

            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to retrieve pattern information for element");
            }
        }

        /// <summary>
        /// Gets the RuntimeId as a string representation for debugging purposes
        /// </summary>
        private static string? GetRuntimeIdString(AutomationElement element)
        {
            try
            {
                var runtimeId = element.GetRuntimeId();
                if (runtimeId == null || runtimeId.Length == 0)
                    return null;

                return string.Join(",", runtimeId);
            }
            catch (Exception)
            {
                // RuntimeId access failed, return null
                return null;
            }
        }
    }
}

