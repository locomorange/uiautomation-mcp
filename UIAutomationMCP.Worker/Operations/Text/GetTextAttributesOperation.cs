using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Shared;
using UIAutomationMCP.Shared.Results;
using UIAutomationMCP.Shared.Requests;
using UIAutomationMCP.Shared.Serialization;
using UIAutomationMCP.Worker.Contracts;
using UIAutomationMCP.Worker.Helpers;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetTextAttributesOperation : IUIAutomationOperation
    {
        private readonly ElementFinderService _elementFinderService;
        private readonly ILogger<GetTextAttributesOperation> _logger;

        public GetTextAttributesOperation(
            ElementFinderService elementFinderService,
            ILogger<GetTextAttributesOperation> logger)
        {
            _elementFinderService = elementFinderService;
            _logger = logger;
        }

        public Task<OperationResult> ExecuteAsync(string parametersJson)
        {
            try
            {
                var request = JsonSerializationHelper.Deserialize<GetTextAttributesRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: request.AutomationId, 
                    name: request.Name,
                    controlType: request.ControlType,
                    processId: request.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(CreateErrorResult(request, "Element not found"));
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    _logger.LogWarning("Element does not support TextPattern: {AutomationId}", request.AutomationId);
                    return Task.FromResult(CreateErrorResult(request, "Element does not support TextPattern"));
                }

                var result = ExtractTextAttributes(textPattern, request);
                return Task.FromResult(new OperationResult { Success = true, Data = result });
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                _logger.LogError(ex, "COM error during text attributes operation");
                return Task.FromResult(CreateErrorResult(null, $"UI Automation error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTextAttributes operation failed");
                return Task.FromResult(CreateErrorResult(null, $"Operation failed: {ex.Message}"));
            }
        }

        private OperationResult CreateErrorResult(GetTextAttributesRequest? request, string error)
        {
            return new OperationResult
            {
                Success = false,
                Error = error,
                Data = new TextAttributesResult
                {
                    AutomationId = request?.AutomationId ?? "",
                    Name = request?.Name ?? "",
                    HasAttributes = false
                }
            };
        }

        private TextAttributesResult ExtractTextAttributes(TextPattern textPattern, GetTextAttributesRequest request)
        {
            var documentRange = textPattern.DocumentRange;
            if (documentRange == null)
            {
                throw new InvalidOperationException("Cannot access text content - element may not contain text");
            }

            var fullText = documentRange.GetText(-1);
            var (startIndex, endIndex) = CalculateTextRange(fullText, request.StartIndex, request.Length);
            
            if (startIndex >= fullText.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(request.StartIndex), "Start index is beyond text length");
            }

            var textRange = CreateTextRange(documentRange, startIndex, endIndex - startIndex);
            var attributes = GetTextAttributes(textRange);

            return new TextAttributesResult
            {
                Success = true,
                AutomationId = request.AutomationId ?? "",
                Name = request.Name ?? "",
                ControlType = request.ControlType ?? "",
                ProcessId = request.ProcessId ?? 0,
                StartPosition = startIndex,
                EndPosition = endIndex,
                TextContent = fullText.Substring(startIndex, endIndex - startIndex),
                HasAttributes = true,
                Pattern = "TextPattern",
                TextAttributes = attributes,
                SupportedAttributes = GetSupportedAttributes()
            };
        }

        private (int startIndex, int endIndex) CalculateTextRange(string fullText, int requestedStart, int requestedLength)
        {
            var startIndex = Math.Max(0, requestedStart);
            var length = requestedLength == -1 ? fullText.Length - startIndex : requestedLength;
            var endIndex = Math.Min(fullText.Length, startIndex + length);
            return (startIndex, endIndex);
        }

        private TextPatternRange CreateTextRange(TextPatternRange documentRange, int startIndex, int length)
        {
            var textRange = documentRange.Clone();
            textRange.Move(TextUnit.Character, startIndex);
            textRange.ExpandToEnclosingUnit(TextUnit.Character);
            textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);
            return textRange;
        }

        private TextAttributes GetTextAttributes(TextPatternRange textRange)
        {
            var attributes = new TextAttributes();

            try
            {
                // Font attributes
                attributes.FontName = GetAttributeValue<string>(textRange, TextPattern.FontNameAttribute);
                attributes.FontSize = GetAttributeValue<double?>(textRange, TextPattern.FontSizeAttribute);
                attributes.FontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute)?.ToString();
                attributes.IsItalic = GetAttributeValue<bool?>(textRange, TextPattern.IsItalicAttribute);
                
                // Colors - Microsoft Learn recommends checking for 0 as "not set"
                var foregroundColor = GetAttributeValue<int>(textRange, TextPattern.ForegroundColorAttribute);
                if (foregroundColor != 0)
                    attributes.ForegroundColor = $"#{foregroundColor:X6}";
                
                var backgroundColor = GetAttributeValue<int>(textRange, TextPattern.BackgroundColorAttribute);
                if (backgroundColor != 0)
                    attributes.BackgroundColor = $"#{backgroundColor:X6}";

                // Text decorations - check for "None" as per Microsoft guidance
                var underlineStyle = GetAttributeValue<object>(textRange, TextPattern.UnderlineStyleAttribute);
                attributes.IsUnderline = underlineStyle != null && !string.Equals(underlineStyle.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.UnderlineStyle = underlineStyle?.ToString();

                var strikethroughStyle = GetAttributeValue<object>(textRange, TextPattern.StrikethroughStyleAttribute);
                attributes.IsStrikethrough = strikethroughStyle != null && !string.Equals(strikethroughStyle.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.StrikethroughStyle = strikethroughStyle?.ToString();

                // Bold detection using FontWeight - simplified as per Microsoft guidance
                attributes.IsBold = DetermineBoldFromFontWeight(GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute));

                // Other attributes
                attributes.HorizontalTextAlignment = GetAttributeValue<object>(textRange, TextPattern.HorizontalTextAlignmentAttribute)?.ToString();
                attributes.Culture = GetAttributeValue<System.Globalization.CultureInfo>(textRange, TextPattern.CultureAttribute)?.Name;
                attributes.IsReadOnly = GetAttributeValue<bool?>(textRange, TextPattern.IsReadOnlyAttribute);
                attributes.IsHidden = GetAttributeValue<bool?>(textRange, TextPattern.IsHiddenAttribute);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get some text attributes");
            }

            return attributes;
        }

        private bool DetermineBoldFromFontWeight(object? fontWeight)
        {
            if (fontWeight == null) return false;

            var weightStr = fontWeight.ToString() ?? "";
            
            // Check for numeric values (700+ is bold as per Microsoft standards)
            if (int.TryParse(weightStr, out var numericWeight))
            {
                return numericWeight >= 700;
            }
            
            // Check for string patterns
            return weightStr.Contains("Bold", StringComparison.OrdinalIgnoreCase) || 
                   weightStr.Contains("Heavy", StringComparison.OrdinalIgnoreCase) ||
                   weightStr.Contains("700") || weightStr.Contains("800") || weightStr.Contains("900");
        }

        private List<string> GetSupportedAttributes()
        {
            return new List<string>
            {
                "FontName", "FontSize", "FontWeight", "IsItalic", "IsBold",
                "ForegroundColor", "BackgroundColor", "IsUnderline", "IsStrikethrough", 
                "HorizontalTextAlignment", "Culture", "IsReadOnly", "IsHidden"
            };
        }

        private T GetAttributeValue<T>(TextPatternRange textRange, AutomationTextAttribute attribute)
        {
            try
            {
                var value = textRange.GetAttributeValue(attribute);
                
                // Handle special cases like MixedAttributeValue and NotSupported
                if (ReferenceEquals(value, TextPattern.MixedAttributeValue) || 
                    ReferenceEquals(value, AutomationElement.NotSupported))
                {
                    return default(T)!;
                }
                
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                // Try conversion for numeric types
                if (typeof(T) == typeof(double) && value != null)
                {
                    if (double.TryParse(value.ToString(), out var doubleValue))
                    {
                        return (T)(object)doubleValue;
                    }
                }
                
                return default(T)!;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to get attribute {Attribute}", attribute.ProgrammaticName);
                return default(T)!;
            }
        }

    }
}