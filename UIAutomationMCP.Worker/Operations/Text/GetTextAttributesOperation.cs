using System.Windows.Automation;
using System.Windows.Automation.Text;
using Microsoft.Extensions.Logging;
using UIAutomationMCP.Models;
using UIAutomationMCP.Models.Results;
using UIAutomationMCP.Models.Requests;
using UIAutomationMCP.Common.Abstractions;
using UIAutomationMCP.Common.Services;
using UIAutomationMCP.Core.Exceptions;

namespace UIAutomationMCP.Worker.Operations.Text
{
    public class GetTextAttributesOperation : BaseUIAutomationOperation<GetTextAttributesRequest, TextAttributesResult>
    {
        public GetTextAttributesOperation(
            ElementFinderService elementFinderService,
            ILogger<GetTextAttributesOperation> logger)
            : base(elementFinderService, logger)
        {
        }

        protected override Core.Validation.ValidationResult ValidateRequest(GetTextAttributesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
            {
                return Core.Validation.ValidationResult.Failure("Either AutomationId or Name is required");
            }

            return Core.Validation.ValidationResult.Success;
        }

        protected override Task<TextAttributesResult> ExecuteOperationAsync(GetTextAttributesRequest request)
        {
            try
            {
                var searchCriteria = new ElementSearchCriteria
                {
                    AutomationId = request.AutomationId,
                    Name = request.Name,
                    ControlType = request.ControlType,
                    ProcessId = request.ProcessId
                };
                var element = _elementFinderService.FindElement(searchCriteria);
                
                if (element == null)
                {
                    throw new UIAutomationElementNotFoundException("Operation", null, "Element not found");
                }

                // Check if element supports TextPattern before proceeding
                if (!IsTextPatternSupported(element))
                {
                    _logger.LogWarning("Element does not support TextPattern: {AutomationId}", request.AutomationId);
                    throw new UIAutomationElementNotFoundException("Operation", null, "Element does not support TextPattern - text attributes can only be retrieved from text controls (TextBox, RichTextBox, Document, etc.)");
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    throw new UIAutomationElementNotFoundException("Operation", null, "Failed to obtain TextPattern from element");
                }

                var result = ExtractTextAttributes(textPattern, request);
                return Task.FromResult(result);
            }
            catch (ElementNotAvailableException ex)
            {
                _logger.LogWarning(ex, "Element not available during text attributes operation: {AutomationId}", request?.AutomationId);
                throw new UIAutomationElementNotFoundException("Operation", null, "Element is no longer available (may have been closed or destroyed)");
            }
            catch (ElementNotEnabledException ex)
            {
                _logger.LogWarning(ex, "Element not enabled during text attributes operation: {AutomationId}", request?.AutomationId);
                throw new UIAutomationElementNotFoundException("Operation", null, "Element is disabled and cannot be accessed");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation during text attributes retrieval: {AutomationId}", request?.AutomationId);
                throw new UIAutomationElementNotFoundException("Operation", null, $"Text attributes operation is not valid for this element: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument in text attributes operation: {AutomationId}", request?.AutomationId);
                throw new UIAutomationElementNotFoundException("Operation", null, $"Invalid parameter: {ex.Message}");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                _logger.LogError(ex, "COM error during text attributes operation: HRESULT=0x{HResult:X8}", ex.HResult);
                var errorMessage = GetCOMErrorMessage(ex);
                throw new UIAutomationElementNotFoundException("Operation", null, errorMessage);
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout during text attributes operation: {AutomationId}", request?.AutomationId);
                throw new UIAutomationElementNotFoundException("Operation", null, "Operation timed out - target application may be unresponsive");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied during text attributes operation: {AutomationId}", request?.AutomationId);
                throw new UIAutomationElementNotFoundException("Operation", null, "Access denied - insufficient permissions to access the element");
            }
        }

