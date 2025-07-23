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
                var typedRequest = JsonSerializationHelper.Deserialize<GetTextAttributesRequest>(parametersJson)!;
                
                var element = _elementFinderService.FindElement(
                    automationId: typedRequest.AutomationId, 
                    name: typedRequest.Name,
                    controlType: typedRequest.ControlType,
                    processId: typedRequest.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element not found",
                        Data = new TextAttributesResult 
                        { 
                            AutomationId = typedRequest.AutomationId ?? "",
                            Name = typedRequest.Name ?? "",
                            HasAttributes = false
                        }
                    });
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = "Element does not support TextPattern",
                        Data = new TextAttributesResult 
                        { 
                            AutomationId = typedRequest.AutomationId ?? "",
                            Name = typedRequest.Name ?? "",
                            HasAttributes = false
                        }
                    });
                }

                try
                {
                    var documentRange = textPattern.DocumentRange;
                    var fullText = documentRange.GetText(-1);
                    
                    // Determine the range to analyze
                    var startIndex = Math.Max(0, typedRequest.StartIndex);
                    var length = typedRequest.Length == -1 ? fullText.Length - startIndex : typedRequest.Length;
                    var endIndex = Math.Min(fullText.Length, startIndex + length);
                    
                    if (startIndex >= fullText.Length)
                    {
                        return Task.FromResult(new OperationResult 
                        { 
                            Success = false, 
                            Error = "Start index is beyond text length",
                            Data = new TextAttributesResult 
                            { 
                                AutomationId = typedRequest.AutomationId,
                                Name = typedRequest.Name,
                                HasAttributes = false
                            }
                        });
                    }

                    // Create a text range for the specified position and length
                    var textRange = documentRange.Clone();
                    textRange.Move(TextUnit.Character, startIndex);
                    textRange.ExpandToEnclosingUnit(TextUnit.Character);
                    textRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, length);

                    
                    var result = new TextAttributesResult
                    {
                        Success = true,
                        AutomationId = typedRequest.AutomationId ?? "",
                        Name = typedRequest.Name ?? "",
                        ControlType = typedRequest.ControlType ?? "",
                        ProcessId = typedRequest.ProcessId ?? 0,
                        StartPosition = startIndex,
                        EndPosition = endIndex,
                        TextContent = fullText.Substring(startIndex, endIndex - startIndex),
                        HasAttributes = true,
                        Pattern = "TextPattern"
                    };

                    // Get all supported attributes
                    var supportedAttributes = GetSupportedAttributes();
                    result.SupportedAttributes = supportedAttributes;

                    // If specific attribute requested, get just that one
                    if (!string.IsNullOrEmpty(typedRequest.AttributeName))
                    {
                        var attributeValue = GetSpecificAttribute(textRange, typedRequest.AttributeName);
                        if (attributeValue != null)
                        {
                            result.TextAttributes[typedRequest.AttributeName] = attributeValue;
                        }
                    }
                    else
                    {
                        // Get all attributes for the range
                        var attributeRange = GetAttributesForRange(textRange, startIndex, endIndex - startIndex);
                        result.AttributeRanges.Add(attributeRange);
                        result.TextRanges.Add(ConvertToTextRangeAttributes(attributeRange));
                    }

                    result.AttributeCount = result.TextAttributes.Count;

                    return Task.FromResult(new OperationResult 
                    { 
                        Success = true, 
                        Data = result
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get text attributes");
                    return Task.FromResult(new OperationResult 
                    { 
                        Success = false, 
                        Error = $"Failed to get text attributes: {ex.Message}",
                        Data = new TextAttributesResult 
                        { 
                            AutomationId = typedRequest.AutomationId ?? "",
                            Name = typedRequest.Name ?? "",
                            HasAttributes = false
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTextAttributes operation failed");
                return Task.FromResult(new OperationResult 
                { 
                    Success = false, 
                    Error = $"Operation failed: {ex.Message}",
                    Data = new TextAttributesResult { HasAttributes = false }
                });
            }
        }

        private List<string> GetSupportedAttributes()
        {
            return new List<string>
            {
                "FontName", "FontSize", "FontWeight", "FontStyle",
                "ForegroundColor", "BackgroundColor", "IsItalic", "IsBold",
                "IsUnderline", "IsStrikethrough", "HorizontalTextAlignment",
                "Culture", "IsReadOnly", "IsHidden"
            };
        }

        private object? GetSpecificAttribute(TextPatternRange textRange, string attributeName)
        {
            try
            {
                return attributeName.ToLowerInvariant() switch
                {
                    "fontname" => textRange.GetAttributeValue(TextPattern.FontNameAttribute),
                    "fontsize" => textRange.GetAttributeValue(TextPattern.FontSizeAttribute),
                    "fontweight" => textRange.GetAttributeValue(TextPattern.FontWeightAttribute),
                    "fontstyle" => textRange.GetAttributeValue(TextPattern.IsItalicAttribute),
                    "foregroundcolor" => textRange.GetAttributeValue(TextPattern.ForegroundColorAttribute),
                    "backgroundcolor" => textRange.GetAttributeValue(TextPattern.BackgroundColorAttribute),
                    "isitalic" => textRange.GetAttributeValue(TextPattern.IsItalicAttribute),
                    "isbold" => textRange.GetAttributeValue(TextPattern.FontWeightAttribute),
                    "isunderline" => textRange.GetAttributeValue(TextPattern.UnderlineStyleAttribute),
                    "isstrikethrough" => textRange.GetAttributeValue(TextPattern.StrikethroughStyleAttribute),
                    "horizontaltextalignment" => textRange.GetAttributeValue(TextPattern.HorizontalTextAlignmentAttribute),
                    "culture" => textRange.GetAttributeValue(TextPattern.CultureAttribute),
                    "isreadonly" => textRange.GetAttributeValue(TextPattern.IsReadOnlyAttribute),
                    "ishidden" => textRange.GetAttributeValue(TextPattern.IsHiddenAttribute),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        private TextAttributeRange GetAttributesForRange(TextPatternRange textRange, int startIndex, int length)
        {
            var range = new TextAttributeRange
            {
                StartIndex = startIndex,
                EndIndex = startIndex + length,
                Length = length,
                Text = textRange.GetText(length),
                BoundingRectangle = new BoundingRectangle()
            };

            try
            {
                // Get bounding rectangle
                var rects = textRange.GetBoundingRectangles();
                if (rects?.Length > 0)
                {
                    range.BoundingRectangle = new BoundingRectangle
                    {
                        X = rects[0].X,
                        Y = rects[0].Y,
                        Width = rects[0].Width,
                        Height = rects[0].Height
                    };
                }

                // Get font and style attributes
                range.FontName = GetAttributeValue<string>(textRange, TextPattern.FontNameAttribute);
                range.FontSize = GetAttributeValue<double>(textRange, TextPattern.FontSizeAttribute);
                range.FontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute)?.ToString();
                range.IsItalic = GetAttributeValue<bool>(textRange, TextPattern.IsItalicAttribute);
                
                // Color attributes
                var foregroundColor = GetAttributeValue<int>(textRange, TextPattern.ForegroundColorAttribute);
                if (foregroundColor != 0)
                {
                    range.ForegroundColor = $"#{foregroundColor:X6}";
                }
                
                var backgroundColor = GetAttributeValue<int>(textRange, TextPattern.BackgroundColorAttribute);
                if (backgroundColor != 0)
                {
                    range.BackgroundColor = $"#{backgroundColor:X6}";
                }

                // Text decoration
                var underlineStyle = GetAttributeValue<object>(textRange, TextPattern.UnderlineStyleAttribute);
                range.IsUnderline = underlineStyle != null && !underlineStyle.ToString()!.Equals("None", StringComparison.OrdinalIgnoreCase);
                
                var strikethroughStyle = GetAttributeValue<object>(textRange, TextPattern.StrikethroughStyleAttribute);
                range.IsStrikethrough = strikethroughStyle != null && !strikethroughStyle.ToString()!.Equals("None", StringComparison.OrdinalIgnoreCase);

                // Bold detection from FontWeight
                var fontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute);
                if (fontWeight != null)
                {
                    var weightStr = fontWeight.ToString();
                    range.IsBold = weightStr?.Contains("Bold") == true || weightStr?.Contains("Heavy") == true;
                }

                // Add all attributes to dictionary for comprehensive access
                foreach (var attr in GetSupportedAttributes())
                {
                    var value = GetSpecificAttribute(textRange, attr);
                    if (value != null)
                    {
                        // Convert complex types to strings for JSON serialization
                        range.Attributes[attr] = ConvertToJsonSafeValue(value);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with partial data
                _logger?.LogWarning(ex, "Failed to get some text attributes");
            }

            return range;
        }

        private T GetAttributeValue<T>(TextPatternRange textRange, AutomationTextAttribute attribute)
        {
            try
            {
                var value = textRange.GetAttributeValue(attribute);
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
            catch
            {
                return default(T)!;
            }
        }

        private object ConvertToJsonSafeValue(object value)
        {
            if (value == null) return null;
            
            // Handle common UI Automation types that aren't JSON serializable
            switch (value)
            {
                case System.Windows.Automation.Text.TextDecorationLineStyle lineStyle:
                    return lineStyle.ToString();
                case System.Windows.FontWeight fontWeight:
                    return fontWeight.ToString();
                case System.Windows.FontStyle fontStyle:
                    return fontStyle.ToString();
                case System.Globalization.CultureInfo culture:
                    return culture.Name;
                case bool boolValue:
                    return boolValue;
                case int intValue:
                    return intValue;
                case double doubleValue:
                    return doubleValue;
                case string stringValue:
                    return stringValue;
                default:
                    // For unknown types, convert to string
                    return value.ToString() ?? "";
            }
        }

        private TextRangeAttributes ConvertToTextRangeAttributes(TextAttributeRange range)
        {
            return new TextRangeAttributes
            {
                FontName = range.FontName,
                FontSize = range.FontSize,
                FontWeight = range.FontWeight,
                FontStyle = range.IsItalic ? "Italic" : "Normal",
                TextColor = range.ForegroundColor,
                BackgroundColor = range.BackgroundColor,
                IsItalic = range.IsItalic,
                IsBold = range.IsBold,
                IsUnderline = range.IsUnderline,
                IsStrikethrough = range.IsStrikethrough,
                Text = range.Text,
                BoundingRectangle = range.BoundingRectangle,
                Attributes = range.Attributes
            };
        }
    }
}