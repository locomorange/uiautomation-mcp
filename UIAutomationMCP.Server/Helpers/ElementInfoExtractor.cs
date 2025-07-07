using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Server.Models;

namespace UIAutomationMCP.Server.Helpers
{
    /// <summary>
    /// 要素情報の抽出を担当するヘルパークラス
    /// </summary>
    public class ElementInfoExtractor
    {
        private readonly ILogger<ElementInfoExtractor> _logger;

        public ElementInfoExtractor(ILogger<ElementInfoExtractor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// AutomationElementから詳細な要素情報を抽出します
        /// </summary>
        public ElementInfo ExtractElementInfo(AutomationElement element)
        {
            try
            {
                var info = new ElementInfo();

                // 基本プロパティの安全な取得
                info.Name = SafeGetProperty(() => element.Current.Name) ?? "";
                info.AutomationId = SafeGetProperty(() => element.Current.AutomationId) ?? "";
                info.ClassName = SafeGetProperty(() => element.Current.ClassName) ?? "";
                info.ControlType = SafeGetProperty(() => element.Current.ControlType?.ProgrammaticName) ?? "";
                info.ProcessId = SafeGetProperty(() => element.Current.ProcessId);
                info.IsEnabled = SafeGetProperty(() => element.Current.IsEnabled);
                info.IsVisible = SafeGetProperty(() => !element.Current.IsOffscreen);

                // BoundingRectangle の安全な取得
                var rect = SafeGetProperty(() => element.Current.BoundingRectangle);
                if (!rect.IsEmpty && rect.Width > 0 && rect.Height > 0)
                {
                    info.BoundingRectangle = new BoundingRectangle
                    {
                        X = SafeDoubleValue(rect.X),
                        Y = SafeDoubleValue(rect.Y),
                        Width = SafeDoubleValue(rect.Width),
                        Height = SafeDoubleValue(rect.Height)
                    };
                }
                else
                {
                    info.BoundingRectangle = new BoundingRectangle
                    {
                        X = 0.0,
                        Y = 0.0,
                        Width = 0.0,
                        Height = 0.0
                    };
                }

                // その他の有用なプロパティ
                info.HelpText = SafeGetProperty(() => element.Current.HelpText) ?? "";
                
                // Value プロパティを試行
                info.Value = TryGetElementValue(element);

                // AvailableActions を取得
                info.AvailableActions = GetAvailableActions(element);

                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementInfoExtractor] Failed to extract element info");
                
                // エラー時は最小限の情報を返す
                return new ElementInfo
                {
                    Name = "",
                    AutomationId = "",
                    ClassName = "",
                    ControlType = "",
                    ProcessId = 0,
                    IsEnabled = false,
                    IsVisible = false,
                    BoundingRectangle = new BoundingRectangle
                    {
                        X = 0.0,
                        Y = 0.0,
                        Width = 0.0,
                        Height = 0.0
                    }
                };
            }
        }

        /// <summary>
        /// プロパティの安全な取得（例外処理付き）
        /// </summary>
        private T? SafeGetProperty<T>(Func<T> getter)
        {
            try
            {
                return getter();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ElementInfoExtractor] Failed to get property");
                return default;
            }
        }

