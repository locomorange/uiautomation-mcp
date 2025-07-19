using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;

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

                // SupportedPatterns を取得
                info.SupportedPatterns = GetSupportedPatterns(element);

                // Properties辞書にUI Automation固有プロパティを追加
                PopulatePropertiesDictionary(element, info);

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
        /// 要素のサポートされているパターンを取得
        /// </summary>
        private List<string> GetSupportedPatterns(AutomationElement element)
        {
            var supportedPatterns = new List<string>();

            try
            {
                var patterns = element.GetSupportedPatterns();

                foreach (var pattern in patterns)
                {
                    supportedPatterns.Add(pattern.ProgrammaticName);
                }

                return supportedPatterns;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ElementInfoExtractor] Failed to get supported patterns");
                return new List<string>();
            }
        }

        /// <summary>
        /// Properties辞書に追加のUI Automation固有プロパティを設定
        /// 基本プロパティ（Name, AutomationId等）は既にElementInfoに存在するため含めない
        /// </summary>
        private void PopulatePropertiesDictionary(AutomationElement element, ElementInfo info)
        {
            try
            {
                // ElementPatternExtractorを使用してパターン情報を取得
                var patternExtractor = new ElementPatternExtractor(_logger);
                var patternInfo = patternExtractor.ExtractAllPatternInfo(element);
                
                // パターン情報をPropertiesに追加
                foreach (var kvp in patternInfo)
                {
                    info.Properties[kvp.Key] = kvp.Value;
                }
                
                _logger.LogDebug($"[ElementInfoExtractor] Added {patternInfo.Count} pattern properties to element {info.AutomationId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ElementInfoExtractor] Failed to populate properties dictionary");
            }
        }
    }
}
