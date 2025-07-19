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
                // Value関連情報 (GetElementValue相当)
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

                return patterns;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ElementPatternExtractor] Failed to extract pattern information");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// GetElementValue操作相当の詳細な値情報を取得
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
    }
}