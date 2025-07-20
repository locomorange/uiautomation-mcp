using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// UI Automationパターン固有の情報を抽出するヘルパークラス
    /// 各GetXXXツール相当の機能を提供
    /// </summary>
    public class ElementPatternExtractor
    {
        private readonly ILogger _logger;

        public ElementPatternExtractor(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// すべてのパターン情報を辞書として取得
        /// </summary>
        public Dictionary<string, object> ExtractAllPatternInfo(AutomationElement element)
        {
            var patterns = new Dictionary<string, object>();

            try
            {
                // Value関連情報
                var valueInfo = GetValueInfo(element);
                if (valueInfo != null)
                {
                    patterns["Value"] = valueInfo;
                }

                // Toggle関連情報 (GetToggleState相当)
                var toggleInfo = GetToggleInfo(element);
                if (toggleInfo != null)
                {
                    patterns["Toggle"] = toggleInfo;
                }
                
                // Range関連情報 (GetRangeValue相当)
                var rangeInfo = GetRangeInfo(element);
                if (rangeInfo != null)
                {
                    patterns["Range"] = rangeInfo;
                }
                
                // Scroll関連情報 (GetScrollInfo相当)
                var scrollInfo = GetScrollInfo(element);
                if (scrollInfo != null)
                {
                    patterns["Scroll"] = scrollInfo;
                }
                
                // Transform関連情報 (GetTransformCapabilities相当)
                var transformInfo = GetTransformInfo(element);
                if (transformInfo != null)
                {
                    patterns["Transform"] = transformInfo;
                }
                
                // Grid関連情報 (GetGridInfo相当)
                var gridInfo = GetGridInfo(element);
                if (gridInfo != null)
                {
                    patterns["Grid"] = gridInfo;
                }
                
                // Selection関連情報 (GetSelection相当)
                var selectionInfo = GetSelectionInfo(element);
                if (selectionInfo != null)
                {
                    patterns["Selection"] = selectionInfo;
                }
                
                // Text関連情報 (GetText相当)
                var textInfo = GetTextInfo(element);
                if (textInfo != null)
                {
                    patterns["Text"] = textInfo;
                }

                // ExpandCollapse関連情報 (GetExpandCollapseState相当)
                var expandCollapseInfo = GetExpandCollapseInfo(element);
                if (expandCollapseInfo != null)
                {
                    patterns["ExpandCollapse"] = expandCollapseInfo;
                }

                // Dock関連情報 (GetDockPosition相当)
                var dockInfo = GetDockInfo(element);
                if (dockInfo != null)
                {
                    patterns["Dock"] = dockInfo;
                }

                // Window関連情報 (GetWindowInteractionState, GetWindowCapabilities相当)
                var windowInfo = GetWindowInfo(element);
                if (windowInfo != null)
                {
                    patterns["Window"] = windowInfo;
                }

                // MultipleView関連情報 (GetAvailableViews, GetCurrentView相当)
                var multipleViewInfo = GetMultipleViewInfo(element);
                if (multipleViewInfo != null)
                {
                    patterns["MultipleView"] = multipleViewInfo;
                }

                // GridItem関連情報 (GetGridItem位置情報相当)
                var gridItemInfo = GetGridItemInfo(element);
                if (gridItemInfo != null)
                {
                    patterns["GridItem"] = gridItemInfo;
                }

                // TableItem関連情報 (GetColumnHeaderItems, GetRowHeaderItems相当)
                var tableItemInfo = GetTableItemInfo(element);
                if (tableItemInfo != null)
                {
                    patterns["TableItem"] = tableItemInfo;
                }

                // SelectionItem関連情報 (IsElementSelected, GetSelectionContainer相当)
                var selectionItemInfo = GetSelectionItemInfo(element);
                if (selectionItemInfo != null)
                {
                    patterns["SelectionItem"] = selectionItemInfo;
                }

                // ScrollItem関連情報 (ScrollItemPattern状態)
                var scrollItemInfo = GetScrollItemInfo(element);
                if (scrollItemInfo != null)
                {
                    patterns["ScrollItem"] = scrollItemInfo;
                }

                // Invoke関連情報 (InvokePattern可用性)
                var invokeInfo = GetInvokeInfo(element);
                if (invokeInfo != null)
                {
                    patterns["Invoke"] = invokeInfo;
                }

                // VirtualizedItem関連情報 (VirtualizedItemPattern状態)
                var virtualizedItemInfo = GetVirtualizedItemInfo(element);
                if (virtualizedItemInfo != null)
                {
                    patterns["VirtualizedItem"] = virtualizedItemInfo;
                }

                // ItemContainer関連情報 (ItemContainerPattern機能)
                var itemContainerInfo = GetItemContainerInfo(element);
                if (itemContainerInfo != null)
                {
                    patterns["ItemContainer"] = itemContainerInfo;
                }

                // SynchronizedInput関連情報 (SynchronizedInputPattern状態)
                var synchronizedInputInfo = GetSynchronizedInputInfo(element);
                if (synchronizedInputInfo != null)
                {
                    patterns["SynchronizedInput"] = synchronizedInputInfo;
                }

                // Accessibility関連情報 (GetLabeledBy, GetDescribedBy相当)
                var accessibilityInfo = GetAccessibilityInfo(element);
                if (accessibilityInfo != null)
                {
                    patterns["Accessibility"] = accessibilityInfo;
                }

                return patterns;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ElementPatternExtractor] Failed to extract pattern information");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// 詳細な値情報を取得
        /// </summary>
        public Dictionary<string, object>? GetValueInfo(AutomationElement element)
        {
            try
            {
                var valueInfo = new Dictionary<string, object>();

                // ValuePattern を試行
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePatternObj) && 
                    valuePatternObj is ValuePattern valuePattern)
                {
                    valueInfo["Value"] = valuePattern.Current.Value ?? "";
                    valueInfo["IsReadOnly"] = valuePattern.Current.IsReadOnly;
                    valueInfo["PatternType"] = "ValuePattern";
                }
                // TextPattern を試行
                else if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPatternObj) && 
                         textPatternObj is TextPattern textPattern)
                {
                    var text = textPattern.DocumentRange.GetText(-1);
                    valueInfo["Text"] = text ?? "";
                    valueInfo["PatternType"] = "TextPattern";
                    
                    // DocumentRange情報
                    var docRange = textPattern.DocumentRange;
                    valueInfo["DocumentRangeLength"] = text?.Length ?? 0;
                }
                // Note: LegacyIAccessiblePattern は現在の環境では利用できない
                else
                {
                    // パターンが利用できない場合はNameプロパティを使用
                    valueInfo["Value"] = element.Current.Name ?? "";
                    valueInfo["PatternType"] = "Name";
                }

                return valueInfo.Count > 0 ? valueInfo : null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get value info");
                return null;
            }
        }

        /// <summary>
        /// GetToggleState操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetToggleInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(TogglePattern.Pattern, out var patternObj) || 
                    patternObj is not TogglePattern togglePattern)
                {
                    return null;
                }

                return new Dictionary<string, object>
                {
                    ["State"] = togglePattern.Current.ToggleState.ToString(),
                    ["PatternType"] = "TogglePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get toggle info");
                return null;
            }
        }

        /// <summary>
        /// GetRangeValue操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetRangeInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var patternObj) || 
                    patternObj is not RangeValuePattern rangePattern)
                {
                    return null;
                }

                return new Dictionary<string, object>
                {
                    ["Value"] = rangePattern.Current.Value,
                    ["Minimum"] = rangePattern.Current.Minimum,
                    ["Maximum"] = rangePattern.Current.Maximum,
                    ["SmallChange"] = rangePattern.Current.SmallChange,
                    ["LargeChange"] = rangePattern.Current.LargeChange,
                    ["IsReadOnly"] = rangePattern.Current.IsReadOnly,
                    ["PatternType"] = "RangeValuePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get range info");
                return null;
            }
        }

        /// <summary>
        /// GetScrollInfo操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetScrollInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(ScrollPattern.Pattern, out var patternObj) || 
                    patternObj is not ScrollPattern scrollPattern)
                {
                    return null;
                }

                return new Dictionary<string, object>
                {
                    ["HorizontalScrollPercent"] = scrollPattern.Current.HorizontalScrollPercent,
                    ["VerticalScrollPercent"] = scrollPattern.Current.VerticalScrollPercent,
                    ["HorizontalViewSize"] = scrollPattern.Current.HorizontalViewSize,
                    ["VerticalViewSize"] = scrollPattern.Current.VerticalViewSize,
                    ["HorizontallyScrollable"] = scrollPattern.Current.HorizontallyScrollable,
                    ["VerticallyScrollable"] = scrollPattern.Current.VerticallyScrollable,
                    ["PatternType"] = "ScrollPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get scroll info");
                return null;
            }
        }

        /// <summary>
        /// GetTransformCapabilities操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetTransformInfo(AutomationElement element)
        {
            try
            {
                var transformInfo = new Dictionary<string, object>();
                var boundingRect = element.Current.BoundingRectangle;

                if (element.TryGetCurrentPattern(TransformPattern.Pattern, out var patternObj) && 
                    patternObj is TransformPattern transformPattern)
                {
                    transformInfo["CanMove"] = transformPattern.Current.CanMove;
                    transformInfo["CanResize"] = transformPattern.Current.CanResize;
                    transformInfo["CanRotate"] = transformPattern.Current.CanRotate;
                    transformInfo["PatternType"] = "TransformPattern";
                }
                else
                {
                    transformInfo["CanMove"] = false;
                    transformInfo["CanResize"] = false;
                    transformInfo["CanRotate"] = false;
                    transformInfo["PatternType"] = "None";
                }

                transformInfo["CurrentX"] = boundingRect.X;
                transformInfo["CurrentY"] = boundingRect.Y;
                transformInfo["CurrentWidth"] = boundingRect.Width;
                transformInfo["CurrentHeight"] = boundingRect.Height;

                return transformInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get transform info");
                return null;
            }
        }

        /// <summary>
        /// GetGridInfo操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetGridInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(GridPattern.Pattern, out var patternObj) || 
                    patternObj is not GridPattern gridPattern)
                {
                    return null;
                }

                var gridInfo = new Dictionary<string, object>
                {
                    ["RowCount"] = gridPattern.Current.RowCount,
                    ["ColumnCount"] = gridPattern.Current.ColumnCount,
                    ["PatternType"] = "GridPattern"
                };

                // SelectionPatternもチェック
                if (element.TryGetCurrentPattern(SelectionPattern.Pattern, out var selPatternObj) && 
                    selPatternObj is SelectionPattern selectionPattern)
                {
                    gridInfo["CanSelectMultiple"] = selectionPattern.Current.CanSelectMultiple;
                }

                return gridInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get grid info");
                return null;
            }
        }

        /// <summary>
        /// GetSelection操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetSelectionInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(SelectionPattern.Pattern, out var patternObj) || 
                    patternObj is not SelectionPattern selectionPattern)
                {
                    return null;
                }

                var selectionInfo = new Dictionary<string, object>
                {
                    ["CanSelectMultiple"] = selectionPattern.Current.CanSelectMultiple,
                    ["IsSelectionRequired"] = selectionPattern.Current.IsSelectionRequired,
                    ["PatternType"] = "SelectionPattern"
                };

                var selectedItems = new List<Dictionary<string, object>>();
                var selection = selectionPattern.Current.GetSelection();
                foreach (AutomationElement selectedElement in selection)
                {
                    if (selectedElement != null)
                    {
                        selectedItems.Add(new Dictionary<string, object>
                        {
                            ["AutomationId"] = selectedElement.Current.AutomationId ?? "",
                            ["Name"] = selectedElement.Current.Name ?? "",
                            ["ControlType"] = selectedElement.Current.ControlType?.LocalizedControlType ?? ""
                        });
                    }
                }

                selectionInfo["SelectedItems"] = selectedItems;
                selectionInfo["SelectedCount"] = selectedItems.Count;

                return selectionInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get selection info");
                return null;
            }
        }

        /// <summary>
        /// GetText操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetTextInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObj) || 
                    patternObj is not TextPattern textPattern)
                {
                    return null;
                }

                var textInfo = new Dictionary<string, object>
                {
                    ["PatternType"] = "TextPattern"
                };

                var documentRange = textPattern.DocumentRange;
                var text = documentRange.GetText(-1);
                
                textInfo["Text"] = text ?? "";
                textInfo["Length"] = text?.Length ?? 0;

                // Selection情報
                var selections = textPattern.GetSelection();
                if (selections.Length > 0)
                {
                    var selectionText = selections[0].GetText(-1);
                    textInfo["SelectedText"] = selectionText ?? "";
                    textInfo["HasSelection"] = !string.IsNullOrEmpty(selectionText);
                }
                else
                {
                    textInfo["SelectedText"] = "";
                    textInfo["HasSelection"] = false;
                }

                return textInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get text info");
                return null;
            }
        }

        /// <summary>
        /// GetExpandCollapseState操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetExpandCollapseInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out var patternObj) || 
                    patternObj is not ExpandCollapsePattern expandCollapsePattern)
                {
                    return null;
                }

                return new Dictionary<string, object>
                {
                    ["State"] = expandCollapsePattern.Current.ExpandCollapseState.ToString(),
                    ["PatternType"] = "ExpandCollapsePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get expand collapse info");
                return null;
            }
        }

        /// <summary>
        /// GetDockPosition操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetDockInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(DockPattern.Pattern, out var patternObj) || 
                    patternObj is not DockPattern dockPattern)
                {
                    return null;
                }

                return new Dictionary<string, object>
                {
                    ["Position"] = dockPattern.Current.DockPosition.ToString(),
                    ["PatternType"] = "DockPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get dock info");
                return null;
            }
        }

        /// <summary>
        /// GetWindowInteractionState, GetWindowCapabilities操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetWindowInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(WindowPattern.Pattern, out var patternObj) || 
                    patternObj is not WindowPattern windowPattern)
                {
                    return null;
                }

                return new Dictionary<string, object>
                {
                    ["CanMaximize"] = windowPattern.Current.CanMaximize,
                    ["CanMinimize"] = windowPattern.Current.CanMinimize,
                    ["IsModal"] = windowPattern.Current.IsModal,
                    ["IsTopmost"] = windowPattern.Current.IsTopmost,
                    ["WindowInteractionState"] = windowPattern.Current.WindowInteractionState.ToString(),
                    ["WindowVisualState"] = windowPattern.Current.WindowVisualState.ToString(),
                    ["PatternType"] = "WindowPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get window info");
                return null;
            }
        }

        /// <summary>
        /// GetAvailableViews, GetCurrentView操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetMultipleViewInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(MultipleViewPattern.Pattern, out var patternObj) || 
                    patternObj is not MultipleViewPattern multipleViewPattern)
                {
                    return null;
                }

                var availableViews = new List<Dictionary<string, object>>();
                var viewIds = multipleViewPattern.Current.GetSupportedViews();
                
                foreach (int viewId in viewIds)
                {
                    try
                    {
                        var viewName = multipleViewPattern.GetViewName(viewId);
                        availableViews.Add(new Dictionary<string, object>
                        {
                            ["ViewId"] = viewId,
                            ["ViewName"] = viewName ?? $"View {viewId}"
                        });
                    }
                    catch
                    {
                        // Skip views that can't be queried
                        availableViews.Add(new Dictionary<string, object>
                        {
                            ["ViewId"] = viewId,
                            ["ViewName"] = $"View {viewId}"
                        });
                    }
                }

                return new Dictionary<string, object>
                {
                    ["CurrentView"] = multipleViewPattern.Current.CurrentView,
                    ["AvailableViews"] = availableViews,
                    ["PatternType"] = "MultipleViewPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get multiple view info");
                return null;
            }
        }

        /// <summary>
        /// GetGridItem位置情報操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetGridItemInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(GridItemPattern.Pattern, out var patternObj) || 
                    patternObj is not GridItemPattern gridItemPattern)
                {
                    return null;
                }

                var gridItemInfo = new Dictionary<string, object>
                {
                    ["Row"] = gridItemPattern.Current.Row,
                    ["Column"] = gridItemPattern.Current.Column,
                    ["RowSpan"] = gridItemPattern.Current.RowSpan,
                    ["ColumnSpan"] = gridItemPattern.Current.ColumnSpan,
                    ["PatternType"] = "GridItemPattern"
                };

                // ContainingGridの情報も追加
                var containingGrid = gridItemPattern.Current.ContainingGrid;
                if (containingGrid != null)
                {
                    gridItemInfo["ContainingGridId"] = containingGrid.Current.AutomationId ?? "";
                    gridItemInfo["ContainingGridName"] = containingGrid.Current.Name ?? "";
                }

                return gridItemInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get grid item info");
                return null;
            }
        }

        /// <summary>
        /// GetColumnHeaderItems, GetRowHeaderItems操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetTableItemInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(TableItemPattern.Pattern, out var patternObj) || 
                    patternObj is not TableItemPattern tableItemPattern)
                {
                    return null;
                }

                var tableItemInfo = new Dictionary<string, object>
                {
                    ["PatternType"] = "TableItemPattern"
                };

                // Column header items
                var columnHeaders = new List<Dictionary<string, object>>();
                var columnHeaderElements = tableItemPattern.Current.GetColumnHeaderItems();
                foreach (AutomationElement header in columnHeaderElements)
                {
                    if (header != null)
                    {
                        columnHeaders.Add(new Dictionary<string, object>
                        {
                            ["AutomationId"] = header.Current.AutomationId ?? "",
                            ["Name"] = header.Current.Name ?? "",
                            ["ControlType"] = header.Current.ControlType?.LocalizedControlType ?? ""
                        });
                    }
                }
                tableItemInfo["ColumnHeaders"] = columnHeaders;

                // Row header items
                var rowHeaders = new List<Dictionary<string, object>>();
                var rowHeaderElements = tableItemPattern.Current.GetRowHeaderItems();
                foreach (AutomationElement header in rowHeaderElements)
                {
                    if (header != null)
                    {
                        rowHeaders.Add(new Dictionary<string, object>
                        {
                            ["AutomationId"] = header.Current.AutomationId ?? "",
                            ["Name"] = header.Current.Name ?? "",
                            ["ControlType"] = header.Current.ControlType?.LocalizedControlType ?? ""
                        });
                    }
                }
                tableItemInfo["RowHeaders"] = rowHeaders;

                return tableItemInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get table item info");
                return null;
            }
        }

        /// <summary>
        /// IsElementSelected, GetSelectionContainer操作相当の情報を取得
        /// </summary>
        public Dictionary<string, object>? GetSelectionItemInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var patternObj) || 
                    patternObj is not SelectionItemPattern selectionItemPattern)
                {
                    return null;
                }

                var selectionItemInfo = new Dictionary<string, object>
                {
                    ["IsSelected"] = selectionItemPattern.Current.IsSelected,
                    ["PatternType"] = "SelectionItemPattern"
                };

                // SelectionContainer情報
                var selectionContainer = selectionItemPattern.Current.SelectionContainer;
                if (selectionContainer != null)
                {
                    selectionItemInfo["SelectionContainerId"] = selectionContainer.Current.AutomationId ?? "";
                    selectionItemInfo["SelectionContainerName"] = selectionContainer.Current.Name ?? "";
                    selectionItemInfo["SelectionContainerType"] = selectionContainer.Current.ControlType?.LocalizedControlType ?? "";
                }

                return selectionItemInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get selection item info");
                return null;
            }
        }

        /// <summary>
        /// ScrollItemPattern状態情報を取得
        /// </summary>
        public Dictionary<string, object>? GetScrollItemInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out var patternObj))
                {
                    return null;
                }

                // ScrollItemPatternには状態プロパティがないが、パターンの可用性を返す
                return new Dictionary<string, object>
                {
                    ["IsScrollable"] = true,
                    ["PatternType"] = "ScrollItemPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get scroll item info");
                return null;
            }
        }

        /// <summary>
        /// InvokePattern可用性情報を取得
        /// </summary>
        public Dictionary<string, object>? GetInvokeInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out var patternObj))
                {
                    return null;
                }

                // InvokePatternには状態プロパティがないが、パターンの可用性を返す
                return new Dictionary<string, object>
                {
                    ["IsInvokable"] = true,
                    ["PatternType"] = "InvokePattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get invoke info");
                return null;
            }
        }

        /// <summary>
        /// VirtualizedItemPattern状態情報を取得
        /// </summary>
        public Dictionary<string, object>? GetVirtualizedItemInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(VirtualizedItemPattern.Pattern, out var patternObj))
                {
                    return null;
                }

                // VirtualizedItemPatternには状態プロパティがないが、パターンの可用性を返す
                return new Dictionary<string, object>
                {
                    ["IsVirtualized"] = true,
                    ["PatternType"] = "VirtualizedItemPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get virtualized item info");
                return null;
            }
        }

        /// <summary>
        /// ItemContainerPattern機能情報を取得
        /// </summary>
        public Dictionary<string, object>? GetItemContainerInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(ItemContainerPattern.Pattern, out var patternObj))
                {
                    return null;
                }

                // ItemContainerPatternには状態プロパティがないが、パターンの可用性を返す
                return new Dictionary<string, object>
                {
                    ["IsItemContainer"] = true,
                    ["PatternType"] = "ItemContainerPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get item container info");
                return null;
            }
        }

        /// <summary>
        /// SynchronizedInputPattern状態情報を取得
        /// </summary>
        public Dictionary<string, object>? GetSynchronizedInputInfo(AutomationElement element)
        {
            try
            {
                if (!element.TryGetCurrentPattern(SynchronizedInputPattern.Pattern, out var patternObj))
                {
                    return null;
                }

                // SynchronizedInputPatternには状態プロパティがないが、パターンの可用性を返す
                return new Dictionary<string, object>
                {
                    ["SupportsSynchronizedInput"] = true,
                    ["PatternType"] = "SynchronizedInputPattern"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get synchronized input info");
                return null;
            }
        }

        /// <summary>
        /// GetLabeledBy, GetDescribedBy相当のアクセシビリティ情報を取得
        /// </summary>
        public Dictionary<string, object>? GetAccessibilityInfo(AutomationElement element)
        {
            try
            {
                var accessibilityInfo = new Dictionary<string, object>();
                bool hasAccessibilityInfo = false;

                // LabeledBy情報
                try
                {
                    var labeledByProperty = element.GetCurrentPropertyValue(AutomationElement.LabeledByProperty);
                    if (labeledByProperty is AutomationElement labeledByElement && labeledByElement != null)
                    {
                        accessibilityInfo["LabeledBy"] = new Dictionary<string, object>
                        {
                            ["AutomationId"] = labeledByElement.Current.AutomationId ?? "",
                            ["Name"] = labeledByElement.Current.Name ?? "",
                            ["ControlType"] = labeledByElement.Current.ControlType?.LocalizedControlType ?? ""
                        };
                        hasAccessibilityInfo = true;
                    }
                }
                catch
                {
                    // LabeledBy取得失敗は無視
                }

                // DescribedBy情報 - .NET実装では利用できない場合がある
                // HelpTextプロパティで代替
                try
                {
                    var helpText = element.Current.HelpText;
                    if (!string.IsNullOrEmpty(helpText))
                    {
                        accessibilityInfo["HelpText"] = helpText;
                        hasAccessibilityInfo = true;
                    }
                }
                catch
                {
                    // HelpText取得失敗は無視
                }

                // アクセシビリティの基本情報
                try
                {
                    var accessKey = element.Current.AccessKey;
                    var acceleratorKey = element.Current.AcceleratorKey;
                    
                    if (!string.IsNullOrEmpty(accessKey))
                    {
                        accessibilityInfo["AccessKey"] = accessKey;
                        hasAccessibilityInfo = true;
                    }
                    
                    if (!string.IsNullOrEmpty(acceleratorKey))
                    {
                        accessibilityInfo["AcceleratorKey"] = acceleratorKey;
                        hasAccessibilityInfo = true;
                    }
                }
                catch
                {
                    // アクセシビリティキー取得失敗は無視
                }

                if (hasAccessibilityInfo)
                {
                    accessibilityInfo["PatternType"] = "AccessibilityProperties";
                    return accessibilityInfo;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementPatternExtractor] Failed to get accessibility info");
                return null;
            }
        }
    }
}