        private static bool IsTextPatternSupported(AutomationElement element)
        {
            try
            {
                return (bool)element.GetCurrentPropertyValue(AutomationElement.IsTextPatternAvailableProperty, false);
            }
            catch
            {
                return false;
            }
        }

        private static string GetCOMErrorMessage(System.Runtime.InteropServices.COMException ex)
        {
            return ex.HResult switch
            {
                unchecked((int)0x80070005) => "Access denied - the target element may be in a different security context or require elevated privileges",
                unchecked((int)0x80040154) => "UI Automation service not available - component may not be registered",
                unchecked((int)0x800706BE) => "Remote procedure call failed - target application may have closed or become unresponsive",
                unchecked((int)0x80004002) => "Interface not supported - the element may not support text operations",
                unchecked((int)0x8000FFFF) => "Unexpected system error - target application may be in an unstable state",
                unchecked((int)0x80070057) => "Invalid parameter passed to UI Automation",
                unchecked((int)0x8007000E) => "Out of memory - system resources may be low",
                _ => $"UI Automation COM error (HRESULT: 0x{ex.HResult:X8}): {ex.Message}"
            };
        }


        private TextAttributesResult ExtractTextAttributes(TextPattern textPattern, GetTextAttributesRequest request)
        {
            var documentRange = textPattern.DocumentRange ?? throw new InvalidOperationException("Cannot access text content - element may not contain text");

            var fullText = documentRange.GetText(-1);
            var (startIndex, endIndex) = CalculateTextRange(fullText, request.StartIndex, request.Length);
            
            if (startIndex >= fullText.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Start index is beyond text length");
            }

            var textRange = CreateTextRange(documentRange, startIndex, endIndex - startIndex);
            
            // Check if any attributes have mixed values that would benefit from segmentation
            bool shouldSegment = ShouldUseSegmentation(textRange);
            
            var result = new TextAttributesResult
            {
                Success = true,
                AutomationId = request.AutomationId ?? "",
                Name = request.Name ?? "",
                ControlType = request.ControlType ?? "",
                ProcessId = request.ProcessId ?? 0,
                StartPosition = startIndex,
                EndPosition = endIndex,
                TextContent = fullText[startIndex..endIndex],
                HasAttributes = true,
                Pattern = "TextPattern",
                SegmentationMode = shouldSegment.ToString(),
                SupportedAttributes = GetSupportedAttributes()
            };

            if (shouldSegment)
            {
                // Use Microsoft Learn recommended approach: segment by format boundaries
                result.TextSegments = ExtractTextSegments(textRange, startIndex);
                _logger.LogInformation("Used segmentation mode due to mixed attributes");
            }
            else
            {
                // Use traditional approach for uniform attributes
                result.TextAttributes = GetTextAttributes(textRange);
                _logger.LogInformation("Used traditional mode for uniform attributes");
            }

            return result;
        }

        private static (int startIndex, int endIndex) CalculateTextRange(string fullText, int requestedStart, int requestedLength)
        {
            var startIndex = Math.Max(0, requestedStart);
            var length = requestedLength == -1 ? fullText.Length - startIndex : requestedLength;
            var endIndex = Math.Min(fullText.Length, startIndex + length);
            return (startIndex, endIndex);
        }

        private static TextPatternRange CreateTextRange(TextPatternRange documentRange, int startIndex, int length)
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
                attributes.FontSize = GetAttributeValue<double?>(textRange, TextPattern.FontSizeAttribute) ?? 0.0;
                attributes.FontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute)?.ToString() ?? string.Empty;
                attributes.IsItalic = GetAttributeValue<bool?>(textRange, TextPattern.IsItalicAttribute) ?? false;
                
                // Colors - Microsoft Learn recommends checking for 0 as "not set"
                var foregroundColor = GetAttributeValue<int>(textRange, TextPattern.ForegroundColorAttribute);
                if (foregroundColor != 0)
                    attributes.ForegroundColor = $"#{foregroundColor:X6}";
                