        /// <summary>
        /// double値の安全な変換（NaN/Infinityチェック付き）
        /// </summary>
        private double SafeDoubleValue(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < -1000000 || value > 1000000)
            {
                return 0.0;
            }
            return Math.Round(value, 2); // Round to 2 decimal places to avoid precision issues
        }

        /// <summary>
        /// 要素の値を取得（ValuePatternやTextPatternを使用）
        /// </summary>
        private string? TryGetElementValue(AutomationElement element)
        {
            try
            {
                // ValuePatternを試行
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePatternObj) && 
                    valuePatternObj is ValuePattern valuePattern)
                {
                    return valuePattern.Current.Value;
                }

                // TextPatternを試行
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var textPatternObj) && 
                    textPatternObj is TextPattern textPattern)
                {
                    return textPattern.DocumentRange.GetText(-1);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[ElementInfoExtractor] Failed to get element value");
                return null;
            }
        }

        /// <summary>
        /// 要素の利用可能なアクションを取得
        /// </summary>
        private Dictionary<string, string> GetAvailableActions(AutomationElement element)
        {
            var availableActions = new Dictionary<string, string>();

            try
            {
                var supportedPatterns = element.GetSupportedPatterns();

                foreach (var pattern in supportedPatterns)
                {
                    switch (pattern.ProgrammaticName)
                    {
                        case "InvokePatternIdentifiers.Pattern":
                            availableActions["invoke"] = "Click or activate this element";
                            break;

                        case "ValuePatternIdentifiers.Pattern":
                            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var valuePattern) && 
                                valuePattern is ValuePattern vp && !vp.Current.IsReadOnly)
                            {
                                availableActions["setValue"] = "Set text or value";
                            }
                            availableActions["getValue"] = "Get current value";
                            break;

                        case "TogglePatternIdentifiers.Pattern":
                            availableActions["toggle"] = "Toggle checkbox or toggle button state";
                            break;

                        case "SelectionItemPatternIdentifiers.Pattern":
                            availableActions["select"] = "Select this item";
                            break;

                        case "ExpandCollapsePatternIdentifiers.Pattern":
                            availableActions["expand"] = "Expand this element";
                            availableActions["collapse"] = "Collapse this element";
                            break;

                        case "ScrollPatternIdentifiers.Pattern":
                            availableActions["scroll"] = "Scroll this element";
                            break;

                        case "RangeValuePatternIdentifiers.Pattern":
                            if (element.TryGetCurrentPattern(RangeValuePattern.Pattern, out var rangePattern) && 
                                rangePattern is RangeValuePattern rvp && !rvp.Current.IsReadOnly)
                            {
                                availableActions["setRangeValue"] = "Set range value (slider, progress bar)";
                            }
                            availableActions["getRangeValue"] = "Get range value";
                            break;

                        case "WindowPatternIdentifiers.Pattern":
                            availableActions["windowAction"] = "Minimize, maximize, or close window";
                            break;

                        case "TextPatternIdentifiers.Pattern":
                            availableActions["getText"] = "Get text content";
                            availableActions["findText"] = "Find text in element";
                            availableActions["selectText"] = "Select text";
                            availableActions["setText"] = "Set text content";
                            break;

                        case "TransformPatternIdentifiers.Pattern":
                            availableActions["transform"] = "Move, resize, or rotate element";
                            break;

                        case "DockPatternIdentifiers.Pattern":
                            availableActions["dock"] = "Dock element to position";
                            break;

                        case "GridPatternIdentifiers.Pattern":
                            availableActions["getGridInfo"] = "Get grid information";
                            availableActions["getGridItem"] = "Get grid item at row/column";
                            break;

                        case "TablePatternIdentifiers.Pattern":
                            availableActions["getTableInfo"] = "Get table information";
                            availableActions["getColumnHeaders"] = "Get column headers";
                            availableActions["getRowHeaders"] = "Get row headers";
                            break;

                        case "MultipleViewPatternIdentifiers.Pattern":
                            availableActions["setView"] = "Set current view";
                            availableActions["getAvailableViews"] = "Get available views";
                            break;

                        case "SelectionPatternIdentifiers.Pattern":
                            availableActions["getSelection"] = "Get current selection";
                            break;

                        case "ScrollItemPatternIdentifiers.Pattern":
                            availableActions["scrollIntoView"] = "Scroll element into view";
                            break;
                    }
                }

                // 基本的なアクション（パターンに依存しない）
                availableActions["getElementInfo"] = "Get element information";
                availableActions["getElementTree"] = "Get element tree structure";
                availableActions["takeScreenshot"] = "Take screenshot";
                availableActions["findElements"] = "Find child elements";

                // アクセシビリティ関連
                availableActions["getAccessibilityInfo"] = "Get accessibility information";
                availableActions["verifyAccessibility"] = "Verify accessibility compliance";

                return availableActions;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ElementInfoExtractor] Failed to get available actions");
                return new Dictionary<string, string>();
            }
        }
    }
}