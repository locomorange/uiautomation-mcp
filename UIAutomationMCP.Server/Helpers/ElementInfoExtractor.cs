using System.Windows.Automation;
using Microsoft.Extensions.Logging;

namespace UiAutomationMcpServer.Helpers
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
        public Dictionary<string, object> ExtractElementInfo(AutomationElement element)
        {
            try
            {
                var info = new Dictionary<string, object>();

                // 基本プロパティの安全な取得
                info["Name"] = SafeGetProperty(() => element.Current.Name) ?? "";
                info["AutomationId"] = SafeGetProperty(() => element.Current.AutomationId) ?? "";
                info["ClassName"] = SafeGetProperty(() => element.Current.ClassName) ?? "";
                info["ControlType"] = SafeGetProperty(() => element.Current.ControlType?.ProgrammaticName) ?? "";
                info["ProcessId"] = SafeGetProperty(() => element.Current.ProcessId);
                info["IsEnabled"] = SafeGetProperty(() => element.Current.IsEnabled);
                info["IsVisible"] = SafeGetProperty(() => !element.Current.IsOffscreen);

                // BoundingRectangle の安全な取得
                var rect = SafeGetProperty(() => element.Current.BoundingRectangle);
                if (!rect.IsEmpty && rect.Width > 0 && rect.Height > 0)
                {
                    info["BoundingRectangle"] = new Dictionary<string, object>
                    {
                        ["X"] = SafeDoubleValue(rect.X),
                        ["Y"] = SafeDoubleValue(rect.Y),
                        ["Width"] = SafeDoubleValue(rect.Width),
                        ["Height"] = SafeDoubleValue(rect.Height)
                    };
                }
                else
                {
                    info["BoundingRectangle"] = new Dictionary<string, object>
                    {
                        ["X"] = 0.0,
                        ["Y"] = 0.0,
                        ["Width"] = 0.0,
                        ["Height"] = 0.0
                    };
                }

                // その他の有用なプロパティ
                info["LocalizedControlType"] = SafeGetProperty(() => element.Current.LocalizedControlType) ?? "";
                info["HelpText"] = SafeGetProperty(() => element.Current.HelpText) ?? "";
                info["AcceleratorKey"] = SafeGetProperty(() => element.Current.AcceleratorKey) ?? "";
                info["AccessKey"] = SafeGetProperty(() => element.Current.AccessKey) ?? "";

                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ElementInfoExtractor] Failed to extract element info");
                
                // エラー時は最小限の情報を返す
                return new Dictionary<string, object>
                {
                    ["Name"] = "",
                    ["AutomationId"] = "",
                    ["ClassName"] = "",
                    ["ControlType"] = "",
                    ["ProcessId"] = 0,
                    ["IsEnabled"] = false,
                    ["IsVisible"] = false,
                    ["BoundingRectangle"] = new Dictionary<string, object>
                    {
                        ["X"] = 0.0,
                        ["Y"] = 0.0,
                        ["Width"] = 0.0,
                        ["Height"] = 0.0
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
    }
}