                var backgroundColor = GetAttributeValue<int>(textRange, TextPattern.BackgroundColorAttribute);
                if (backgroundColor != 0)
                    attributes.BackgroundColor = $"#{backgroundColor:X6}";

                // Bold detection using FontWeight
                var fontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute);
                attributes.IsBold = DetermineBoldFromFontWeight(fontWeight);

                // Italic
                attributes.IsItalic = GetAttributeValue<bool?>(textRange, TextPattern.IsItalicAttribute) ?? false;

                // Underline
                var underlineStyleValue = GetAttributeValue<object>(textRange, TextPattern.UnderlineStyleAttribute);
                attributes.IsUnderline = underlineStyleValue != null && !string.Equals(underlineStyleValue.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.UnderlineStyle = underlineStyleValue?.ToString() ?? string.Empty;

                var strikethroughStyle = GetAttributeValue<object>(textRange, TextPattern.StrikethroughStyleAttribute);
                attributes.IsStrikethrough = strikethroughStyle != null && !string.Equals(strikethroughStyle.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.StrikethroughStyle = strikethroughStyle?.ToString() ?? string.Empty;

                // Other attributes
                attributes.HorizontalTextAlignment = GetAttributeValue<object>(textRange, TextPattern.HorizontalTextAlignmentAttribute)?.ToString() ?? string.Empty;
                attributes.Culture = GetAttributeValue<System.Globalization.CultureInfo>(textRange, TextPattern.CultureAttribute)?.Name ?? string.Empty;
                attributes.IsReadOnly = GetAttributeValue<bool?>(textRange, TextPattern.IsReadOnlyAttribute) ?? false;
                attributes.IsHidden = GetAttributeValue<bool?>(textRange, TextPattern.IsHiddenAttribute) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get some text attributes");
            }

            return attributes;
        }

        private bool DetermineBoldFromFontWeight(object? fontWeight)
        {
            _logger.LogDebug("DetermineBoldFromFontWeight called with: {FontWeight} (Type: {Type})", fontWeight?.ToString(), fontWeight?.GetType().Name);
            
            if (fontWeight == null) 
            {
                _logger.LogDebug("FontWeight is null, returning false");
                return false;
            }

            var weightStr = fontWeight.ToString() ?? "";
            _logger.LogDebug("FontWeight string: '{WeightStr}'", weightStr);
            
            // Check for numeric values (700+ is bold as per Microsoft standards)
            if (int.TryParse(weightStr, out var numericWeight))
            {
                _logger.LogDebug("Parsed numeric weight: {NumericWeight}, isBold: {IsBold}", numericWeight, numericWeight >= 700);
                return numericWeight >= 700;
            }
            
            // Check for string patterns
            var isBold = weightStr.Contains("Bold", StringComparison.OrdinalIgnoreCase) || 
                   weightStr.Contains("Heavy", StringComparison.OrdinalIgnoreCase) ||
                   weightStr.Contains("700") || weightStr.Contains("800") || weightStr.Contains("900");
            
            _logger.LogDebug("String pattern check result: {IsBold}", isBold);
            return isBold;
        }

        private static List<string> GetSupportedAttributes()
        {
            return [
                "FontName", "FontSize", "FontWeight", "IsItalic", "IsBold",
                "ForegroundColor", "BackgroundColor", "IsUnderline", "IsStrikethrough", 
                "HorizontalTextAlignment", "Culture", "IsReadOnly", "IsHidden"
            ];
        }

        private object? GetRawAttributeValue(TextPatternRange textRange, AutomationTextAttribute attribute)
        {
            try
            {
                var value = textRange.GetAttributeValue(attribute);
                _logger.LogDebug("Raw Attribute {Attribute}: Value={Value}, Type={Type}, IsMixed={IsMixed}, IsNotSupported={IsNotSupported}", 
                    attribute.ProgrammaticName, 
                    value?.ToString(), 
                    value?.GetType().Name,
                    ReferenceEquals(value, TextPattern.MixedAttributeValue),
                    ReferenceEquals(value, AutomationElement.NotSupported));
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to get raw attribute {Attribute}", attribute.ProgrammaticName);
                return null;
            }
        }

        private T GetAttributeValue<T>(TextPatternRange textRange, AutomationTextAttribute attribute)
        {
            try
            {
                var value = textRange.GetAttributeValue(attribute);
                
                // Log for debugging
                _logger.LogDebug("Attribute {Attribute}: Value={Value}, Type={Type}, IsMixed={IsMixed}, IsNotSupported={IsNotSupported}", 
                    attribute.ProgrammaticName, 
                    value?.ToString(), 
                    value?.GetType().Name,
                    ReferenceEquals(value, TextPattern.MixedAttributeValue),
                    ReferenceEquals(value, AutomationElement.NotSupported));
                
                // Handle special cases like MixedAttributeValue and NotSupported
                if (ReferenceEquals(value, TextPattern.MixedAttributeValue) || 
                    ReferenceEquals(value, AutomationElement.NotSupported))
                {
                    return default!;
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
                
                return default!;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to get attribute {Attribute}", attribute.ProgrammaticName);
                return default!;
            }
        }

        private bool ShouldUseSegmentation(TextPatternRange textRange)
        {
            try
            {
                // Check if any key attributes have mixed values
                var attributes = new[]
                {
                    TextPattern.FontNameAttribute,
                    TextPattern.FontSizeAttribute,
                    TextPattern.FontWeightAttribute,
                    TextPattern.IsItalicAttribute,
                    TextPattern.ForegroundColorAttribute,
                    TextPattern.BackgroundColorAttribute,
                    TextPattern.UnderlineStyleAttribute
                };

                foreach (var attribute in attributes)
                {
                    var value = GetRawAttributeValue(textRange, attribute);
                    if (ReferenceEquals(value, TextPattern.MixedAttributeValue))
                    {
                        _logger.LogDebug("Found mixed attribute: {Attribute}", attribute.ProgrammaticName);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking for mixed attributes, defaulting to segmentation");
                return true; // Default to segmentation on error
            }
        }

        private List<TextSegment> ExtractTextSegments(TextPatternRange textRange, int globalStartIndex)
        {
            var segments = new List<TextSegment>();
            
            try
            {
                var fullText = textRange.GetText(-1);
                
                if (string.IsNullOrEmpty(fullText))
                {
                    return segments;
                }

                // Use Microsoft Learn recommended approach: Format boundary detection
                var currentRange = textRange.Clone();
                var totalProcessed = 0;

                while (totalProcessed < fullText.Length)
                {
                    // Get attributes for current position
                    var segmentAttributes = GetSegmentAttributes(currentRange);
                    
                    // Find the end of current format by expanding until attributes change
                    var formatRange = currentRange.Clone();
                    formatRange.ExpandToEnclosingUnit(TextUnit.Format);
                    
                    var formatText = formatRange.GetText(-1);
                    var segmentLength = Math.Min(formatText.Length, fullText.Length - totalProcessed);
                    
                    if (segmentLength <= 0)
                    {
                        _logger.LogWarning("Invalid segment length {Length} at position {Position}", segmentLength, totalProcessed);
                        break;
                    }
                    
                    var segmentText = fullText.Substring(totalProcessed, segmentLength);
                    
                    var segment = new TextSegment
                    {
                        StartPosition = globalStartIndex + totalProcessed,
                        EndPosition = globalStartIndex + totalProcessed + segmentLength,
                        Text = segmentText,
                        Attributes = segmentAttributes
                    };
                    
                    segments.Add(segment);
                    _logger.LogDebug("Created format segment: [{Start}-{End}] '{Text}' Bold={Bold} Italic={Italic}", 
                        segment.StartPosition, segment.EndPosition, 
                        segmentText, 
                        segmentAttributes.IsBold, 
                        segmentAttributes.IsItalic);
                    
                    // Move to next format boundary
                    totalProcessed += segmentLength;
                    
                    if (totalProcessed < fullText.Length)
                    {
                        // Move range to next position
                        currentRange.Move(TextUnit.Character, segmentLength);
                        currentRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, 1);
                    }
                }
                
                _logger.LogDebug("Extracted {Count} format-based segments from text", segments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text segments");
                // Return a single segment with basic attributes as fallback
                var fallbackAttributes = new SegmentAttributes();
                var fallbackText = textRange.GetText(-1);
                segments.Add(new TextSegment
                {
                    StartPosition = globalStartIndex,
                    EndPosition = globalStartIndex + fallbackText.Length,
                    Text = fallbackText,
                    Attributes = fallbackAttributes
                });
            }
            
            return segments;
        }

        private SegmentAttributes GetSegmentAttributes(TextPatternRange textRange)
        {
            var attributes = new SegmentAttributes();

            try
            {
                // Font attributes
                attributes.FontName = GetAttributeValue<string>(textRange, TextPattern.FontNameAttribute);
                attributes.FontSize = GetAttributeValue<double?>(textRange, TextPattern.FontSizeAttribute) ?? 0.0;
                attributes.FontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute)?.ToString() ?? string.Empty;
                attributes.IsItalic = GetAttributeValue<bool?>(textRange, TextPattern.IsItalicAttribute) ?? false;
                
                // Colors - Microsoft Learn recommends checking for 0 as "not set"
                var foregroundColor = GetAttributeValue<int>(textRange, TextPattern.ForegroundColorAttribute);
                if (foregroundColor != 0)
                    attributes.ForegroundColor = $"#{foregroundColor:X6}";
                
                var backgroundColor = GetAttributeValue<int>(textRange, TextPattern.BackgroundColorAttribute);
                if (backgroundColor != 0)
                    attributes.BackgroundColor = $"#{backgroundColor:X6}";

                // Bold detection - no mixed values in segments
                var fontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute);
                attributes.IsBold = DetermineBoldFromFontWeight(fontWeight);

                // Underline
                var underlineStyleValue = GetAttributeValue<object>(textRange, TextPattern.UnderlineStyleAttribute);
                attributes.IsUnderline = underlineStyleValue != null && 
                    !string.Equals(underlineStyleValue.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.UnderlineStyle = underlineStyleValue?.ToString() ?? string.Empty;

                // Strikethrough
                var strikethroughStyle = GetAttributeValue<object>(textRange, TextPattern.StrikethroughStyleAttribute);
                attributes.IsStrikethrough = strikethroughStyle != null && 
                    !string.Equals(strikethroughStyle.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.StrikethroughStyle = strikethroughStyle?.ToString() ?? string.Empty;

                // Other attributes
                attributes.HorizontalTextAlignment = GetAttributeValue<object>(textRange, TextPattern.HorizontalTextAlignmentAttribute)?.ToString() ?? string.Empty;
                attributes.Culture = GetAttributeValue<System.Globalization.CultureInfo>(textRange, TextPattern.CultureAttribute)?.Name ?? string.Empty;
                attributes.IsReadOnly = GetAttributeValue<bool?>(textRange, TextPattern.IsReadOnlyAttribute) ?? false;
                attributes.IsHidden = GetAttributeValue<bool?>(textRange, TextPattern.IsHiddenAttribute) ?? false;
                attributes.IsSubscript = GetAttributeValue<bool?>(textRange, TextPattern.IsSubscriptAttribute) ?? false;
                attributes.IsSuperscript = GetAttributeValue<bool?>(textRange, TextPattern.IsSuperscriptAttribute) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get some segment attributes");
            }

            return attributes;
        }


    }
}