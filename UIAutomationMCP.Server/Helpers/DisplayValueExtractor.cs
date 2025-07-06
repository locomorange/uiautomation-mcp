using System.Windows.Automation;
using Microsoft.Extensions.Logging;
using UiAutomationMcpServer.Services;

namespace UiAutomationMcpServer.Helpers
{
    /// <summary>
    /// 電卓等の表示要素から値を取得するためのヘルパークラス
    /// ValuePatternが使用できない場合の代替手段を提供
    /// </summary>
    public class DisplayValueExtractor
    {
        private readonly ILogger<DisplayValueExtractor> _logger;

        public DisplayValueExtractor(ILogger<DisplayValueExtractor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 要素から表示値を取得（複数の方法を順次試行）
        /// </summary>
        public async Task<(bool Success, string? Value, string Method)> ExtractDisplayValueAsync(
            AutomationElement element, 
            CancellationToken cancellationToken = default)
        {
            if (element == null)
            {
                return (false, null, "ElementNull");
            }

            _logger.LogInformation("[DisplayValueExtractor] Attempting to extract value from element: {ElementName}", 
                element.Current.Name);

            // 方法1: ValuePattern
            var valueResult = await TryValuePatternAsync(element, cancellationToken);
            if (valueResult.Success)
            {
                return valueResult;
            }

            // 方法2: TextPattern
            var textResult = await TryTextPatternAsync(element, cancellationToken);
            if (textResult.Success)
            {
                return textResult;
            }

            // 方法3: Name Property
            var nameResult = await TryNamePropertyAsync(element, cancellationToken);
            if (nameResult.Success)
            {
                return nameResult;
            }

            // 方法4: 子要素から値を検索
            var childResult = await TryChildElementsAsync(element, cancellationToken);
            if (childResult.Success)
            {
                return childResult;
            }

            _logger.LogWarning("[DisplayValueExtractor] All value extraction methods failed for element: {ElementName}", 
                element.Current.Name);

            return (false, null, "AllMethodsFailed");
        }

        /// <summary>
        /// ValuePatternを使用して値を取得
        /// </summary>
        private Task<(bool Success, string? Value, string Method)> TryValuePatternAsync(
            AutomationElement element, 
            CancellationToken cancellationToken)
        {
            try
            {
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObject) && patternObject is ValuePattern valuePattern)
                {
                    var value = valuePattern.Current.Value;
                    _logger.LogDebug("[DisplayValueExtractor] ValuePattern successful: {Value}", value);
                    return Task.FromResult((true, value, "ValuePattern"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[DisplayValueExtractor] ValuePattern failed");
            }

            return Task.FromResult((false, (string?)null, "ValuePattern"));
        }

        /// <summary>
        /// TextPatternを使用して値を取得
        /// </summary>
        private Task<(bool Success, string? Value, string Method)> TryTextPatternAsync(
            AutomationElement element, 
            CancellationToken cancellationToken)
        {
            try
            {
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var patternObject) && patternObject is TextPattern textPattern)
                {
                    var text = textPattern.DocumentRange.GetText(-1);
                    _logger.LogDebug("[DisplayValueExtractor] TextPattern successful: {Text}", text);
                    return Task.FromResult((true, text, "TextPattern"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[DisplayValueExtractor] TextPattern failed");
            }

            return Task.FromResult((false, (string?)null, "TextPattern"));
        }

        /// <summary>
        /// Name プロパティから値を取得
        /// </summary>
        private Task<(bool Success, string? Value, string Method)> TryNamePropertyAsync(
            AutomationElement element, 
            CancellationToken cancellationToken)
        {
            try
            {
                var name = element.Current.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    _logger.LogDebug("[DisplayValueExtractor] Name property successful: {Name}", name);
                    return Task.FromResult((true, name, "NameProperty"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[DisplayValueExtractor] Name property failed");
            }

            return Task.FromResult((false, (string?)null, "NameProperty"));
        }


        /// <summary>
        /// 子要素から表示値を検索
        /// </summary>
        private async Task<(bool Success, string? Value, string Method)> TryChildElementsAsync(
            AutomationElement element, 
            CancellationToken cancellationToken)
        {
            try
            {
                var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);

                foreach (AutomationElement child in children)
                {
                    try
                    {
                        // Text要素を探す
                        var controlType = child.Current.ControlType;

                        if (controlType == ControlType.Text || controlType == ControlType.Edit)
                        {
                            var childResult = await ExtractDisplayValueAsync(child, cancellationToken);
                            if (childResult.Success)
                            {
                                _logger.LogDebug("[DisplayValueExtractor] Child element successful: {Value}", childResult.Value);
                                return (true, childResult.Value, $"ChildElement-{childResult.Method}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "[DisplayValueExtractor] Child element check failed");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[DisplayValueExtractor] Child elements search failed");
            }

            return (false, null, "ChildElements");
        }

        /// <summary>
        /// 電卓の表示値から数値を抽出（日本語表示対応）
        /// </summary>
        public string? ExtractNumericValue(string? displayText)
        {
            if (string.IsNullOrEmpty(displayText))
            {
                return null;
            }

            _logger.LogDebug("[DisplayValueExtractor] Extracting numeric value from: {DisplayText}", displayText);

            // 日本語の表示形式に対応
            // 例: "表示は 123 です" -> "123"
            var patterns = new[]
            {
                @"表示は\s*([+-]?[\d,.\s]+)\s*です",      // 日本語形式
                @"Display is\s*([+-]?[\d,.\s]+)",        // 英語形式
                @"^([+-]?[\d,.\s]+)$",                    // 数値のみ
                @"([+-]?[\d,.\s]+)",                      // 任意の位置の数値
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(displayText, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    var extractedValue = match.Groups[1].Value.Trim().Replace(" ", "").Replace(",", "");
                    _logger.LogDebug("[DisplayValueExtractor] Extracted numeric value: {Value}", extractedValue);
                    return extractedValue;
                }
            }

            // エラーメッセージの場合
            if (displayText.Contains("割ることはできません") || displayText.Contains("cannot divide"))
            {
                _logger.LogDebug("[DisplayValueExtractor] Division by zero error detected");
                return "Error: Division by zero";
            }

            if (displayText.Contains("エラー") || displayText.Contains("error"))
            {
                _logger.LogDebug("[DisplayValueExtractor] General error detected");
                return "Error";
            }

            _logger.LogWarning("[DisplayValueExtractor] Could not extract numeric value from: {DisplayText}", displayText);
            return displayText; // 元の文字列をそのまま返す
        }
    }
}