using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;

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
                LocalizedControlType = string.IsNullOrEmpty(element.Current.ControlType.LocalizedControlType) ? null : element.Current.ControlType.LocalizedControlType,
                IsEnabled = element.Current.IsEnabled,
                IsVisible = !element.Current.IsOffscreen,
                IsOffscreen = element.Current.IsOffscreen,
                ProcessId = element.Current.ProcessId,
                MainProcessId = GetMainProcessId(element, false),
                ClassName = element.Current.ClassName ?? "",
                FrameworkId = string.IsNullOrEmpty(element.Current.FrameworkId) ? null : element.Current.FrameworkId,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Current.BoundingRectangle.X,
                    Y = element.Current.BoundingRectangle.Y,
                    Width = element.Current.BoundingRectangle.Width,
                    Height = element.Current.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element, false)
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
                LocalizedControlType = string.IsNullOrEmpty(element.Cached.ControlType.LocalizedControlType) ? null : element.Cached.ControlType.LocalizedControlType,
                IsEnabled = element.Cached.IsEnabled,
                IsVisible = !element.Cached.IsOffscreen,
                IsOffscreen = element.Cached.IsOffscreen,
                ProcessId = element.Cached.ProcessId,
                MainProcessId = GetMainProcessId(element, true),
                ClassName = element.Cached.ClassName ?? "",
                FrameworkId = string.IsNullOrEmpty(element.Cached.FrameworkId) ? null : element.Cached.FrameworkId,
                BoundingRectangle = new BoundingRectangle
                {
                    X = element.Cached.BoundingRectangle.X,
                    Y = element.Cached.BoundingRectangle.Y,
                    Width = element.Cached.BoundingRectangle.Width,
                    Height = element.Cached.BoundingRectangle.Height
                },
                SupportedPatterns = GetSupportedPatternsArray(element, true)
            };

            // Include details if requested
            if (includeDetails)
            {
                elementInfo.Details = CreateElementDetailsFromCached(element, logger);
            }

            return elementInfo;
        }

        private static int? GetMainProcessId(AutomationElement element, bool useCached = false)
        {
            try
            {
                // まず要素のプロセスIDを取得
                var elementProcessId = useCached ? element.Cached.ProcessId : element.Current.ProcessId;
                
                // TreeWalkerを使用してウィンドウ要素まで遡る
                var current = element;
                
                while (current != null)
                {
                    try
                    {
                        var controlType = useCached ? current.Cached.ControlType : current.Current.ControlType;
                        
                        // ウィンドウ要素が見つかった場合
                        if (controlType == ControlType.Window)
                        {
                            var windowProcessId = useCached ? current.Cached.ProcessId : current.Current.ProcessId;
                            
                            // ウィンドウのプロセスIDからメインプロセスIDを特定
                            var windowMainProcessId = FindMainProcessId(windowProcessId);
                            
                            // 自分自身のプロセスと同じ場合はnullを返す
                            return windowMainProcessId == elementProcessId ? null : windowMainProcessId;
                        }
                        
                        // 親要素に移動
                        current = TreeWalker.ControlViewWalker.GetParent(current);
                    }
                    catch
                    {
                        // アクセスエラーが発生した場合は親要素に移動
                        current = TreeWalker.ControlViewWalker.GetParent(current);
                    }
                }
                
                // ウィンドウが見つからない場合は要素のプロセスIDからメインプロセスを特定
                var mainProcessId = FindMainProcessId(elementProcessId);
                
                // 自分自身のプロセスと同じ場合はnullを返す
                return mainProcessId == elementProcessId ? null : mainProcessId;
            }
            catch (Exception)
            {
                // 親要素の取得に失敗した場合はnullを返す
                return null;
            }
        }

        private static int? FindMainProcessId(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                var current = process;
                var visited = new HashSet<int>();
                
                // 親プロセスを辿ってメインプロセスを見つける
                while (current != null && !visited.Contains(current.Id))
                {
                    visited.Add(current.Id);
                    
                    try
                    {
                        // 親プロセスを取得
                        var parentId = GetParentProcessId(current.Id);
                        if (parentId == null || parentId == 0)
                        {
                            // 親プロセスがない場合、現在のプロセスがメインプロセス
                            return current.Id;
                        }
                        
                        // 親プロセスが存在し、同じプロセス名の場合は親に移動
                        try
                        {
                            var parentProcess = Process.GetProcessById(parentId.Value);
                            if (parentProcess.ProcessName.Equals(current.ProcessName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (current != process) current.Dispose();
                                current = parentProcess;
                            }
                            else
                            {
                                // 異なるプロセス名の場合、現在のプロセスがメインプロセス
                                parentProcess.Dispose();
                                return current.Id;
                            }
                        }
                        catch (ArgumentException)
                        {
                            // 親プロセスが既に終了している場合、現在のプロセスがメインプロセス
                            return current.Id;
                        }
                    }
                    catch
                    {
                        // 親プロセスへのアクセスに失敗した場合、現在のプロセスがメインプロセス
                        return current.Id;
                    }
                }
                
                return current?.Id ?? processId;
            }
            catch
            {
                // プロセス情報の取得に失敗した場合は元のプロセスIDを返す
                return processId;
            }
        }

        private static int? GetParentProcessId(int processId)
        {
            try
            {
                var query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}";
                using var searcher = new System.Management.ManagementObjectSearcher(query);
                using var results = searcher.Get();
                
                foreach (System.Management.ManagementObject obj in results)
                {
                    return Convert.ToInt32(obj["ParentProcessId"]);
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string[] GetSupportedPatternsArray(AutomationElement element, bool useCached = false)
        {
            try
            {
                if (useCached)
                {
                    // キャッシュ要素の場合、GetSupportedPatternsは使用できない
                    // キャッシュされたパターンプロパティから推測する
                    var patterns = new List<string>();
                    
                    // パターンプロパティの存在をチェックして推測
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
                    // 非キャッシュ要素の場合、GetSupportedPatternsを使用
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
                HelpText = string.IsNullOrEmpty(element.Cached.HelpText) ? null : element.Cached.HelpText,
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
                    details.Toggle = new ToggleInfo
                    {
                        State = useCached ? togglePattern.Cached.ToggleState.ToString() : togglePattern.Current.ToggleState.ToString(),
                        CanToggle = true
                    };
                }

                // Selection Pattern
                if ((useCached ? element.TryGetCachedPattern(SelectionPattern.Pattern, out var selectionPatternObj) : element.TryGetCurrentPattern(SelectionPattern.Pattern, out selectionPatternObj)) && 
                    selectionPatternObj is SelectionPattern selectionPattern)
                {
                    details.Selection = new SelectionInfo
                    {
                        CanSelectMultiple = useCached ? selectionPattern.Cached.CanSelectMultiple : selectionPattern.Current.CanSelectMultiple,
                        IsSelectionRequired = useCached ? selectionPattern.Cached.IsSelectionRequired : selectionPattern.Current.IsSelectionRequired
                    };
                }

                // Range Pattern
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

                // Grid Pattern
                if ((useCached ? element.TryGetCachedPattern(GridPattern.Pattern, out var gridPatternObj) : element.TryGetCurrentPattern(GridPattern.Pattern, out gridPatternObj)) && 
                    gridPatternObj is GridPattern gridPattern)
                {
                    details.Grid = new GridInfo
                    {
                        RowCount = useCached ? gridPattern.Cached.RowCount : gridPattern.Current.RowCount,
                        ColumnCount = useCached ? gridPattern.Cached.ColumnCount : gridPattern.Current.ColumnCount
                    };
                }

                // Table Pattern
                if ((useCached ? element.TryGetCachedPattern(TablePattern.Pattern, out var tablePatternObj) : element.TryGetCurrentPattern(TablePattern.Pattern, out tablePatternObj)) && 
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

                // Transform Pattern
                if ((useCached ? element.TryGetCachedPattern(TransformPattern.Pattern, out var transformPatternObj) : element.TryGetCurrentPattern(TransformPattern.Pattern, out transformPatternObj)) && 
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
                if ((useCached ? element.TryGetCachedPattern(WindowPattern.Pattern, out var windowPatternObj) : element.TryGetCurrentPattern(WindowPattern.Pattern, out windowPatternObj)) && 
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
                if ((useCached ? element.TryGetCachedPattern(ExpandCollapsePattern.Pattern, out var expandCollapsePatternObj) : element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out expandCollapsePatternObj)) && 
                    expandCollapsePatternObj is ExpandCollapsePattern expandCollapsePattern)
                {
                    details.ExpandCollapse = new ExpandCollapseInfo
                    {
                        State = useCached ? expandCollapsePattern.Cached.ExpandCollapseState.ToString() : expandCollapsePattern.Current.ExpandCollapseState.ToString()
                    };
                }

                // Dock Pattern
                if ((useCached ? element.TryGetCachedPattern(DockPattern.Pattern, out var dockPatternObj) : element.TryGetCurrentPattern(DockPattern.Pattern, out dockPatternObj)) && 
                    dockPatternObj is DockPattern dockPattern)
                {
                    details.Dock = new DockInfo
                    {
                        Position = useCached ? dockPattern.Cached.DockPosition.ToString() : dockPattern.Current.DockPosition.ToString()
                    };
                }

                // MultipleView Pattern
                if ((useCached ? element.TryGetCachedPattern(MultipleViewPattern.Pattern, out var multipleViewPatternObj) : element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out multipleViewPatternObj)) && 
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
                if ((useCached ? element.TryGetCachedPattern(TextPattern.Pattern, out var textPatternObj) : element.TryGetCurrentPattern(TextPattern.Pattern, out textPatternObj)) && 
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
                if ((useCached ? element.TryGetCachedPattern(GridItemPattern.Pattern, out var gridItemPatternObj) : element.TryGetCurrentPattern(GridItemPattern.Pattern, out gridItemPatternObj)) && 
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
                if ((useCached ? element.TryGetCachedPattern(TableItemPattern.Pattern, out var tableItemPatternObj) : element.TryGetCurrentPattern(TableItemPattern.Pattern, out tableItemPatternObj)) && 
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
                if ((useCached ? element.TryGetCachedPattern(InvokePattern.Pattern, out var invokePatternObj) : element.TryGetCurrentPattern(InvokePattern.Pattern, out invokePatternObj)) && 
                    invokePatternObj is InvokePattern)
                {
                    details.Invoke = new InvokeInfo
                    {
                        IsInvokable = true
                    };
                }

                // ScrollItem Pattern  
                if ((useCached ? element.TryGetCachedPattern(ScrollItemPattern.Pattern, out var scrollItemPatternObj) : element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out scrollItemPatternObj)) && 
                    scrollItemPatternObj is ScrollItemPattern)
                {
                    details.ScrollItem = new ScrollItemInfo
                    {
                        IsScrollable = true
                    };
                }

                // VirtualizedItem Pattern
                if ((useCached ? element.TryGetCachedPattern(VirtualizedItemPattern.Pattern, out var virtualizedItemPatternObj) : element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out virtualizedItemPatternObj)) && 
                    virtualizedItemPatternObj is VirtualizedItemPattern)
                {
                    details.VirtualizedItem = new VirtualizedItemInfo
                    {
                        IsVirtualized = true
                    };
                }

                // ItemContainer Pattern
                if ((useCached ? element.TryGetCachedPattern(ItemContainerPattern.Pattern, out var itemContainerPatternObj) : element.TryGetCurrentPattern(ItemContainerPattern.Pattern, out itemContainerPatternObj)) && 
                    itemContainerPatternObj is ItemContainerPattern)
                {
                    details.ItemContainer = new ItemContainerInfo
                    {
                        IsItemContainer = true
                    };
                }

                // SynchronizedInput Pattern
                if ((useCached ? element.TryGetCachedPattern(SynchronizedInputPattern.Pattern, out var synchronizedInputPatternObj) : element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out synchronizedInputPatternObj)) && 
                    synchronizedInputPatternObj is SynchronizedInputPattern)
                {
                    details.SynchronizedInput = new SynchronizedInputInfo
                    {
                        SupportsSynchronizedInput = true
                    };
                }

                // Accessibility Information
                details.Accessibility = new AccessibilityInfo
                {
                    AcceleratorKey = useCached 
                        ? (string.IsNullOrEmpty(element.Cached.AcceleratorKey) ? null : element.Cached.AcceleratorKey)
                        : (string.IsNullOrEmpty(element.Current.AcceleratorKey) ? null : element.Current.AcceleratorKey),
                    AccessKey = useCached 
                        ? (string.IsNullOrEmpty(element.Cached.AccessKey) ? null : element.Cached.AccessKey)
                        : (string.IsNullOrEmpty(element.Current.AccessKey) ? null : element.Current.AccessKey),
                    HelpText = useCached 
                        ? (string.IsNullOrEmpty(element.Cached.HelpText) ? null : element.Cached.HelpText)
                        : (string.IsNullOrEmpty(element.Current.HelpText) ? null : element.Current.HelpText)
                };

            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to retrieve pattern information for element");
            }
        }
    }
}