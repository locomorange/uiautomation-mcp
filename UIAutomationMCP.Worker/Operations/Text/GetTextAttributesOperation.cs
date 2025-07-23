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
            GetTextAttributesRequest? request = null;
            try
            {
                request = JsonSerializationHelper.Deserialize<GetTextAttributesRequest>(parametersJson)!;
                
                // Validate required parameters early
                if (string.IsNullOrWhiteSpace(request.AutomationId) && string.IsNullOrWhiteSpace(request.Name))
                {
                    return Task.FromResult(CreateErrorResult(request, "Either AutomationId or Name is required"));
                }
                
                var element = _elementFinderService.FindElement(
                    automationId: request.AutomationId, 
                    name: request.Name,
                    controlType: request.ControlType,
                    processId: request.ProcessId ?? 0);
                
                if (element == null)
                {
                    return Task.FromResult(CreateErrorResult(request, "Element not found"));
                }

                // Check if element supports TextPattern before proceeding
                if (!IsTextPatternSupported(element))
                {
                    _logger.LogWarning("Element does not support TextPattern: {AutomationId}", request.AutomationId);
                    return Task.FromResult(CreateErrorResult(request, 
                        "Element does not support TextPattern - text attributes can only be retrieved from text controls (TextBox, RichTextBox, Document, etc.)"));
                }

                if (!element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern) || pattern is not TextPattern textPattern)
                {
                    return Task.FromResult(CreateErrorResult(request, "Failed to obtain TextPattern from element"));
                }

                var result = ExtractTextAttributes(textPattern, request);
                return Task.FromResult(new OperationResult { Success = true, Data = result });
            }
            catch (ElementNotAvailableException ex)
            {
                // Element no longer exists (e.g., dialog closed, application terminated)
                _logger.LogWarning(ex, "Element not available during text attributes operation: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, "Element is no longer available (may have been closed or destroyed)"));
            }
            catch (ElementNotEnabledException ex)
            {
                // Element exists but is disabled
                _logger.LogWarning(ex, "Element not enabled during text attributes operation: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, "Element is disabled and cannot be accessed"));
            }
            catch (InvalidOperationException ex)
            {
                // TextPattern operations may not be valid for current element state
                _logger.LogError(ex, "Invalid operation during text attributes retrieval: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, $"Text attributes operation is not valid for this element: {ex.Message}"));
            }
            catch (ArgumentException ex)
            {
                // Invalid parameters (e.g., range out of bounds)
                _logger.LogError(ex, "Invalid argument in text attributes operation: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, $"Invalid parameter: {ex.Message}"));
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                // COM interop errors - common in UI Automation when accessing system-level elements
                _logger.LogError(ex, "COM error during text attributes operation: HRESULT=0x{HResult:X8}", ex.HResult);
                var errorMessage = GetCOMErrorMessage(ex);
                return Task.FromResult(CreateErrorResult(request, errorMessage));
            }
            catch (TimeoutException ex)
            {
                // Operation timed out
                _logger.LogError(ex, "Timeout during text attributes operation: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, "Operation timed out - target application may be unresponsive"));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Security/permission issues
                _logger.LogError(ex, "Access denied during text attributes operation: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, "Access denied - insufficient permissions to access the element"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetTextAttributes operation: {AutomationId}", request?.AutomationId);
                return Task.FromResult(CreateErrorResult(request, $"Unexpected error: {ex.Message}"));
            }
        }

        private bool IsTextPatternSupported(AutomationElement element)
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

        private string GetCOMErrorMessage(System.Runtime.InteropServices.COMException ex)
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
                TextContent = fullText.Substring(startIndex, endIndex - startIndex),
                HasAttributes = true,
                Pattern = "TextPattern",
                SegmentationMode = shouldSegment,
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

                // Bold detection using FontWeight
                var fontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute);
                attributes.IsBold = DetermineBoldFromFontWeight(fontWeight);

                // Italic
                attributes.IsItalic = GetAttributeValue<bool?>(textRange, TextPattern.IsItalicAttribute);

                // Underline
                var underlineStyleValue = GetAttributeValue<object>(textRange, TextPattern.UnderlineStyleAttribute);
                attributes.IsUnderline = underlineStyleValue != null && !string.Equals(underlineStyleValue.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.UnderlineStyle = underlineStyleValue?.ToString();

                var strikethroughStyle = GetAttributeValue<object>(textRange, TextPattern.StrikethroughStyleAttribute);
                attributes.IsStrikethrough = strikethroughStyle != null && !string.Equals(strikethroughStyle.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.StrikethroughStyle = strikethroughStyle?.ToString();

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

        private List<string> GetSupportedAttributes()
        {
            return new List<string>
            {
                "FontName", "FontSize", "FontWeight", "IsItalic", "IsBold",
                "ForegroundColor", "BackgroundColor", "IsUnderline", "IsStrikethrough", 
                "HorizontalTextAlignment", "Culture", "IsReadOnly", "IsHidden"
            };
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

                SegmentAttributes? currentAttributes = null;
                var segmentStartPos = 0;

                // Check each character to detect attribute changes
                for (int i = 0; i < fullText.Length; i++)
                {
                    // Create a range for the current character
                    var charRange = textRange.Clone();
                    charRange.Move(TextUnit.Character, i);
                    charRange.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, 1);
                    
                    var charAttributes = GetSegmentAttributes(charRange);
                    
                    // If this is the first character or attributes changed, start a new segment
                    if (currentAttributes == null || !AreAttributesEqual(currentAttributes, charAttributes))
                    {
                        // Save previous segment if it exists
                        if (currentAttributes != null && i > segmentStartPos)
                        {
                            var segmentText = fullText.Substring(segmentStartPos, i - segmentStartPos);
                            var segment = new TextSegment
                            {
                                StartPosition = globalStartIndex + segmentStartPos,
                                EndPosition = globalStartIndex + i,
                                Text = segmentText,
                                Attributes = currentAttributes
                            };
                            
                            segments.Add(segment);
                            _logger.LogDebug("Created segment: [{Start}-{End}] '{Text}' Bold={Bold} Italic={Italic}", 
                                segment.StartPosition, segment.EndPosition, 
                                segmentText, 
                                currentAttributes.IsBold, 
                                currentAttributes.IsItalic);
                        }
                        
                        // Start new segment
                        currentAttributes = charAttributes;
                        segmentStartPos = i;
                    }
                }

                // Add the final segment
                if (currentAttributes != null && segmentStartPos < fullText.Length)
                {
                    var segmentText = fullText.Substring(segmentStartPos);
                    var segment = new TextSegment
                    {
                        StartPosition = globalStartIndex + segmentStartPos,
                        EndPosition = globalStartIndex + fullText.Length,
                        Text = segmentText,
                        Attributes = currentAttributes
                    };
                    
                    segments.Add(segment);
                    _logger.LogDebug("Created final segment: [{Start}-{End}] '{Text}' Bold={Bold} Italic={Italic}", 
                        segment.StartPosition, segment.EndPosition, 
                        segmentText, 
                        currentAttributes.IsBold, 
                        currentAttributes.IsItalic);
                }
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

                // Bold detection - no mixed values in segments
                var fontWeight = GetAttributeValue<object>(textRange, TextPattern.FontWeightAttribute);
                attributes.IsBold = DetermineBoldFromFontWeight(fontWeight);

                // Underline
                var underlineStyleValue = GetAttributeValue<object>(textRange, TextPattern.UnderlineStyleAttribute);
                attributes.IsUnderline = underlineStyleValue != null && 
                    !string.Equals(underlineStyleValue.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.UnderlineStyle = underlineStyleValue?.ToString();

                // Strikethrough
                var strikethroughStyle = GetAttributeValue<object>(textRange, TextPattern.StrikethroughStyleAttribute);
                attributes.IsStrikethrough = strikethroughStyle != null && 
                    !string.Equals(strikethroughStyle.ToString(), "None", StringComparison.OrdinalIgnoreCase);
                attributes.StrikethroughStyle = strikethroughStyle?.ToString();

                // Other attributes
                attributes.HorizontalTextAlignment = GetAttributeValue<object>(textRange, TextPattern.HorizontalTextAlignmentAttribute)?.ToString();
                attributes.Culture = GetAttributeValue<System.Globalization.CultureInfo>(textRange, TextPattern.CultureAttribute)?.Name;
                attributes.IsReadOnly = GetAttributeValue<bool?>(textRange, TextPattern.IsReadOnlyAttribute);
                attributes.IsHidden = GetAttributeValue<bool?>(textRange, TextPattern.IsHiddenAttribute);
                attributes.IsSubscript = GetAttributeValue<bool?>(textRange, TextPattern.IsSubscriptAttribute);
                attributes.IsSuperscript = GetAttributeValue<bool?>(textRange, TextPattern.IsSuperscriptAttribute);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get some segment attributes");
            }

            return attributes;
        }

        private bool AreAttributesEqual(SegmentAttributes attr1, SegmentAttributes attr2)
        {
            return attr1.FontName == attr2.FontName &&
                   attr1.FontSize == attr2.FontSize &&
                   attr1.FontWeight == attr2.FontWeight &&
                   attr1.IsItalic == attr2.IsItalic &&
                   attr1.IsBold == attr2.IsBold &&
                   attr1.ForegroundColor == attr2.ForegroundColor &&
                   attr1.BackgroundColor == attr2.BackgroundColor &&
                   attr1.IsUnderline == attr2.IsUnderline &&
                   attr1.UnderlineStyle == attr2.UnderlineStyle &&
                   attr1.IsStrikethrough == attr2.IsStrikethrough &&
                   attr1.StrikethroughStyle == attr2.StrikethroughStyle &&
                   attr1.HorizontalTextAlignment == attr2.HorizontalTextAlignment &&
                   attr1.Culture == attr2.Culture;
        }

    }
}